using System.Security.Claims;
using System.Text.RegularExpressions;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers {
public class UserController : Controller {
  private readonly DB db;
  private readonly PasswordHasher<User> passwordHasher =
      new PasswordHasher<User>();
  private readonly IConfiguration _config;

  public UserController(DB db, IConfiguration config) {
    this.db = db;
    this.db.Database.EnsureCreated();
    this._config = config;
  }

  public IActionResult Login() { return View(); }

  [HttpPost]
  public async Task<ActionResult<MessageResponse>>
  Login([FromBody] LoginDTO user) {
    var userObj = db.users.FirstOrDefault(u => u.Username.ToLower() ==
                                               user.username.ToLower());
    if (userObj == null) {
      return StatusCode(406,
                        new MessageResponse { Message = "User not found" });
    }
    if (passwordHasher.VerifyHashedPassword(userObj, userObj.Password,
                                            user.password) ==
        PasswordVerificationResult.Failed) {
      return StatusCode(
          406, new MessageResponse { Message = "Password is not correct" });
    }

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, userObj.Username),
                                   new Claim(ClaimTypes.Role, userObj.Role) };
    var Identity = new ClaimsIdentity(
        claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(Identity);
    var authProperties = new AuthenticationProperties {
      IsPersistent = true,
      ExpiresUtc = DateTimeOffset.UtcNow.AddDays(10),
    };
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme, principal,
        authProperties);
    return Ok(new MessageResponse { Message = "Login Success" });
  }

  [Authorize]
  public IActionResult Logout() {
    HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
  }

  // userProfile
  [Authorize]
  public IActionResult Index() {
    var user = db.users.FirstOrDefault(u => u.Username == User.Identity.Name);
    var orders = db.Historys
                     .Join(db.products, h => h.ProductId, p => p.ProductId,
                           (h, p) => new { h, p })
                     .Where(x => x.h.UserId == user.UserId)
                     .Select(x => new Order {
                       Id = x.h.Id,
                       ProductName = x.p.Name,
                       Price = x.p.Price,
                       TranscationDate = x.h.Date,
                       TranscationId = x.h.TranscationId,
                       Quantity = x.h.Quantity,
                     })
                     .OrderByDescending((x) => x.TranscationDate)
                     .Take(5)
                     .ToList();
    return View();
  }

  public IActionResult Register() { return View(); }

  [HttpPost]
  public ActionResult<MessageResponse> Register([FromBody] RegisterDTO user) {
    // validation
    var regx = new Regex(@"[\[\]@-_!#$%^&*()<>?/\|}{~:]");
    if (user.Password.Length < 8 || !regx.IsMatch(user.Password) ||
        !user.Password.Any(char.IsDigit) || !user.Password.Any(char.IsUpper) ||
        !user.Password.Any(char.IsLower) ||
        user.Password.Any(char.IsWhiteSpace) ||
        !user.Password.Any(char.IsLetter)) {
      return StatusCode(
          406, new MessageResponse { Message = "Password is not valid" });
    }

    user.Password = passwordHasher.HashPassword(user, user.Password);
    var userObj = new User {
      Username = user.Username,
      Password = user.Password,
      FullName = user.FullName,
      Email = user.Email,
      Role = "User",
      Address = user.Address,
    };

    db.users.Add(userObj);
    db.SaveChangesAsync();
    return Ok(new MessageResponse { Message = "User created successfully" });
  }
}
}
