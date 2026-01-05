using System.ComponentModel.DataAnnotations;

namespace vinted2.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Product>? Products { get; set; }
    }
}
