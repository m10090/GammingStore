namespace gammingStore.Models;

public record ProductDTO : Product
{
    public IFormFile ImageFile { get; set; } = null!;
}
