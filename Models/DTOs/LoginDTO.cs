namespace gammingStore.Models;

public record LoginDTO
{
    public string username { get; set; } = "";
    public string password { get; set; } = "";
}
