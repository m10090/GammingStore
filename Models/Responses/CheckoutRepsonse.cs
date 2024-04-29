namespace gammingStore.Models;
public record CartResponse
{
    // naming is not conventional because this will go to the client
    public int id { get; set; }
    public int quantity { get; set; }
    public string name { get; set; } = "";
    public double price { get; set; }
    public string description { get; set; } = "";
}
