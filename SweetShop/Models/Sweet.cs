using Microsoft.AspNetCore.Http;

namespace SweetShop.Mvc.Models
{
    public class Sweet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; }

        // For create/edit forms
        public IFormFile? ImageFile { get; set; }
    }
}
