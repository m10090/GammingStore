using System.ComponentModel.DataAnnotations.Schema;
namespace gammingStore.Models;

// add constrain if product stock == 0 then IsDeleted = true
public record Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [Column(TypeName = "decimal(9, 2)")]
    public double Price { get; set; } = 0;
    public int Stock { get; set; } = 0;
    public bool IsDeleted { get; set; } = false;
}
