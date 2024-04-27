using System.ComponentModel.DataAnnotations;
namespace gammingStore.Models;

public record User {
  public int UserId { get; set; }
  public string Username { get; set; }= "";
  public string Password { get; set; }= "";
  public string FullName { get; set; }= "";
  public string Email { get; set; }= "";
  public string Role { get; set; } = "User";
  public string Address { get; set; } = "";
}
