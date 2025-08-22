using System.ComponentModel.DataAnnotations;
namespace SharedApp.Models
{
    public class Supplier
    {
        [Required(ErrorMessage = "Supplier name is required")]
        public required string Name { get; set; }
        public string? Location { get; set; }
    }
}