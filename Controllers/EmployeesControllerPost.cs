using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

[Authorize(Policy = "Employees")]
public partial class EmployeesController : Controller {
  [HttpPost]
  public IActionResult AddProduct([FromForm] ProductDTO productRequest) {
    // save file IFormFile

    var productObj = new Product {
      Name = productRequest.Name,
      Price = productRequest.Price,
      Description = productRequest.Description,
    };
    productObj = db.products.Add(productRequest).Entity;
    // add image to wwwroot/images
    db.SaveChanges();
    var filePath =
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images",
                     productObj.ProductId.ToString() + ".jpeg");
    using (var stream = new FileStream(filePath, FileMode.Create)) {
      productRequest.ImageFile.CopyTo(stream);
    }
    return RedirectToAction("Products");
  }

  [HttpPost("Employees/EditProduct/{id}")]
  public IActionResult UpdateProduct([FromForm] ProductDTO productRequest,
                                     int id) {
    var product = db.products.FirstOrDefault(p => p.ProductId == id);
    if (product == null) {
      return NotFound("Product not found");
    }
    product.IsDeleted = true;
    db.products.Update(product);

    var productObj = db.products.Add(productRequest).Entity;
    // rename image in wwwroot/images/{id}
    db.SaveChanges();
    System.IO.File.Move($"wwwroot/images/{id}.jpeg",
                        $"wwwroot/images/{productObj.ProductId}.jpeg");

    return RedirectToAction("Products", "Employees");
  }

  [HttpPost]
  [Authorize(Roles = "Admin")]
  public IActionResult AddUser([FromForm] UserDTO user) {
    db.users.Add(user);
    user.Password = passwordHasher.HashPassword(user, user.Password);
    db.SaveChanges();
    return RedirectToAction("Users");
  }

  [HttpPost("Employees/EditUser/{id}")]
  public IActionResult UpdateUser([FromForm] UserDTO user, int id) {
    var userObj = db.users.FirstOrDefault(u => u.UserId == id && !u.IsDeleted);
    if (userObj == null) {
      return NotFound("User not found");
    }
    userObj.FullName = user.FullName;
    userObj.Email = user.Email;
    userObj.Address = user.Address;
    userObj.Password = passwordHasher.HashPassword(userObj, userObj.Password);
    db.users.Update(userObj);
    db.SaveChanges();
    return RedirectToAction("Users", "Employees");
  }
}
