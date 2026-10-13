using System.ComponentModel.DataAnnotations;

namespace Inventory.API.Models;

public class ItemRequestDto
{
    [Required(ErrorMessage = "itemCode wajib diisi")]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "itemName wajib diisi")]
    [StringLength(150)]
    public string ItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "category wajib diisi")]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Price tidak boleh negatif")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock tidak boleh negatif")]
    public int Stock { get; set; }
}

public class ItemResponseDto : ItemRequestDto
{
    public int Id { get; set; }
}
