using Microsoft.EntityFrameworkCore;

namespace gammingStore.Models;

// index or Delivered column
[Index(nameof(Delivered))]
public record TranscationHistory {
  public int Id { get; set; }
  public int TranscationId { get; set; }
  public int UserId { get; set; }
  public int ProductId { get; set; }
  public int Quantity { get; set; }
  public DateTime Date { get; set; } = DateTime.Now;

  public bool Delivered { get; set; } = false;
}
