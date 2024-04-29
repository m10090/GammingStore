using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers {
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
    Console.WriteLine(filePath);
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

    product = db.products.Add(productRequest).Entity;
    // rename image in wwwroot/images/{id}
    db.SaveChanges();
    System.IO.File.Move($"wwwroot/images/{id}.jpeg",
                        $"wwwroot/images/{product.ProductId}.jpeg");

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
}
}
