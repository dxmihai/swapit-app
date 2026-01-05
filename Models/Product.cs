using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace vinted2.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        // ❗ NU este obligatoriu (produse fara poze)
        public string? ImagePath { get; set; }

        // ✔ vine din dropdown
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        // ❌ navigation property – NU required
        public Category? Category { get; set; }

        // ❌ setat in controller, NU din form
        public string? UserId { get; set; }

        // ❌ navigation property – NU required
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigations
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
