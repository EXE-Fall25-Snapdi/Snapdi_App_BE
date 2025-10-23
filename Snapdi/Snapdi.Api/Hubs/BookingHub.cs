using Microsoft.AspNetCore.SignalR;
using Snapdi.Services.DTOs;

namespace Snapdi.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time booking status updates
    /// </summary>
    public class BookingHub : Hub
    {
        /// <summary>
        /// Client joins the admin booking monitoring group
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminBookingMonitoring");
            await Clients.Caller.SendAsync("JoinedAdminGroup", "Successfully joined admin booking monitoring");
        }

        /// <summary>
        /// Client leaves the admin booking monitoring group
        /// </summary>
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminBookingMonitoring");
            await Clients.Caller.SendAsync("LeftAdminGroup", "Successfully left admin booking monitoring");
        }

        /// <summary>
        /// Send booking status update to all admins
        /// </summary>
        public async Task NotifyBookingStatusChange(BookingStatusNotificationDto notification)
        {
            await Clients.Group("AdminBookingMonitoring").SendAsync("BookingStatusChanged", notification);
        }

        /// <summary>
        /// Send new booking notification to all admins
        /// </summary>
        public async Task NotifyNewBooking(BookingDto booking)
        {
            await Clients.Group("AdminBookingMonitoring").SendAsync("NewBookingCreated", booking);
        }

        /// <summary>
        /// Client joins a specific booking room for updates
        /// </summary>
        public async Task JoinBookingRoom(int bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Booking_{bookingId}");
            await Clients.Caller.SendAsync("JoinedBookingRoom", bookingId, "Successfully joined booking room");
        }

        /// <summary>
        /// Client leaves a specific booking room
        /// </summary>
        public async Task LeaveBookingRoom(int bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Booking_{bookingId}");
            await Clients.Caller.SendAsync("LeftBookingRoom", bookingId, "Successfully left booking room");
        }

        /// <summary>
        /// Send update to specific booking room
        /// </summary>
        public async Task NotifyBookingRoomUpdate(int bookingId, BookingDto booking)
        {
            await Clients.Group($"Booking_{bookingId}").SendAsync("BookingUpdated", booking);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
