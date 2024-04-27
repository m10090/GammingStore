namespace gammingStore.Models;

public record MessageResponse {
  public string Message { get; set; } = "";
  public string ? Status { get; set; }
  public string ? Fixes { get; set; }
}
