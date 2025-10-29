-- =============================================
-- Snapdi Database PostgreSQL Initialization Script
-- Generated for PostgreSQL 12+ with PostGIS support
-- =============================================

-- Enable PostGIS extension for spatial data support
CREATE EXTENSION IF NOT EXISTS postgis;

-- =============================================
-- Drop existing tables (if any) in reverse dependency order
-- =============================================
DROP TABLE IF EXISTS "VoucherUsage" CASCADE;
DROP TABLE IF EXISTS "Payment" CASCADE;
DROP TABLE IF EXISTS "Review" CASCADE;
DROP TABLE IF EXISTS "PhotographerPhotoType" CASCADE;
DROP TABLE IF EXISTS "PhotographerStyle" CASCADE;
DROP TABLE IF EXISTS "KeywordsInBlog" CASCADE;
DROP TABLE IF EXISTS "ConversationParticipant" CASCADE;
DROP TABLE IF EXISTS "Message" CASCADE;
DROP TABLE IF EXISTS "PhotoPortfolio" CASCADE;
DROP TABLE IF EXISTS "Booking" CASCADE;
DROP TABLE IF EXISTS "PhotographerProfile" CASCADE;
DROP TABLE IF EXISTS "Blog" CASCADE;
DROP TABLE IF EXISTS "Conversation" CASCADE;
DROP TABLE IF EXISTS "User" CASCADE;
DROP TABLE IF EXISTS "Voucher" CASCADE;
DROP TABLE IF EXISTS "FeePolicy" CASCADE;
DROP TABLE IF EXISTS "PaymentStatus" CASCADE;
DROP TABLE IF EXISTS "BookingStatus" CASCADE;
DROP TABLE IF EXISTS "PhotoType" CASCADE;
DROP TABLE IF EXISTS "Style" CASCADE;
DROP TABLE IF EXISTS "Keyword" CASCADE;
DROP TABLE IF EXISTS "Role" CASCADE;

-- =============================================
-- Create Tables
-- =============================================

-- Role Table
CREATE TABLE "Role" (
    "RoleID" SERIAL PRIMARY KEY,
    "RoleName" VARCHAR(100) NOT NULL
);

-- User Table
CREATE TABLE "User" (
    "UserID" SERIAL PRIMARY KEY,
    "RoleID" INTEGER,
    "Name" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "Phone" VARCHAR(50),
    "Password" VARCHAR(255) NOT NULL,
    "RefreshToken" VARCHAR(255),
    "ExpiredRefreshTokenAt" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsVerify" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LocationAddress" VARCHAR(255),
    "LocationCity" VARCHAR(100),
    "AvatarUrl" VARCHAR(255),
    "CurrentLocation" geography(Point, 4326),
    CONSTRAINT "FK__User__RoleID__3C69FB99" FOREIGN KEY ("RoleID") REFERENCES "Role"("RoleID")
);

-- Create unique index on Email
CREATE UNIQUE INDEX "UQ__User__A9D10534DB5CBBA4" ON "User"("Email");

-- BookingStatus Table
CREATE TABLE "BookingStatus" (
    "StatusID" SERIAL PRIMARY KEY,
    "StatusName" VARCHAR(100) NOT NULL
);

-- PaymentStatus Table
CREATE TABLE "PaymentStatus" (
    "PaymentStatusID" SERIAL PRIMARY KEY,
    "StatusName" VARCHAR(100) NOT NULL
);

