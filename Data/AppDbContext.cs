using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using ResidentManagementSystem.Models;
using System;

namespace ResidentManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "Server=127.0.0.1;Port=3310;Database=residentmanagementdb;User=root;Password=root;",
                options => options.EnableRetryOnFailure()
            ) ;
        }
    }
}
