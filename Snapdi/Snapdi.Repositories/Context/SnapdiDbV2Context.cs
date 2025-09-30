using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Context;

public partial class SnapdiDbV2Context : DbContext
{
    public SnapdiDbV2Context()
    {
    }

    public SnapdiDbV2Context(DbContextOptions<SnapdiDbV2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }

    public virtual DbSet<FeePolicy> FeePolicies { get; set; }

    public virtual DbSet<Keyword> Keywords { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<PhotoPortfolio> PhotoPortfolios { get; set; }

    public virtual DbSet<PhotographerProfile> PhotographerProfiles { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Style> Styles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<VoucherUsage> VoucherUsages { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E506DDA2403");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs).HasConstraintName("FK__Blog__AuthorID__3F466844");

            entity.HasMany(d => d.Keywords).WithMany(p => p.Blogs)
                .UsingEntity<Dictionary<string, object>>(
                    "KeywordsInBlog",
                    r => r.HasOne<Keyword>().WithMany()
                        .HasForeignKey("KeywordId")
                        .HasConstraintName("FK__KeywordsI__Keywo__4316F928"),
                    l => l.HasOne<Blog>().WithMany()
                        .HasForeignKey("BlogId")
                        .HasConstraintName("FK__KeywordsI__BlogI__4222D4EF"),
                    j =>
                    {
                        j.HasKey("BlogId", "KeywordId").HasName("PK__Keywords__574B8D0CE2A6215B");
                        j.ToTable("KeywordsInBlog");
                        j.IndexerProperty<int>("BlogId").HasColumnName("BlogID");
                        j.IndexerProperty<int>("KeywordId").HasColumnName("KeywordID");
                    });
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951ACDC273B8F2");

            entity.HasOne(d => d.Customer).WithMany(p => p.BookingCustomers).HasConstraintName("FK__Booking__Custome__534D60F1");

            entity.HasOne(d => d.Photographer).WithMany(p => p.BookingPhotographers).HasConstraintName("FK__Booking__Photogr__5441852A");

            entity.HasOne(d => d.Status).WithMany(p => p.Bookings).HasConstraintName("FK__Booking__StatusI__5629CD9C");

            entity.HasOne(d => d.Style).WithMany(p => p.Bookings).HasConstraintName("FK__Booking__StyleID__5535A963");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__BookingS__C8EE20438C9AFA58");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D897EF513182");
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ConversationId, e.UserId }).HasName("PK__Conversa__1128545DD045149E");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationParticipants).HasConstraintName("FK__Conversat__Conve__6FE99F9F");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationParticipants).HasConstraintName("FK__Conversat__UserI__70DDC3D8");
        });

        modelBuilder.Entity<FeePolicy>(entity =>
        {
            entity.HasKey(e => e.FeePolicyId).HasName("PK__FeePolic__ADCA966AAC41B8C5");
        });

        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK__Keyword__37C135C1F834A132");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C037C0DF786B5");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Messages__Conver__73BA3083");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Messages__Sender__74AE54BC");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A5840ED1111");

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Payment__Booking__693CA210");

            entity.HasOne(d => d.FeePolicy).WithMany(p => p.Payments).HasConstraintName("FK__Payment__FeePoli__6A30C649");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.Payments).HasConstraintName("FK__Payment__Payment__6B24EA82");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.PaymentStatusId).HasName("PK__PaymentS__34F8AC1F9AB83D1F");
        });

        modelBuilder.Entity<PhotoPortfolio>(entity =>
        {
            entity.HasKey(e => e.PhotoPortfolioId).HasName("PK__PhotoPor__75BE09A8AE74CC21");

            entity.HasOne(d => d.User).WithMany(p => p.PhotoPortfolios)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PhotoPort__UserI__PhotoPortfolio_User");
        });

        modelBuilder.Entity<PhotographerProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Photogra__1788CCACD97935F9");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.PhotographerProfile).HasConstraintName("FK__Photograp__UserI__4AB81AF0");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79AE14FA1EB8");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Review__BookingI__59063A47");

            entity.HasOne(d => d.FromUser).WithMany(p => p.ReviewFromUsers).HasConstraintName("FK__Review__FromUser__59FA5E80");

            entity.HasOne(d => d.ToUser).WithMany(p => p.ReviewToUsers).HasConstraintName("FK__Review__ToUserID__5AEE82B9");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3AD61AC071");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.StyleId).HasName("PK__Styles__8AD147A0C035264E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC98BF1C87");

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK__User__RoleID__3C69FB99");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Voucher__3AEE79C1B9C1175C");
        });

        modelBuilder.Entity<VoucherUsage>(entity =>
        {
            entity.HasKey(e => e.VoucherUsageId).HasName("PK__VoucherU__4264F82BBFB948C0");

            entity.HasOne(d => d.Booking).WithMany(p => p.VoucherUsages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VoucherUs__Booki__60A75C0F");

            entity.HasOne(d => d.User).WithMany(p => p.VoucherUsages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VoucherUs__UserI__628FA481");

            entity.HasOne(d => d.Voucher).WithMany(p => p.VoucherUsages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__VoucherUs__Vouch__619B8048");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
