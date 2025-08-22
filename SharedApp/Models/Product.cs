using System.ComponentModel.DataAnnotations;
using SharedApp.Models;

namespace SharedApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public List<string>? Categories { get; set; }
        public Supplier? Supplier { get; set; }
        public bool Available => Stock > 0;

    }
}