using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Polyclinic.Models.data;

namespace Polyclinic.Data
{
    public partial class dataContext : DbContext
    {
        public dataContext()
        {
        }

        public dataContext(DbContextOptions<dataContext> options) : base(options)
        {
        }

        partial void OnModelBuilding(ModelBuilder builder);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Polyclinic.Models.data.AspNetUserLogin>().HasKey(table => new {
                table.LoginProvider, table.ProviderKey
            });

            builder.Entity<Polyclinic.Models.data.AspNetUserRole>().HasKey(table => new {
                table.UserId, table.RoleId
            });

            builder.Entity<Polyclinic.Models.data.AspNetUserToken>().HasKey(table => new {
                table.UserId, table.LoginProvider, table.Name
            });

            builder.Entity<Polyclinic.Models.data.Doctor>()
              .HasOne(i => i.Gender)
              .WithMany(i => i.Doctors)
              .HasForeignKey(i => i.IdGender)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Doctor>()
              .HasOne(i => i.Specialization)
              .WithMany(i => i.Doctors)
              .HasForeignKey(i => i.IdSpecialization)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Doctor>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.Doctors)
              .HasForeignKey(i => i.IdUser)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Patient>()
              .HasOne(i => i.Gender)
              .WithMany(i => i.Patients)
              .HasForeignKey(i => i.IdGender)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Patient>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.Patients)
              .HasForeignKey(i => i.IdUser)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Record>()
              .HasOne(i => i.Patient)
              .WithMany(i => i.Records)
              .HasForeignKey(i => i.IdPatient)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Record>()
              .HasOne(i => i.Schedule)
              .WithMany(i => i.Records)
              .HasForeignKey(i => i.IdSchedule)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Schedule>()
              .HasOne(i => i.Doctor)
              .WithMany(i => i.Schedules)
              .HasForeignKey(i => i.IdDoctor)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Visit>()
              .HasOne(i => i.Diagnosis)
              .WithMany(i => i.Visits)
              .HasForeignKey(i => i.IdDiagnosis)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.Visit>()
              .HasOne(i => i.Record)
              .WithMany(i => i.Visits)
              .HasForeignKey(i => i.IdRecord)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetRoleClaim>()
              .HasOne(i => i.AspNetRole)
              .WithMany(i => i.AspNetRoleClaims)
              .HasForeignKey(i => i.RoleId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetUserClaim>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.AspNetUserClaims)
              .HasForeignKey(i => i.UserId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetUserLogin>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.AspNetUserLogins)
              .HasForeignKey(i => i.UserId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetUserRole>()
              .HasOne(i => i.AspNetRole)
              .WithMany(i => i.AspNetUserRoles)
              .HasForeignKey(i => i.RoleId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetUserRole>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.AspNetUserRoles)
              .HasForeignKey(i => i.UserId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<Polyclinic.Models.data.AspNetUserToken>()
              .HasOne(i => i.AspNetUser)
              .WithMany(i => i.AspNetUserTokens)
              .HasForeignKey(i => i.UserId)
              .HasPrincipalKey(i => i.Id);
            this.OnModelBuilding(builder);
        }

        public DbSet<Polyclinic.Models.data.Diagnosis> Diagnoses { get; set; }

        public DbSet<Polyclinic.Models.data.Doctor> Doctors { get; set; }

        public DbSet<Polyclinic.Models.data.Gender> Genders { get; set; }

        public DbSet<Polyclinic.Models.data.Patient> Patients { get; set; }

        public DbSet<Polyclinic.Models.data.Record> Records { get; set; }

        public DbSet<Polyclinic.Models.data.Schedule> Schedules { get; set; }

        public DbSet<Polyclinic.Models.data.Specialization> Specializations { get; set; }

        public DbSet<Polyclinic.Models.data.Visit> Visits { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetRoleClaim> AspNetRoleClaims { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetRole> AspNetRoles { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetUserClaim> AspNetUserClaims { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetUserLogin> AspNetUserLogins { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetUserRole> AspNetUserRoles { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetUser> AspNetUsers { get; set; }

        public DbSet<Polyclinic.Models.data.AspNetUserToken> AspNetUserTokens { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    
    }
}