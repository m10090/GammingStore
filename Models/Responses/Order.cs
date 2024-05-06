namespace gammingStore.Models;

public record Order {
  public int Id { get; set; }
  public string UserName { get; set; } = "";
  public string ProductName { get; set; } = "";
  public double Price { get; set; }
  public int Quantity { get; set; }
  public DateTime TranscationDate { get; set; } = DateTime.Now;
  public int TranscationId { get; set; } = 0;
  public bool Delivered { get; set; }
}