-- FeePolicy Table
CREATE TABLE "FeePolicy" (
    "FeePolicyID" SERIAL PRIMARY KEY,
    "TransactionType" VARCHAR(100),
    "FeePercent" DOUBLE PRECISION NOT NULL,
    "EffectiveDate" TIMESTAMP NOT NULL,
    "ExpiryDate" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- PhotographerProfile Table
CREATE TABLE "PhotographerProfile" (
    "UserID" INTEGER PRIMARY KEY,
    "EquipmentDescription" VARCHAR(500),
    "YearsOfExperience" VARCHAR(100),
    "AvgRating" DOUBLE PRECISION,
    "IsAvailable" BOOLEAN NOT NULL DEFAULT TRUE,
    "Description" VARCHAR(500),
    "LevelPhotographer" VARCHAR(50),
    "WorkLocation" VARCHAR(255),
    CONSTRAINT "FK__Photograp__UserI__4AB81AF0" FOREIGN KEY ("UserID") REFERENCES "User"("UserID") ON DELETE CASCADE
);

-- Style Table
CREATE TABLE "Style" (
    "StyleID" SERIAL PRIMARY KEY,
    "StyleName" VARCHAR(100) NOT NULL
);

-- PhotoType Table
CREATE TABLE "PhotoType" (
    "PhotoTypeID" SERIAL PRIMARY KEY,
    "PhotoTypeName" VARCHAR(100) NOT NULL
);

-- PhotographerStyle Table (Many-to-Many)
CREATE TABLE "PhotographerStyle" (
    "UserID" INTEGER NOT NULL,
    "StyleID" INTEGER NOT NULL,
    PRIMARY KEY ("UserID", "StyleID"),
    CONSTRAINT "FK__Photograp__UserI__PhotographerStyle" FOREIGN KEY ("UserID") REFERENCES "PhotographerProfile"("UserID") ON DELETE CASCADE,
    CONSTRAINT "FK__Photograp__Style__PhotographerStyle" FOREIGN KEY ("StyleID") REFERENCES "Style"("StyleID") ON DELETE CASCADE
);

-- PhotographerPhotoType Table (Many-to-Many with additional fields)
CREATE TABLE "PhotographerPhotoType" (
    "UserID" INTEGER NOT NULL,
    "PhotoTypeID" INTEGER NOT NULL,
    "PhotoPrice" DOUBLE PRECISION,
    "Time" INTEGER,
    PRIMARY KEY ("UserID", "PhotoTypeID"),
    CONSTRAINT "FK__Photograp__UserI__PhotographerPhotoType" FOREIGN KEY ("UserID") REFERENCES "PhotographerProfile"("UserID") ON DELETE CASCADE,
    CONSTRAINT "FK__Photograp__Photo__PhotographerPhotoType" FOREIGN KEY ("PhotoTypeID") REFERENCES "PhotoType"("PhotoTypeID") ON DELETE CASCADE
);

-- Booking Table
CREATE TABLE "Booking" (
    "BookingID" SERIAL PRIMARY KEY,
    "CustomerID" INTEGER,
    "PhotographerID" INTEGER,
    "ScheduleAt" TIMESTAMP NOT NULL,
    "LocationAddress" VARCHAR(255),
    "StatusID" INTEGER,
    "Price" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Note" VARCHAR(1000),
    "PhotoLink" VARCHAR(500),
    "PhotoTypeID" INTEGER,
    "Time" INTEGER,
    CONSTRAINT "FK__Booking__Custome__534D60F1" FOREIGN KEY ("CustomerID") REFERENCES "User"("UserID"),
    CONSTRAINT "FK__Booking__Photogr__5441852A" FOREIGN KEY ("PhotographerID") REFERENCES "User"("UserID"),
    CONSTRAINT "FK__Booking__StatusI__5629CD9C" FOREIGN KEY ("StatusID") REFERENCES "BookingStatus"("StatusID")
);

-- Payment Table
CREATE TABLE "Payment" (
    "PaymentID" SERIAL PRIMARY KEY,
    "BookingID" INTEGER,
    "Amount" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "FeePolicyID" INTEGER,
    "FeePercent" DOUBLE PRECISION,
    "FeeAmount" DOUBLE PRECISION,
    "NetAmount" DOUBLE PRECISION,
    "TransactionMethod" VARCHAR(50),
    "TransactionReference" VARCHAR(255),
    "PaymentStatusID" INTEGER,
    "PaymentDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK__Payment__Booking__693CA210" FOREIGN KEY ("BookingID") REFERENCES "Booking"("BookingID") ON DELETE CASCADE,
    CONSTRAINT "FK__Payment__FeePoli__6A30C649" FOREIGN KEY ("FeePolicyID") REFERENCES "FeePolicy"("FeePolicyID"),
    CONSTRAINT "FK__Payment__Payment__6B24EA82" FOREIGN KEY ("PaymentStatusID") REFERENCES "PaymentStatus"("PaymentStatusID")
);

-- Review Table
CREATE TABLE "Review" (
    "ReviewID" SERIAL PRIMARY KEY,
    "BookingID" INTEGER,
    "FromUserID" INTEGER,
    "ToUserID" INTEGER,
    "Rating" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Comment" TEXT,
    "CreateAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK__Review__BookingI__59063A47" FOREIGN KEY ("BookingID") REFERENCES "Booking"("BookingID") ON DELETE CASCADE,
    CONSTRAINT "FK__Review__FromUser__59FA5E80" FOREIGN KEY ("FromUserID") REFERENCES "User"("UserID"),
    CONSTRAINT "FK__Review__ToUserID__5AEE82B9" FOREIGN KEY ("ToUserID") REFERENCES "User"("UserID")
);

-- Blog Table
CREATE TABLE "Blog" (
    "BlogID" SERIAL PRIMARY KEY,
    "AuthorID" INTEGER,
    "Title" VARCHAR(255) NOT NULL,
    "ThumbnailUrl" VARCHAR(255) NOT NULL,
    "Content" TEXT NOT NULL,
    "CreateAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdateAt" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "FK__Blog__AuthorID__3F466844" FOREIGN KEY ("AuthorID") REFERENCES "User"("UserID")
);

-- Keyword Table
CREATE TABLE "Keyword" (
    "KeywordID" SERIAL PRIMARY KEY,
    "Keyword" VARCHAR(255) NOT NULL
);

-- KeywordsInBlog Table (Many-to-Many)
CREATE TABLE "KeywordsInBlog" (
    "BlogID" INTEGER NOT NULL,
    "KeywordID" INTEGER NOT NULL,
    PRIMARY KEY ("BlogID", "KeywordID"),
    CONSTRAINT "FK__KeywordsI__BlogI__4222D4EF" FOREIGN KEY ("BlogID") REFERENCES "Blog"("BlogID") ON DELETE CASCADE,
    CONSTRAINT "FK__KeywordsI__Keywo__4316F928" FOREIGN KEY ("KeywordID") REFERENCES "Keyword"("KeywordID") ON DELETE CASCADE
);

-- PhotoPortfolio Table
CREATE TABLE "PhotoPortfolio" (
    "PhotoPortfolioID" SERIAL PRIMARY KEY,
    "UserID" INTEGER NOT NULL,
    "PhotoUrl" VARCHAR(500) NOT NULL,
    CONSTRAINT "FK__PhotoPort__UserI__PhotoPortfolio_User" FOREIGN KEY ("UserID") REFERENCES "User"("UserID") ON DELETE CASCADE
);

-- Conversation Table
CREATE TABLE "Conversation" (
    "ConversationID" SERIAL PRIMARY KEY,
    "Type" VARCHAR(50),
    "CreateAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ConversationParticipant Table
CREATE TABLE "ConversationParticipant" (
    "ConversationID" INTEGER NOT NULL,
    "UserID" INTEGER NOT NULL,
    "JoinedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastReadMessageId" INTEGER,
    "LastReadAt" TIMESTAMP,
    PRIMARY KEY ("ConversationID", "UserID"),
    CONSTRAINT "FK__Conversat__Conve__6FE99F9F" FOREIGN KEY ("ConversationID") REFERENCES "Conversation"("ConversationID") ON DELETE CASCADE,
    CONSTRAINT "FK__Conversat__UserI__70DDC3D8" FOREIGN KEY ("UserID") REFERENCES "User"("UserID") ON DELETE CASCADE
);

-- Message Table
CREATE TABLE "Message" (
    "MessageID" SERIAL PRIMARY KEY,
    "ConversationID" INTEGER,
    "SenderID" INTEGER,
    "Content" TEXT NOT NULL,
    "SendAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Status" VARCHAR(50),
    "ExpiredDate" TIMESTAMP,
    CONSTRAINT "FK__Messages__Conver__73BA3083" FOREIGN KEY ("ConversationID") REFERENCES "Conversation"("ConversationID") ON DELETE CASCADE,
    CONSTRAINT "FK__Messages__Sender__74AE54BC" FOREIGN KEY ("SenderID") REFERENCES "User"("UserID") ON DELETE CASCADE
);

-- Voucher Table
CREATE TABLE "Voucher" (
    "VoucherID" SERIAL PRIMARY KEY,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Description" VARCHAR(255),
    "DiscountType" VARCHAR(50),
    "DiscountValue" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "MaxDiscount" DOUBLE PRECISION,
    "MinSpend" DOUBLE PRECISION,
    "StartDate" TIMESTAMP NOT NULL,
    "EndDate" TIMESTAMP NOT NULL,
    "UsageLimit" INTEGER,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

-- Create unique index on Voucher Code
CREATE UNIQUE INDEX "UQ__Voucher__A25C5AA7912D9D85" ON "Voucher"("Code");

-- VoucherUsage Table
CREATE TABLE "VoucherUsage" (
    "VoucherUsageID" SERIAL PRIMARY KEY,
    "BookingID" INTEGER,
    "VoucherID" INTEGER,
    "UserID" INTEGER,
    "UsedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK__VoucherUs__Booki__60A75C0F" FOREIGN KEY ("BookingID") REFERENCES "Booking"("BookingID") ON DELETE CASCADE,
    CONSTRAINT "FK__VoucherUs__Vouch__619B8048" FOREIGN KEY ("VoucherID") REFERENCES "Voucher"("VoucherID") ON DELETE CASCADE,
    CONSTRAINT "FK__VoucherUs__UserI__628FA481" FOREIGN KEY ("UserID") REFERENCES "User"("UserID") ON DELETE CASCADE
);

-- =============================================
-- Seed Data
-- =============================================

-- Insert Roles
INSERT INTO "Role" ("RoleID", "RoleName") VALUES
(1, 'ADMIN'),
(2, 'CUSTOMER'),
(3, 'PHOTOGRAPHER');

-- Set sequence for Role to continue from 4
SELECT setval(pg_get_serial_sequence('"Role"', 'RoleID'), 3, true);

-- Insert BookingStatus
INSERT INTO "BookingStatus" ("StatusID", "StatusName") VALUES
(1, 'Pending'),
(2, 'Confirmed'),
(3, 'Paid'),
(4, 'Going'),
(5, 'Processing'),
(6, 'Done'),
(7, 'Completed');

-- Set sequence for BookingStatus to continue from 8
SELECT setval(pg_get_serial_sequence('"BookingStatus"', 'StatusID'), 7, true);

-- Insert PaymentStatus
INSERT INTO "PaymentStatus" ("PaymentStatusID", "StatusName") VALUES
(1, 'Pending'),
(2, 'Paid'),
(3, 'Refunded'),
(4, 'Confirmed');

-- Set sequence for PaymentStatus to continue from 5
SELECT setval(pg_get_serial_sequence('"PaymentStatus"', 'PaymentStatusID'), 4, true);

-- Insert Default Admin User
INSERT INTO "User" (
    "RoleID",
    "Name",
    "Email",
    "Password",
    "IsActive",
    "IsVerify",
    "CreatedAt"
) VALUES (
    1,
    'Admin User',
    'admin@snapdi.com',
    '$2y$10$zW75IVkYdDjmMPRZBDz9r.2/iXlAlby5NWm6dq6EpiuGFlcacVPDS',
    TRUE,
    TRUE,
    CURRENT_TIMESTAMP
);

-- =============================================
-- Script Completion
-- =============================================
-- Database initialization completed successfully

