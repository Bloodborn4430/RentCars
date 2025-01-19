using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [NotMapped]
        public double TotalPrice => OrderCars?.Sum(op => op.Car.Price * op.Amount) ?? 0;
        [JsonIgnore]
        public ICollection<OrderCar> OrderCars { get; set; } = new List<OrderCar>();
    }
}
