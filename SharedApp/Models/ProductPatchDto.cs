using SharedApp.Models;

namespace FullStackApp.Models
{
    public class ProductPatchDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public List<string>? Categories { get; set; }
        public SupplierPatchDto? Supplier { get; set; }
    }
}