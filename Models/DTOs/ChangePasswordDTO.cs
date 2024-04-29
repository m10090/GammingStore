namespace gammingStore.Models;


public record ChangePasswordDTO {
  public string CurrentPassword { get; set; } = "";
  public string NewPassword { get; set; } = "";
  public string ConfirmPassword { get; set; } = "";
}
