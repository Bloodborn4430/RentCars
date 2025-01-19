
using Microsoft.EntityFrameworkCore;
using Shop.Models;

namespace Maxx_Speed.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Car> Cars => Set<Car>();  // Изменено с Product на Car

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderCar> OrderCars => Set<OrderCar>();  // Если вы хотите назвать таблицу OrderCar

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Category-Car: One-to-Many
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Category)  
                .WithMany(cat => cat.Cars)  
                .HasForeignKey(c => c.CategoryId); 

            // Order-OrderCar: One-to-Many
            modelBuilder.Entity<OrderCar>()
                .HasOne(oc => oc.Order)
                .WithMany(o => o.OrderCars)  
                .HasForeignKey(oc => oc.OrderId);

            // Car-OrderCar: One-to-Many
            modelBuilder.Entity<OrderCar>()
                .HasOne(oc => oc.Car)  
                .WithMany(c => c.OrderProducts)  
                .HasForeignKey(oc => oc.CarId);  

            base.OnModelCreating(modelBuilder);
        }
    }
}
