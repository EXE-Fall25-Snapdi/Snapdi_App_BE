USE Snapdi_DB_v2u1;
GO

-- ========================================
-- Insert Sample Data
-- ========================================

-- Roles (already added some, just in case)
INSERT INTO Role (RoleName) VALUES
('ADMIN'),
('CUSTOMER'),
('PHOTOGRAPHER')

-- Users | password: 'Password@123'
INSERT INTO [User] (RoleID, Name, Email, Phone, Password, RefreshToken, ExpiredRefreshTokenAt, IsActive, IsVerify, CreatedAt, LocationAddress, LocationCity, AvatarUrl)
VALUES
(1, 'Admin User', 'admin@snapdi.com', '0123456789', '$2a$11$lHzmPJOdO0rXW60gJazRQ.BriiRsgJ.mmAahqoEFsGogh9JRGiA/e', NULL, NULL, 1, 1, GETDATE(), '123 Admin St', 'Hanoi', 'https://example.com/avatar/admin.jpg'),
(2, 'Alice Customer', 'alice@snapdi.com', '0987654321', '$2a$11$lHzmPJOdO0rXW60gJazRQ.BriiRsgJ.mmAahqoEFsGogh9JRGiA/e', NULL, NULL, 1, 1, GETDATE(), '456 Main St', 'Hanoi', 'https://example.com/avatar/alice.jpg'),
(2, 'Bob Customer', 'bob@snapdi.com', '0911222333', '$2a$11$lHzmPJOdO0rXW60gJazRQ.BriiRsgJ.mmAahqoEFsGogh9JRGiA/e', NULL, NULL, 1, 0, GETDATE(), '789 Second St', 'HCMC', 'https://example.com/avatar/bob.jpg'),
(3, 'John Photographer', 'john@snapdi.com', '0909090909', '$2a$11$lHzmPJOdO0rXW60gJazRQ.BriiRsgJ.mmAahqoEFsGogh9JRGiA/e', NULL, NULL, 1, 1, GETDATE(), '88 Studio St', 'Hanoi', 'https://example.com/avatar/john.jpg'),
(3, 'Sara Photographer', 'sara@snapdi.com', '0933445566', '$2a$11$lHzmPJOdO0rXW60gJazRQ.BriiRsgJ.mmAahqoEFsGogh9JRGiA/e', NULL, NULL, 1, 1, GETDATE(), '77 Photo St', 'HCMC', 'https://example.com/avatar/sara.jpg');

-- Keywords
INSERT INTO Keyword (Keyword) VALUES
('Wedding'),
('Portrait'),
('Nature'),
('Studio');

-- Blog
INSERT INTO Blog (AuthorID, Title, ThumbnailUrl, Content, CreateAt, UpdateAt, IsActive)
VALUES
(1, 'Top 10 Wedding Photography Tips', 'https://example.com/blog/wedding.jpg', 'Content for wedding tips...', GETDATE(), NULL, 1),
(1, 'Best Studio Lighting for Portraits', 'https://example.com/blog/studio.jpg', 'Content for studio portraits...', GETDATE(), NULL, 1);

-- KeywordsInBlog
INSERT INTO KeywordsInBlog (BlogID, KeywordID) VALUES
(1, 1),
(1, 3),
(2, 2),
(2, 4);

-- PhotoPortfolio
INSERT INTO PhotoPortfolio (UserID, PhotoUrl)
VALUES
(4, 'https://example.com/portfolio/john1.jpg'),
(4, 'https://example.com/portfolio/john2.jpg'),
(5, 'https://example.com/portfolio/sara1.jpg');

-- PhotographerProfile
INSERT INTO PhotographerProfile (UserID, EquipmentDescription, YearsOfExperience, AvgRating, IsAvailable, Description)
VALUES
(4, 'Canon EOS R5', '3-5 years of experience', 4.7, 1, 'Experienced wedding photographer'),
(5, 'Nikon Z6 II', 'Below 1 years of experience', 4.5, 1, 'Specialist in portrait photography');

-- Styles
INSERT INTO Styles (StyleName) VALUES
('Wedding'),
('Portrait'),
('Fashion');

-- BookingStatus
INSERT INTO BookingStatus (StatusName) VALUES
('Pending'),
('Confirmed'),
('Completed'),
('Cancelled');

-- Booking
INSERT INTO Booking (CustomerID, PhotographerID, ScheduleAt, LocationCity, LocationAddress, StyleID, StatusID, Price)
VALUES
(2, 4, DATEADD(DAY, 7, GETDATE()), 'Hanoi', 'Golden Palace', 1, 2, 300.0),
(3, 5, DATEADD(DAY, 10, GETDATE()), 'HCMC', 'Diamond Hall', 2, 1, 200.0);

-- Review
INSERT INTO Review (BookingID, FromUserID, ToUserID, Rating, Comment, CreateAt)
VALUES
(1, 2, 4, 5.0, 'Amazing experience! Highly recommended.', GETDATE()),
(2, 3, 5, 4.0, 'Good but room for improvement.', GETDATE());

-- Voucher
INSERT INTO Voucher (Code, Description, DiscountType, DiscountValue, MaxDiscount, MinSpend, StartDate, EndDate, UsageLimit, IsActive)
VALUES
('WELCOME10', '10% off for new users', 'PERCENT', 10, 50, 100, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 100, 1),
('SAVE50', 'Flat 50k off', 'AMOUNT', 50, NULL, 200, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 50, 1);

-- VoucherUsage
INSERT INTO VoucherUsage (BookingID, VoucherID, UserID, UsedAt)
VALUES
(1, 1, 2, GETDATE());

-- FeePolicy
INSERT INTO FeePolicy (TransactionType, FeePercent, EffectiveDate, ExpiryDate, IsActive)
VALUES
('Booking', 5.0, GETDATE(), NULL, 1);

-- PaymentStatus
INSERT INTO PaymentStatus (StatusName) VALUES
('Pending'),
('Paid'),
('Refunded');

-- Payment
INSERT INTO Payment (BookingID, Amount, FeePolicyID, FeePercent, FeeAmount, NetAmount, TransactionMethod, TransactionReference, PaymentStatusID, PaymentDate)
VALUES
(1, 300, 1, 5, 15, 285, 'CreditCard', 'TXN123456', 2, GETDATE()),
(2, 200, 1, 5, 10, 190, 'PayPal', 'TXN654321', 1, GETDATE());

-- Conversations
INSERT INTO Conversations (Type, CreateAt)
VALUES
('Private', GETDATE()),
('Group', GETDATE());

-- ConversationParticipants
INSERT INTO ConversationParticipants (ConversationID, UserID, JoinedAt)
VALUES
(1, 2, GETDATE()),
(1, 4, GETDATE()),
(2, 2, GETDATE()),
(2, 3, GETDATE()),
(2, 5, GETDATE());

-- Messages
INSERT INTO Messages (ConversationID, SenderID, Content, SendAt, Status, ExpiredDate)
VALUES
(1, 2, 'Hello John, I want to confirm our booking.', GETDATE(), 'Sent', NULL),
(1, 4, 'Sure Alice, see you next week!', GETDATE(), 'Sent', NULL),
(2, 5, 'Hey everyone, let’s collaborate!', GETDATE(), 'Sent', NULL);
