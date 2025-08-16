using SharedApp.Models;

namespace SharedApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
        public List<string>? Categories { get; set; }
        public Supplier? Supplier { get; set; }
        public bool Available => Stock > 0;

    }
}