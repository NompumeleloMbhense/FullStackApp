using System.ComponentModel.DataAnnotations;
using SharedApp.Models;

namespace SharedApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        
        public int Stock { get; set; }

        public List<string>? Categories { get; set; }
        public Supplier? Supplier { get; set; }
        public bool Available => Stock > 0;

    }
}