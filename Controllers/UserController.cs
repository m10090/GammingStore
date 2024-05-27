using System.Security.Claims;
using System.Text.RegularExpressions;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

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
    var userObj = db.users.FirstOrDefault(
        u => u.Username.ToLower() == user.username.ToLower() && !u.IsDeleted);
    if (userObj == null || userObj.IsDeleted) {
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
                                   new Claim(ClaimTypes.NameIdentifier,
                                             userObj.UserId.ToString()),
                                   new Claim(ClaimTypes.Role, userObj.Role) };
    var Identity = new ClaimsIdentity(
        claims, CookieAuthenticationDefaults.AuthenticationScheme);
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ==
        "Development") {
      Console.WriteLine("Development");
      Console.WriteLine(userObj.Role);
    }
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
    string script = @"
        <script type='text/javascript'>
            localStorage.removeItem('cart');
            window.location.href = '/Home';
        </script>
    ";
    return Content(script, "text/html");
  }

  // userProfile
  [Authorize]
  public IActionResult Index() {
    if (User.Identity?.Name == null) {
      return RedirectToAction("Login");
    }
    var user = db.users.FirstOrDefault(u => u.Username == User.Identity.Name &&
                                            !u.IsDeleted);
    if (user == null) {
      return RedirectToAction("Login");
    }
    var orders = db.historys
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
    return View(orders);
  }

  public IActionResult Register() { return View(); }

  [HttpPost]
  public ActionResult<MessageResponse> Register([FromBody] UserDTO user) {
    // validation
    var regx = new Regex(@"[!#$%^&*()_+@]");
    if (user.Password.Length < 8 || !regx.IsMatch(user.Password) ||
        !user.Password.Any(char.IsDigit) || !user.Password.Any(char.IsUpper) ||
        !user.Password.Any(char.IsLower) || !user.Password.Any(char.IsLetter)) {
      return StatusCode(
          406, new MessageResponse { Message = "Password is not valid" });
    }
    var emailRegx = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    if (user.Email == null || !emailRegx.IsMatch(user.Email)) {
      return StatusCode(406,
                        new MessageResponse { Message = "Email is not valid" });
    }

    if (db.users.Any((x) => x.Username.ToLower() == user.Username.ToLower())) {
      return StatusCode(
          406, new MessageResponse { Message = "Username is allready taken" });
    }

    user.Password = passwordHasher.HashPassword(user, user.Password);
    var userObj = new User {
      Username = user.Username.ToLower(),
      Password = user.Password,
      FullName = user.FullName,
      Email = user.Email,
      Role = "User",
      Address = user.Address,
    };
    db.users.Add(userObj);
    db.SaveChangesAsync();

    return StatusCode(
        201, new MessageResponse { Message = "User created successfully" });
  }

  [Authorize]
  public IActionResult EditProfile() {
    if (User.Identity?.Name == null) {
      return RedirectToAction("Login");
    }
    var user = db.users.FirstOrDefault(
        u => u.Username == User.Identity.Name.ToLower() && !u.IsDeleted);
    return View(user);
  }

  [HttpPost]
  [Authorize]
  public IActionResult EditProfile([FromForm] UserDTO user) {
    if (User.Identity?.Name == null) {
      return RedirectToAction("Login");
    }
    var userObj = db.users.FirstOrDefault(u => u.Username ==
                                               User.Identity.Name.ToLower());
    if (userObj == null) {
      return RedirectToAction("Login");
    }
    if (passwordHasher.VerifyHashedPassword(userObj, userObj.Password,
                                            user.Password) ==
        PasswordVerificationResult.Failed) {
      return Content(@"<h1>Password is not correct</h1>
          <script>
              setTimeout(() => {
                  window.location.href = '/User/EditProfile';
              }, 2000);
          </script>",
                     "text/html");
    }
    userObj.FullName = user.FullName;
    userObj.Email = user.Email;
    userObj.Address = user.Address;
    userObj.Password = passwordHasher.HashPassword(userObj, user.Password);
    db.users.Update(userObj);
    db.SaveChanges();
    return RedirectToAction("Index");
  }

  [Authorize]
  public IActionResult ChangePassword() { return View(); }

  [HttpPost]
  [Authorize]
  public ActionResult<MessageResponse>
  ChangePassword([FromBody] ChangePasswordDTO user) {
    if (User.Identity?.Name == null) {
      return RedirectToAction("Login");
    }
    var userObj = db.users.FirstOrDefault(u => u.Username ==
                                               User.Identity.Name.ToLower());
    if (userObj == null || userObj.IsDeleted ) {
      return RedirectToAction("Login");
    }
    if (passwordHasher.VerifyHashedPassword(userObj, userObj.Password,
                                            user.oldPassword) ==
        PasswordVerificationResult.Success) {
      userObj.Password = passwordHasher.HashPassword(userObj, user.newPassword);
    } else {
      return StatusCode(
          406, new MessageResponse { Message = "Old password is not correct" });
    }
    db.users.Update(userObj);
    db.SaveChanges();
    return Ok(
        new MessageResponse { Message = "Password changed successfully" });
  }
}
