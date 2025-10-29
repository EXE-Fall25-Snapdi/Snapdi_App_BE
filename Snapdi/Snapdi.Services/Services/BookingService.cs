using Microsoft.Extensions.Logging;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;

namespace Snapdi.Services.Services
{
    public class BookingService : Interfaces.IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBookingStatusRepository _statusRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentStatusRepository _paymentStatusRepo;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepo,
            IUserRepository userRepo,
            IBookingStatusRepository statusRepo,
            IPaymentRepository paymentRepo,
            IPaymentStatusRepository paymentStatusRepo,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
            _paymentRepo = paymentRepo;
            _paymentStatusRepo = paymentStatusRepo;
            _logger = logger;
        }

        /// <summary>
        /// Process PayOS payment callback for booking
        /// </summary>
        public async Task<bool> ProcessPaymentCallbackAsync(PaymentResponseModel paymentResponse)
        {
            try
            {
                if (!paymentResponse.Success || paymentResponse.BookingId <= 0)
                {
                    _logger.LogWarning("Invalid payment response received for booking");
                    return false;
                }

                var booking = await _bookingRepo.GetBookingWithDetailsAsync(paymentResponse.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for ID: {BookingId}", paymentResponse.BookingId);
                    return false;
                }

                // Check if booking is in a valid state for payment processing
                var validStatuses = new[] { "Pending", "Confirmed" };
                if (booking.Status == null || !validStatuses.Contains(booking.Status.StatusName))
                {
                    _logger.LogWarning("Booking {BookingId} is not in valid status for payment. Current status: {Status}",
                        paymentResponse.BookingId, booking.Status?.StatusName ?? "Unknown");
                    return false;
                }

                // Get required payment and booking statuses
                var paymentStatus = await GetPaymentStatusByName(paymentResponse.Success ? "Paid" : "Failed");
                var bookingStatus = await GetBookingStatusByName(paymentResponse.Success ? "Confirmed" : "Cancelled");

                if (paymentStatus == null || bookingStatus == null)
                {
                    _logger.LogError("Required payment or booking status not found");
                    return false;
                }

                // Create or update payment record
                await CreateOrUpdatePaymentRecord(booking, paymentResponse, paymentStatus);

                // Update booking status based on payment result
                if (paymentResponse.Success)
                {
                    // Payment successful - confirm booking
                    booking.StatusId = bookingStatus.StatusId;
                    await _bookingRepo.UpdateAsync(booking);
                    await _bookingRepo.SaveChangesAsync();

                    _logger.LogInformation("Booking {BookingId} confirmed after successful payment", booking.BookingId);
                }
                else
                {
                    // Payment failed - cancel booking
                    booking.StatusId = bookingStatus.StatusId;
                    await _bookingRepo.UpdateAsync(booking);
                    await _bookingRepo.SaveChangesAsync();

                    _logger.LogInformation("Booking {BookingId} cancelled due to payment failure", booking.BookingId);
                }

                _logger.LogInformation("Payment processed successfully for booking {BookingId} with status {PaymentStatus}",
                    booking.BookingId, paymentResponse.Success ? "Success" : "Failed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for booking {BookingId}", paymentResponse.BookingId);
                return false;
            }
        }

        /// <summary>
        /// Confirm manual payment for booking
        /// </summary>
        public async Task<bool> ConfirmManualPaymentAsync(int bookingId, int feePolicyId, int userId)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
                    return false;
                }

                // Validate user has permission (customer or admin)
                if (booking.CustomerId != userId)
                {
                    _logger.LogWarning("User {UserId} does not have permission to confirm payment for booking {BookingId}", userId, bookingId);
                    return false;
                }

                // Get payment and booking statuses
                var pendingPaymentStatus = await GetPaymentStatusByName("Pending");
                var confirmedBookingStatus = await GetBookingStatusByName("Confirmed");

                if (pendingPaymentStatus == null || confirmedBookingStatus == null)
                {
                    _logger.LogError("Required statuses not found for manual payment confirmation");
                    return false;
                }

                // Calculate payment amounts (20% of booking price)
                var amountCustomerPay = Math.Round(booking.Price * 20 / 100);

                // Create payment record for manual payment
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = amountCustomerPay,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    FeePolicyId = feePolicyId,
                    PaymentDate = DateTime.UtcNow,
                    TransactionMethod = "Manual"
                };

                await _paymentRepo.AddAsync(payment);
                await _paymentRepo.SaveChangesAsync();

                // Update booking status to confirmed
                booking.StatusId = confirmedBookingStatus.StatusId;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation("Manual payment confirmed for booking {BookingId} by user {UserId}", bookingId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming manual payment for booking {BookingId}", bookingId);
                return false;
            }
        }

        /// <summary>
        /// Mark payment as paid for booking
        /// </summary>
        public async Task<bool> MarkPaymentAsPaidAsync(int bookingId, int paymentId, int userId)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
                    return false;
                }

                var payment = await _paymentRepo.GetByIdAsync(paymentId);
                if (payment == null || payment.BookingId != bookingId)
                {
                    _logger.LogWarning("Payment {PaymentId} not found or does not belong to booking {BookingId}", paymentId, bookingId);
                    return false;
                }

                // Get required statuses
                var paidPaymentStatus = await GetPaymentStatusByName("Paid");
                var confirmedBookingStatus = await GetBookingStatusByName("Confirmed");

                if (paidPaymentStatus == null || confirmedBookingStatus == null)
                {
                    _logger.LogError("Required statuses not found for marking payment as paid");
                    return false;
                }

                // Update payment status
                payment.PaymentStatusId = paidPaymentStatus.PaymentStatusId;
                await _paymentRepo.UpdateAsync(payment);
                await _paymentRepo.SaveChangesAsync();

                // Update booking status
                booking.StatusId = confirmedBookingStatus.StatusId;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} marked as paid for booking {BookingId}", paymentId, bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment as paid for booking {BookingId}", bookingId);
                return false;
            }
        }

        /// <summary>
        /// Cancel payment and booking
        /// </summary>
        public async Task<bool> CancelPaymentAsync(int bookingId, int paymentId, int userId)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
                    return false;
                }

                var payment = await _paymentRepo.GetByIdAsync(paymentId);
                if (payment == null || payment.BookingId != bookingId)
                {
                    _logger.LogWarning("Payment {PaymentId} not found or does not belong to booking {BookingId}", paymentId, bookingId);
                    return false;
                }

                // Get required statuses
                var refundedPaymentStatus = await GetPaymentStatusByName("Refunded");
                var cancelledBookingStatus = await GetBookingStatusByName("Cancelled");

                if (refundedPaymentStatus == null || cancelledBookingStatus == null)
                {
                    _logger.LogError("Required statuses not found for cancelling payment");
                    return false;
                }

                // Update payment status to refunded
                payment.PaymentStatusId = refundedPaymentStatus.PaymentStatusId;
                await _paymentRepo.UpdateAsync(payment);
                await _paymentRepo.SaveChangesAsync();

                // Update booking status to cancelled
                booking.StatusId = cancelledBookingStatus.StatusId;
                await _bookingRepo.UpdateAsync(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} cancelled and booking {BookingId} cancelled", paymentId, bookingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment for booking {BookingId}", bookingId);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<PaymentStatus?> GetPaymentStatusByName(string statusName)
        {
            try
            {
                return await _paymentStatusRepo.GetByNameAsync(statusName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status by name: {StatusName}", statusName);
                return null;
            }
        }

        private async Task<BookingStatus?> GetBookingStatusByName(string statusName)
        {
            try
            {
                return await _statusRepo.GetByNameAsync(statusName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking status by name: {StatusName}", statusName);
                return null;
            }
        }

        private async Task CreateOrUpdatePaymentRecord(Booking booking, PaymentResponseModel paymentResponse, PaymentStatus paymentStatus)
        {
            try
            {
                // Check if payment record already exists for this booking and order code
                var existingPayments = await _paymentRepo.GetPaymentsByBookingIdAsync(booking.BookingId);

                if (existingPayments != null)
                {
                    // Update existing payment
                    existingPayments.PaymentStatusId = paymentStatus.PaymentStatusId;
                    existingPayments.TransactionReference = paymentResponse.TransactionReference ?? paymentResponse.OrderCode.ToString();
                    await _paymentRepo.UpdateAsync(existingPayments);
                }
                else
                {
                    // Create new payment record
                    var payment = new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = paymentResponse.Amount,
                        PaymentStatusId = paymentStatus.PaymentStatusId,
                        TransactionMethod = "PayOS",
                        TransactionReference = paymentResponse.TransactionReference ?? paymentResponse.OrderCode.ToString(),
                        PaymentDate = paymentResponse.PaymentDate
                    };

                    await _paymentRepo.AddAsync(payment);
                }

                await _paymentRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating payment record for booking {BookingId}", booking.BookingId);
                throw;
            }
        }

        #endregion

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
        {
            // Validate users exist
            var customer = await _userRepo.GetByIdAsync(request.CustomerId);
            var photographer = await _userRepo.GetByIdAsync(request.PhotographerId);

            if (customer == null || photographer == null)
                throw new ArgumentException("Customer or Photographer not found");

            // Validate status
            var status = await _statusRepo.GetByNameAsync("Pending");
            if (status == null)
                throw new ArgumentException("Default status 'Pending' not found");

            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                PhotographerId = request.PhotographerId,
                ScheduleAt = request.ScheduleAt,
                LocationAddress = request.LocationAddress,
                Price = request.Price,
                StatusId = status.StatusId,
                Note = request.Note
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload booking with related entities for mapping
            var createdBooking = await _bookingRepo.GetBookingWithDetailsAsync(booking.BookingId);

            return MapToBookingResponse(createdBooking!);
        }

        public async Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, int newStatusId)
        {
            var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");

            var oldStatusId = booking.StatusId;
            var oldStatusName = booking.Status?.StatusName;

            booking.StatusId = newStatusId;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload to get updated status name
            var updatedBooking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            var response = MapToBookingResponse(updatedBooking!);

            return response;
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");
            return MapToBookingResponse(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepo.GetAllAsync();
            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByCustomerAsync(int customerId)
        {
            var bookings = await _bookingRepo.GetBookingsByCustomerAsync(customerId);
            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByPhotographerAsync(int photographerId)
        {
            var bookings = await _bookingRepo.GetBookingsByPhotographerAsync(photographerId);
            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByStatusAsync(int statusId)
        {
            var bookings = await _bookingRepo.GetBookingsByStatusAsync(statusId);
            return bookings.Select(MapToDto);
        }

        public async Task<BookingSearchResultDto> SearchBookingsAsync(BookingSearchDto searchDto)
        {
            var (bookings, totalCount) = await _bookingRepo.SearchBookingsAsync(
                searchDto.PageNumber,
                searchDto.PageSize,
                searchDto.SearchTerm,
                searchDto.CustomerId,
                searchDto.PhotographerId,
                searchDto.StatusId,
                searchDto.LocationAddress,
                searchDto.MinPrice,
                searchDto.MaxPrice,
                searchDto.ScheduleFrom,
                searchDto.ScheduleTo,
                searchDto.SortBy,
                searchDto.SortDirection
            );

            var bookingDtos = bookings.Select(MapToDto).ToList();

            return new BookingSearchResultDto
            {
                Data = bookingDtos,
                TotalRecords = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<BookingDto> CreateBookingDtoAsync(CreateBookingDto createDto)
        {
            // Validate customer exists
            var customer = await _userRepo.GetByIdAsync(createDto.CustomerId);
            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with ID {createDto.CustomerId} does not exist");
            }

            // Validate photographer exists
            var photographer = await _userRepo.GetByIdAsync(createDto.PhotographerId);
            if (photographer == null)
            {
                throw new InvalidOperationException($"Photographer with ID {createDto.PhotographerId} does not exist");
            }

            var booking = new Booking
            {
                CustomerId = createDto.CustomerId,
                PhotographerId = createDto.PhotographerId,
                ScheduleAt = createDto.ScheduleAt,
                LocationAddress = createDto.LocationAddress,
                Price = createDto.Price,
                Note = createDto.Note,
                PhotoTypeId = createDto.PhotoTypeId,
                Time = createDto.Time,
                StatusId = 1 // Default to "Pending" status (assuming StatusID 1 is Pending)
            };

            var createdBooking = await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload booking with details
            var bookingWithDetails = await _bookingRepo.GetBookingWithDetailsAsync(createdBooking.BookingId);

            return MapToDto(bookingWithDetails!);
        }

        public async Task<BookingDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return null;

            if (updateDto.ScheduleAt.HasValue)
                booking.ScheduleAt = updateDto.ScheduleAt.Value;

            if (updateDto.LocationAddress != null)
                booking.LocationAddress = updateDto.LocationAddress;

            if (updateDto.Price.HasValue)
                booking.Price = updateDto.Price.Value;

            if (updateDto.Note != null)
                booking.Note = updateDto.Note;

            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload booking with details
            var bookingWithDetails = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);

            return MapToDto(bookingWithDetails!);
        }

        public async Task<BookingDto?> UpdateBookingStatusDtoAsync(int bookingId, UpdateBookingStatusDto statusDto)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return null;

            await _bookingRepo.UpdateBookingStatusAsync(bookingId, statusDto.StatusId);
            await _bookingRepo.SaveChangesAsync();

            // Reload booking with details
            var bookingWithDetails = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);

            return MapToDto(bookingWithDetails!);
        }

        public async Task<BookingDto?> UpdatePhotoLinkAsync(int bookingId, PhotoLinkUpdateDto photoLink)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return null;

            booking.PhotoLink = photoLink.PhotoLink;
            booking.CustomerId = booking.CustomerId;

            var status = await _statusRepo.GetByNameAsync("Completed");

            booking.StatusId = status.StatusId;

            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            var bookingWithDetails = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            return MapToDto(bookingWithDetails!);
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return false;

            await _bookingRepo.DeleteAsync(booking);
            await _bookingRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BookingExistsAsync(int bookingId)
        {
            return await _bookingRepo.BookingExistsAsync(bookingId);
        }

        public async Task<Dictionary<int, int>> GetBookingsCountByStatusAsync()
        {
            return await _bookingRepo.GetBookingsCountByStatusAsync();
        }

        public async Task<BookingStatisticsResponseDto> GetBookingStatisticsAsync()
        {
            var statusCounts = await _bookingRepo.GetBookingsCountByStatusWithNamesAsync();
            var totalBookings = statusCounts.Sum(x => x.Count);

            var statistics = statusCounts.Select(x => new BookingStatusStatisticsDto
            {
                StatusId = x.StatusId,
                StatusName = x.StatusName,
                Count = x.Count,
                Percentage = totalBookings > 0 ? Math.Round((double)x.Count / totalBookings * 100, 2) : 0
            }).OrderBy(x => x.StatusId).ToList();

            return new BookingStatisticsResponseDto
            {
                TotalBookings = totalBookings,
                StatusStatistics = statistics,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<PagedResultDto<BookingResponse>> GetMyBookingsAsync(int currentUserId, int page, int pageSize)
        {
            // Validate pagination
            var currentPage = Math.Max(1, page);
            var currentPageSize = Math.Clamp(pageSize, 1, 100);

            var (bookings, totalCount) = await _bookingRepo.GetBookingsForUserPagedAsync(currentUserId, currentPage, currentPageSize);
            var items = bookings.Select(MapToBookingResponse).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);

            return new PagedResultDto<BookingResponse>
            {
                Items = items,
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Get pending bookings for a photographer
        /// </summary>
        public async Task<PhotographerPendingBookingsResponseDto> GetPhotographerPendingBookingsAsync(int photographerId, int page = 1, int pageSize = 10)
        {
            // Validate pagination
            var currentPage = Math.Max(1, page);
            var currentPageSize = Math.Clamp(pageSize, 1, 100);

            // Get pending status ID (assuming 1 is pending or get it dynamically)
            var pendingStatus = await _statusRepo.GetByNameAsync("Pending");
            if (pendingStatus == null)
            {
                return new PhotographerPendingBookingsResponseDto
                {
                    Data = new List<PendingBookingResponseDto>(),
                    TotalCount = 0,
                    CurrentPage = currentPage,
                    PageSize = currentPageSize,
                    TotalPages = 0
                };
            }

            // Get photographer's pending bookings
            var bookings = await _bookingRepo.GetBookingsByPhotographerAsync(photographerId);
            var pendingBookings = bookings
                .Where(b => b.StatusId == pendingStatus.StatusId)
                .OrderByDescending(b => b.ScheduleAt)
                .ToList();

            // Apply pagination
            var totalCount = pendingBookings.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);

            var paginatedBookings = pendingBookings
                .Skip((currentPage - 1) * currentPageSize)
                .Take(currentPageSize)
                .ToList();

            // Enrich bookings with additional data (customer photo types for this booking)
            var pendingBookingResponses = new List<PendingBookingResponseDto>();

            foreach (var booking in paginatedBookings)
            {
                // Reload booking with full details
                var bookingWithDetails = await _bookingRepo.GetBookingWithDetailsAsync(booking.BookingId);
                if (bookingWithDetails != null)
                {
                    var pendingResponse = MapToPendingBookingResponseDto(bookingWithDetails);
                    pendingBookingResponses.Add(pendingResponse);
                }
            }

            return new PhotographerPendingBookingsResponseDto
            {
                Data = pendingBookingResponses,
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages
            };
        }

        #region Private Mapping Methods

        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer?.Name,
                CustomerEmail = booking.Customer?.Email,
                CustomerPhone = booking.Customer?.Phone,
                PhotographerId = booking.PhotographerId,
                PhotographerName = booking.Photographer?.Name,
                PhotographerEmail = booking.Photographer?.Email,
                PhotographerPhone = booking.Photographer?.Phone,
                ScheduleAt = booking.ScheduleAt,
                LocationAddress = booking.LocationAddress,
                StatusId = booking.StatusId,
                StatusName = booking.Status?.StatusName,
                Price = booking.Price,
                Note = booking.Note,
                PhotoTypeId = booking.PhotoTypeId,
                Time = booking.Time
            };
        }

        private static BookingResponse MapToBookingResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                Customer = booking.Customer != null ? MapToBookingUserDto(booking.Customer) : null,
                Photographer = booking.Photographer != null ? MapToBookingPhotographerDto(booking.Photographer) : null,
                ScheduleAt = booking.ScheduleAt,
                LocationAddress = booking.LocationAddress,
                Status = booking.Status != null ? MapToBookingStatusDto(booking.Status) : null,
                Price = booking.Price,
                Note = booking.Note,
                PhotoLink = booking.PhotoLink
            };
        }

        private static BookingUserDto MapToBookingUserDto(User user)
        {
            return new BookingUserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone
            };
        }

        private static BookingPhotographerDto MapToBookingPhotographerDto(User user)
        {
            var dto = new BookingPhotographerDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone,
                AvgRating = user.PhotographerProfile?.AvgRating,
                IsAvailable = user.PhotographerProfile?.IsAvailable ?? false,
                LevelPhotographer = user.PhotographerProfile?.LevelPhotographer,
                PhotoPrice = null, // PhotoPrice is not available in PhotographerProfile model
                AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? null : user.AvatarUrl
            };
            return dto;
        }

        private static BookingStatusDto MapToBookingStatusDto(BookingStatus status)
        {
            return new BookingStatusDto
            {
                StatusId = status.StatusId,
                StatusName = status.StatusName
            };
        }

        private static PendingBookingResponseDto MapToPendingBookingResponseDto(Booking booking)
        {
            PendingBookingPhotoTypeDto? photoType = null;
            int? duration = booking.Time;

            // Get photo type details based on booking.PhotoTypeId
            if (booking.PhotoTypeId.HasValue && booking.Photographer?.PhotographerProfile?.PhotographerPhotoTypes?.Any() == true)
            {
                // Find the specific photo type that was booked
                var bookedPhotoType = booking.Photographer.PhotographerProfile.PhotographerPhotoTypes
                    .FirstOrDefault(ppt => ppt.PhotoTypeId == booking.PhotoTypeId.Value);

                if (bookedPhotoType?.PhotoType != null)
                {
                    photoType = new PendingBookingPhotoTypeDto
                    {
                        PhotoTypeId = bookedPhotoType.PhotoTypeId,
                        PhotoTypeName = bookedPhotoType.PhotoType.PhotoTypeName,
                        PhotoPrice = bookedPhotoType.PhotoPrice,
                        Time = bookedPhotoType.Time
                    };
                }
            }

            return new PendingBookingResponseDto
            {
                BookingId = booking.BookingId,
                User = booking.Customer != null ? new PendingBookingUserDto
                {
                    UserId = booking.Customer.UserId,
                    AvatarUrl = string.IsNullOrWhiteSpace(booking.Customer.AvatarUrl) ? null : booking.Customer.AvatarUrl,
                    Name = booking.Customer.Name,
                    Email = booking.Customer.Email,
                    Phone = string.IsNullOrEmpty(booking.Customer.Phone) ? null : booking.Customer.Phone
                } : null,
                Photographer = booking.Photographer != null ? new PendingBookingPhotographerDto
                {
                    UserId = booking.Photographer.UserId,
                    AvatarUrl = string.IsNullOrWhiteSpace(booking.Photographer.AvatarUrl) ? null : booking.Photographer.AvatarUrl,
                    Name = booking.Photographer.Name,
                    Email = booking.Photographer.Email,
                    Phone = string.IsNullOrEmpty(booking.Photographer.Phone) ? null : booking.Photographer.Phone,
                    AvgRating = booking.Photographer.PhotographerProfile?.AvgRating,
                    IsAvailable = booking.Photographer.PhotographerProfile?.IsAvailable ?? false,
                    LevelPhotographer = booking.Photographer.PhotographerProfile?.LevelPhotographer
                } : null,
                ScheduleAt = booking.ScheduleAt,
                LocationAddress = booking.LocationAddress,
                Price = booking.Price,
                Status = booking.Status != null ? new PendingBookingStatusDto
                {
                    StatusId = booking.Status.StatusId,
                    StatusName = booking.Status.StatusName
                } : null,
                Duration = duration,
                PhotoType = photoType,
                Note = booking.Note
            };
        }

        #endregion
    }
}
