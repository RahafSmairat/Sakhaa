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

    public virtual DbSet<BeneficiaryApplication> BeneficiaryApplications { get; set; }

    public virtual DbSet<BeneficiaryFeedback> BeneficiaryFeedbacks { get; set; }

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<CorporatePartnership> CorporatePartnerships { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<DonationProgram> DonationPrograms { get; set; }

    public virtual DbSet<DonationReport> DonationReports { get; set; }

    public virtual DbSet<GiftDonation> GiftDonations { get; set; }

    public virtual DbSet<NewsEvent> NewsEvents { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PublicReport> PublicReports { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SuccessStory> SuccessStories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-KIES2N9;Database=Sakhaa;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BeneficiaryApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Benefici__3214EC0769DBCEFE");

            entity.Property(e => e.AidVerificationDocument)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FamilyBookUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.InsuranceStatusDocument)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BeneficiaryFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Benefici__3214EC07F7254CAC");

            entity.ToTable("BeneficiaryFeedback");

            entity.Property(e => e.BeneficiaryName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Feedback).HasColumnType("text");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Donations__Progr__3E52440B");

            entity.HasOne(d => d.User).WithMany(p => p.Donations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Donations__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<DonationProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC07BCFF7755");

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

        modelBuilder.Entity<GiftDonation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GiftDona__3214EC07E6043509");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DonationType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GiverEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GiverName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GiverPhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PersonalMessage).HasColumnType("text");
            entity.Property(e => e.ReceiverEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ReceiverName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ReceiverPhone)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NewsEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NewsEven__3214EC070A6ED02A");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);
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

        modelBuilder.Entity<PublicReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PublicRe__3214EC074FE3F62C");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.ReportFileName).HasMaxLength(255);
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
