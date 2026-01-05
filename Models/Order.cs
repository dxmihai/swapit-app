using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vinted2.Models
{
    public enum OrderStatus
    {
        Pending,
        InTransit,
        Completed,
        Canceled
    }

    public class Order
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        public string DeliveryMethod { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}
