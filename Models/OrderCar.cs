using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop.Models
{
    public class OrderCar
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int CarId { get; set; }
        [Required]
        public int Amount { get; set; }

        [ForeignKey(nameof(OrderId))]
        [JsonIgnore]
        public Order Order { get; set; }

        [ForeignKey(nameof(CarId))]
        [JsonIgnore]
        public Car Car { get; set; }
    }
}
