using System;
using Microsoft.EntityFrameworkCore;

namespace DailyCheckBackend.Models
{
    public class DailyCheckDbContext : DbContext
    {
        public DbSet<ToDo> ToDos { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GoogleUser> GoogleUsers { get; set; }



        public DailyCheckDbContext(DbContextOptions<DailyCheckDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration of the User table 
            modelBuilder.Entity<User>()
               .HasKey(u => u.Id); // define as primary key.

            modelBuilder.Entity<User>()
               .Property(u => u.Id)
               .ValueGeneratedOnAdd(); // value added automatically. autoincrement



            modelBuilder.Entity<GoogleUser>()
                 .HasKey(u => u.Id); // define as primary key.


            // Configuration of the ToDo table
            modelBuilder.Entity<ToDo>()
                .HasKey(t => t.Id); // define as primary key.

            modelBuilder.Entity<ToDo>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd(); // value added automatically.
                                        // Configuration of the ToDo table

        }
    }
}
