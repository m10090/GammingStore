using System.ComponentModel.DataAnnotations.Schema;

namespace gammingStore.Models;

public class GiftCard {
  public int Id { get; set; }

  [Column(TypeName = "decimal(9, 2)")]
  public decimal Discount { get; set; }
  public DateTime StartDate { get; set; } = DateTime.Now;
  public DateTime EndDate { get; set; }
  public bool IsActive { get; set; }
}
