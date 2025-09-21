using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SharedApp.Models;

namespace SharedApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        
        public int Stock { get; set; }

        public string Category { get; set; } = string.Empty;
        [JsonIgnore]
        public Supplier? Supplier { get; set; } = default!;
        public int SupplierId { get; set; }
        public bool Available => Stock > 0;

    }
}