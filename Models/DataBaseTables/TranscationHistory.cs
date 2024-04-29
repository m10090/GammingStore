using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace gammingStore.Models;

// index or Delivered column
[Index(nameof(Delivered))]
public record TranscationHistory
{
    public int Id { get; set; }
    public int TranscationId { get; set; }
    [ForeignKey("UserId")]
    public int UserId { get; set; }
    [ForeignKey("ProductId")]
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public bool Delivered { get; set; } = false;
    virtual public User User { get; set; } = null!;
    virtual public Product Product { get; set; } = null!;
}
