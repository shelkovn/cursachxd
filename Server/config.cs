using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using static micpix.Server.Resources;

namespace micpix.Server
{
    class Class1 : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //"Server=DBSRV\\OV2025;Database=micpix;TrustServerCertificate=True;Trusted_Connection=True;" - сервер колледжа
            //"Server=DESKTOP-L229MBG\\SQLEXPRESS;Database=micpix;TrustServerCertificate=True;Trusted_Connection=True;" - сервер на домашнем пк
            optionsBuilder.UseSqlServer("Server=DESKTOP-L229MBG\\SQLEXPRESS;Database=micpix;TrustServerCertificate=True;Trusted_Connection=True;");
        }

        public DbSet<Users> UserSet { get; set; }
        public DbSet<Resources> ResourcesSet { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resources>().ToTable("Resources");
            modelBuilder.Entity<Users>().ToTable("Users");

            modelBuilder.Entity<Resources>()
                .HasOne(r => r.Author)
                .WithMany(u => u.Resources)
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); //нельзя удалять пользователя если в базе остались созданные ими элементы

            modelBuilder.Entity<UserCredential>()
           .ToTable("UserCredentials");  

            modelBuilder.Entity<UserCredential>()
                .HasOne(uc => uc.User)
                .WithOne()  
                .HasForeignKey<UserCredential>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // удалить информацию о пароле при удалении пользователя
        }
    }
}
