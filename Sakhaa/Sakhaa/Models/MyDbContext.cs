using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sakhaa.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<BeneficiaryApplication> BeneficiaryApplications { get; set; }

    public virtual DbSet<BeneficiaryFeedback> BeneficiaryFeedbacks { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<CorporatePartnership> CorporatePartnerships { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<DonationProgram> DonationPrograms { get; set; }

    public virtual DbSet<DonationReport> DonationReports { get; set; }

    public virtual DbSet<GiftCardCustomization> GiftCardCustomizations { get; set; }

    public virtual DbSet<GiftDonation> GiftDonations { get; set; }

    public virtual DbSet<NewsEvent> NewsEvents { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<PublicReport> PublicReports { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SuccessStory> SuccessStories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-KIES2N9;Database=Sakhaa;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Addresse__091C2AFB0D2FB7D3");

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Street).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Addresses__UserI__2EDAF651");
        });

        modelBuilder.Entity<BeneficiaryApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Benefici__3214EC0769DBCEFE");

            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
        });

        modelBuilder.Entity<BeneficiaryFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Benefici__3214EC07F7254CAC");

            entity.ToTable("BeneficiaryFeedback");

            entity.Property(e => e.BeneficiaryName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carts__3214EC0737ABBB7E");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Carts__UserId__09A971A2");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CartItem__3214EC07D453F6E2");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CartItems__CartI__0D7A0286");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CartItems__Produ__0E6E26BF");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC077217DE53");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactM__3214EC0769EEF0FC");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(255);
        });

        modelBuilder.Entity<CorporatePartnership>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Corporat__3214EC076CA39AA5");

            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PartnershipDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC077CB1ECD9");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonationEndDate).HasColumnType("datetime");
            entity.Property(e => e.DonationStartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Program).WithMany(p => p.Donations)
                .HasForeignKey(d => d.ProgramId)
                .HasConstraintName("FK__Donations__Progr__3E52440B");

            entity.HasOne(d => d.Project).WithMany(p => p.Donations)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK_Donations_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Donations__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<DonationProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC07BCFF7755");

            entity.Property(e => e.ImagePath).IsUnicode(false);
            entity.Property(e => e.IsSubscibtion)
                .HasDefaultValue(true)
                .HasColumnName("isSubscibtion");
            entity.Property(e => e.MinimumDonationAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<DonationReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC07F632BCD2");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ImpactDescription).HasColumnType("text");
            entity.Property(e => e.ReportDate).HasColumnType("datetime");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Donation).WithMany(p => p.DonationReports)
                .HasForeignKey(d => d.DonationId)
                .HasConstraintName("FK__DonationR__Donat__46E78A0C");

            entity.HasOne(d => d.Subscription).WithMany(p => p.DonationReports)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK__DonationR__Subsc__45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.DonationReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonationR__UserI__44FF419A");
        });

        modelBuilder.Entity<GiftCardCustomization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GiftCard__3214EC078CB34984");

            entity.HasIndex(e => e.GiftDonationId, "UQ__GiftCard__A2B58B707DA489E0").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DecorationImageUrl).HasMaxLength(100);
            entity.Property(e => e.OccasionImageUrl).HasMaxLength(200);
            entity.Property(e => e.TextColor).HasMaxLength(20);

            entity.HasOne(d => d.GiftDonation).WithOne(p => p.GiftCardCustomization)
                .HasForeignKey<GiftCardCustomization>(d => d.GiftDonationId)
                .HasConstraintName("FK__GiftCardC__GiftD__3F115E1A");
        });

        modelBuilder.Entity<GiftDonation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GiftDona__3214EC07E6043509");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.GiftCardUrl).HasColumnName("GiftCardURL");
            entity.Property(e => e.GiverEmail).HasMaxLength(100);
            entity.Property(e => e.GiverName).HasMaxLength(255);
            entity.Property(e => e.GiverPhone).HasMaxLength(20);
            entity.Property(e => e.ReceiverEmail).HasMaxLength(100);
            entity.Property(e => e.ReceiverName).HasMaxLength(255);
            entity.Property(e => e.ReceiverPhone).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.GiftDonations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_GiftDonations_Users");
        });

        modelBuilder.Entity<NewsEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NewsEven__3214EC070A6ED02A");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC074E0F2967");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_Orders_Addresses");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("FK_Orders_PaymentMethods");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Orders__UserId__1332DBDC");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC072829BA58");

            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItem__Order__17036CC0");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__OrderItem__Produ__17F790F9");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07772CAB4D");

            entity.HasIndex(e => e.TransactionId, "UQ__Payments__55433A6A78B13E1E").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionId)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Donation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.DonationId)
                .HasConstraintName("FK__Payments__Donati__5AEE82B9");

            entity.HasOne(d => d.GiftDonation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.GiftDonationId)
                .HasConstraintName("FK__Payments__GiftDo__5CD6CB2B");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK__Payments__Subscr__5BE2A6F2");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__UserId__59FA5E80");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC07E1CC5446");

            entity.Property(e => e.CardHolderName).HasMaxLength(100);
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Cvv)
                .HasMaxLength(5)
                .HasColumnName("CVV");
            entity.Property(e => e.ExpiryDate).HasMaxLength(10);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.PaymentMethods)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__PaymentMe__UserI__339FAB6E");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC072D177B8A");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Price)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductImage).HasMaxLength(255);
            entity.Property(e => e.ProductName).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Products__Catego__05D8E0BE");

            entity.HasOne(d => d.Sponsor).WithMany(p => p.Products)
                .HasForeignKey(d => d.SponsorId)
                .HasConstraintName("FK__Products__Sponso__2BFE89A6");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Projects__3214EC078F16CFC5");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentAmount)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TargetAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<PublicReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PublicRe__3214EC074FE3F62C");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.ReportFileName).HasMaxLength(255);
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.SponsorId).HasName("PK__Sponsors__3B609ED5914D404C");

            entity.Property(e => e.LogoPath).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC0740A99BB2");

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Program).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__Progr__4222D4EF");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__UserI__412EB0B6");
        });

        modelBuilder.Entity<SuccessStory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SuccessS__3214EC0742D65F06");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07E5E8F1B5");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349836F208").IsUnique();

            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
