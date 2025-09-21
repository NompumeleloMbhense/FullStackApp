namespace SharedApp.Models.Dto
{
    public class SupplierReadDto
    {
        // Used for listing suppliers (GET: api/suppliers)
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    // Used for detailed view of a supplier with its products (GET: api/suppliers/{id})
    public class SupplierWithProductsDto : SupplierReadDto
    {
        public List<ProductForSupplierDto> Products { get; set; } = new();
    }

    // Focused product info for supplier details
    public class ProductForSupplierDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool Available { get; set; }
    }

    // Used for creating a new supplier (POST: api/suppliers)
    public class SupplierCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    // Used for updating an existing supplier (PUT: api/suppliers/{id})
    public class SupplierUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
    
     // Used for partial update (PATCH: api/suppliers/{id})
    public class SupplierPatchDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
    }

}