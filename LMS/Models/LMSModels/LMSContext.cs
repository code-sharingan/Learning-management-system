using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enroll> Enrolls { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Sshkey> Sshkeys { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.8-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_general_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => new { e.CategoryId, e.Name }, "CategoryID")
                    .IsUnique();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever()
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CategoryID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Due).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Points).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignedIn");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PRIMARY");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.ClassId, "CategoryOf");

                entity.HasIndex(e => new { e.Name, e.ClassId }, "Name")
                    .IsUnique();

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CategoryID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Weight).HasColumnType("tinyint(3) unsigned");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CategoryOf");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.CourseId, "OfferingOF");

                entity.HasIndex(e => new { e.Season, e.Year, e.CourseId }, "Season")
                    .IsUnique();

                entity.HasIndex(e => e.Professor, "fk_taughtby");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("courseID");

                entity.Property(e => e.EndTime).HasColumnType("time");

                entity.Property(e => e.Location).HasMaxLength(100);

                entity.Property(e => e.Professor)
                    .HasMaxLength(8)
                    .IsFixedLength();

                entity.Property(e => e.Season).HasMaxLength(6);

                entity.Property(e => e.StartTime).HasColumnType("time");

                entity.Property(e => e.Year).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("OfferingOF");

                entity.HasOne(d => d.ProfessorNavigation)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.Professor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_taughtby");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.Subject, "Listing");

                entity.HasIndex(e => new { e.Num, e.Subject }, "Num")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(11)")
                    .HasColumnName("CourseID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Num).HasColumnType("smallint(6)");

                entity.Property(e => e.Subject)
                    .HasMaxLength(4)
                    .IsFixedLength();

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.Subject)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Listing");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Subject)
                    .HasName("PRIMARY");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.Property(e => e.Subject)
                    .HasMaxLength(4)
                    .IsFixedLength();

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Enroll>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.ClassId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("Enroll");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.ClassId, "EnrolledIn");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(11)")
                    .HasColumnName("ClassID");

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrolls)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EnrolledIn");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Enrolls)
                    .HasForeignKey(d => d.UId)
                    .HasConstraintName("studentEnrolled");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.Subject, "WorkdIN");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Subject)
                    .HasMaxLength(4)
                    .IsFixedLength();

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.Subject)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("WorkdIN");
            });

            modelBuilder.Entity<Sshkey>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("sshkey");

                entity.Property(e => e.Sshkey1)
                    .HasColumnType("text")
                    .HasColumnName("sshkey");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.Subject, "MajorsIn");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Subject)
                    .HasMaxLength(4)
                    .IsFixedLength();

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Subject)
                    .HasConstraintName("MajorsIn");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => new { e.UId, e.AssignmentId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasCharSet("latin1")
                    .UseCollation("latin1_swedish_ci");

                entity.HasIndex(e => e.AssignmentId, "Assignment");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Score).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.HasOne(d => d.Assignment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssignmentId)
                    .HasConstraintName("Assignment");

                entity.HasOne(d => d.UIdNavigation)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.UId)
                    .HasConstraintName("Student");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
