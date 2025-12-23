using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using static micpix.Server.Resources;
using System.Collections.Generic;

namespace micpix.Server
{
    class AppDbContext : DbContext
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
        public DbSet<Collages> Collages { get; set; }
        public DbSet<Layers> Layers { get; set; }
        public DbSet<ResultGIFs> ResultGIFs { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<ResourceCategoryTags> ResourceCategoryTags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resources>().ToTable("Resources");
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Collages>().ToTable("Collages");
            modelBuilder.Entity<Layers>().ToTable("Layers");
            modelBuilder.Entity<ResultGIFs>().ToTable("ResultGIFs");
            modelBuilder.Entity<Categories>().ToTable("Categories");
            modelBuilder.Entity<ResourceCategoryTags>().ToTable("ResourceCategoryTags");

            modelBuilder.Entity<ResourceCategoryTags>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(rt => rt.Category)
                      .WithMany()  
                      .HasForeignKey(rt => rt.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rt => rt.Resource)
                      .WithMany(r => r.Tags)  
                      .HasForeignKey(rt => rt.ResourceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Categories>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Children)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });

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

            modelBuilder.Entity<Collages>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collages>()
                .HasMany(c => c.Layers)
                .WithOne(l => l.Collage)
                .HasForeignKey(l => l.CollageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Layers>()
                .HasOne(l => l.Resource)
                .WithMany()
                .HasForeignKey(l => l.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Collages>()
                .HasMany(c => c.ResultGIFs)
                .WithOne(g => g.Collage)
                .HasForeignKey(g => g.CollageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
