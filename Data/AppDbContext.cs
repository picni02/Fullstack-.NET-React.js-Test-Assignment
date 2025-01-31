using Microsoft.EntityFrameworkCore;
// using Pomelo.EntityFrameworkCore.MySql;
using ResidentManagementSystem.Models;
using System;

namespace ResidentManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<ResidentApartment> ResidentApartments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "Server=127.0.0.1;Port=3310;Database=residentmanagementdb;User=root;Password=root;"
            );
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

            // ResidentApartment (gerund): Kompozitni ključ 
         //   modelBuilder.Entity<ResidentApartment>()
         //       .HasKey(ra => new { ra.ResidentId, ra.ApartmentId });

            //modelBuilder.Entity<ResidentApartment>()
            //    .HasOne(ra => ra.Resident)
            //    .WithMany(r => r.ResidentApartments)
            //    .HasForeignKey(ra => ra.ResidentId);

            //modelBuilder.Entity<ResidentApartment>()
            //    .HasOne(ra => ra.Apartment)
            //    .WithMany(a => a.ResidentApartments)
            //    .HasForeignKey(ra => ra.ApartmentId);

            // Event: Foreign Key to Apartment
            //modelBuilder.Entity<Event>()
            //    .HasOne(e => e.Apartment)
            //    .WithMany()
            //    .HasForeignKey(e => e.ApartmentId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // Event: Foreign Key to Resident
            //modelBuilder.Entity<Event>()
            //    .HasOne(e => e.Resident)
            //    .WithMany()
            //    .HasForeignKey(e => e.ResidentId);
        //}

    }
}
