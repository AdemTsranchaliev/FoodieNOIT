using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Foodie1.Models;


namespace Foodie1.Data
{
    public class FoodieContext : IdentityDbContext<User>
    {
        public FoodieContext()
        {

        }

        public FoodieContext(DbContextOptions<FoodieContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Town> Towns { get; set; }
        public DbSet<TypeRestaurant> TypeRestaurants { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RestaurantCategory> RestaurantCategories { get; set; }
        public DbSet<Ingredients> Ingredients { get; set; }
        public DbSet<ProductIngredients> ProductIngredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer("Server=DESKTOP-Q3A1J42\\SQLEXPRESS;Database=Foodie;Integrated Security=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {


            builder.Entity<Address>()
                .HasOne(x => x.Town)
                .WithMany(y => y.Addresses)
                .HasForeignKey(x => x.TownId);

            builder.Entity<Restaurant>()
               .HasOne(x => x.Address)
               .WithOne(y => y.Restaurant)
               .HasForeignKey<Restaurant>(x => x.AddressId);

            builder.Entity<Restaurant>()
             .HasMany(x => x.MenuCategories)
             .WithOne(y => y.Restaurant)
             .HasForeignKey(x => x.RestaurantId);

            builder.Entity<Product>()
             .HasOne(x => x.MenuCategory)
             .WithMany(y => y.Products)
             .HasForeignKey(x => x.MenuCategoryId);

            builder.Entity<Restaurant>()
           .HasOne(x => x.TypeRestaurant)
           .WithMany(y => y.Restaurants)
           .HasForeignKey(x => x.TypeRestaurantId);

            builder.Entity<User>()
                .HasOne(x => x.Restaurant)
                .WithOne(y => y.User)
                .HasForeignKey<Restaurant>(x => x.UserId);

            builder.Entity<UserReview>()
                .HasOne(x => x.Review)
                .WithMany(y => y.UserReviews)
                .HasForeignKey(x => x.ReviewId);

            builder.Entity<UserReview>()
               .HasOne(x => x.User)
               .WithMany(y => y.UserReviews)
               .HasForeignKey(x => x.UserId);

            builder.Entity<UserReview>()
               .HasKey(x => new { x.UserId, x.ReviewId });

            builder.Entity<Restaurant>()
           .HasMany(x => x.Reviews)
           .WithOne(y => y.Restaurant)
           .HasForeignKey(x => x.RestaurantId);


            builder.Entity<Restaurant>()
          .HasMany(x => x.Reservations)
          .WithOne(y => y.Restaurant)
          .HasForeignKey(x => x.RestaurantId);

            builder.Entity<User>()
         .HasMany(x => x.Reservations)
         .WithOne(y => y.User)
         .HasForeignKey(x => x.UserId);

            builder.Entity<OrderProduct>()
               .HasOne(x => x.Order)
               .WithMany(y => y.OrderProducts)
               .HasForeignKey(x => x.OrderId);

            builder.Entity<OrderProduct>()
               .HasOne(x => x.Product)
               .WithMany(y => y.OrderProducts)
               .HasForeignKey(x => x.ProductId);

            builder.Entity<OrderProduct>()
               .HasKey(x => new { x.ProductId, x.OrderId });

            builder.Entity<RestaurantCategory>()
           .HasOne(x => x.Category)
           .WithMany(y => y.RestaurantCategories)
           .HasForeignKey(x => x.CategoryId);

            builder.Entity<RestaurantCategory>()
               .HasOne(x => x.Restaurant)
               .WithMany(y => y.RestaurantCategories)
               .HasForeignKey(x => x.RestaurantId);

            builder.Entity<RestaurantCategory>()
               .HasKey(x => new { x.RestaurantId, x.CategoryId });

            builder.Entity<ProductIngredients>()
           .HasOne(x => x.Ingredients)
           .WithMany(y => y.ProductIngredients)
           .HasForeignKey(x => x.IngredientId);

            builder.Entity<ProductIngredients>()
               .HasOne(x => x.Product)
               .WithMany(y => y.ProductIngredients)
               .HasForeignKey(x => x.ProductId);

            builder.Entity<ProductIngredients>()
               .HasKey(x => new { x.ProductId, x.IngredientId });

            base.OnModelCreating(builder);
        }
    }
}
