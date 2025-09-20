using System.ComponentModel.DataAnnotations;

namespace SweetShop.API.Models
{
    public class Sweet
    {
        [Key] // Primary key
        public int Id { get; set; }

        [Required] // Name mandatory
        [StringLength(100)] // Max length 100 chars
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, 100000)] // Price range
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [StringLength(800)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
