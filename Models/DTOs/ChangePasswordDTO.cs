namespace gammingStore.Models;


public record ChangePasswordDTO
{
    public string oldPassword { get; set; } = "";
    public string newPassword { get; set; } = "";
}
