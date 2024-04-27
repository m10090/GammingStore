using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers {
[Authorize("Employees")]
public partial class EmployeesController : Controller {
  // todo: AddProduct
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

    using (var stream = new FileStream($"wwwroot/images/{productObj.ProductId}",
                                       FileMode.Create)) {
      productRequest.ImageFile.CopyTo(stream);
    }
    db.SaveChanges();
    return RedirectToAction("Products");
  }

  [HttpPost("Employees/EditProduct/{id}")]
  public IActionResult UpdateProduct([FromForm] ProductDTO productRequest,
                                     int id) {
    var product = db.products.FirstOrDefault(p => p.ProductId ==
                                                  productRequest.ProductId);
    // to do:
    return RedirectToAction("Products");
  }

}
}
