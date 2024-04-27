using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

[Authorize("Employees")]
public partial class EmployeesController : Controller {
  private readonly DB db;

  public EmployeesController(DB db) { this.db = db; }

  // Views for Employees
  public IActionResult Index() { return View(); }

  public IActionResult Products() { return View(); }

  public IActionResult Orders() {
    var orders = db.Historys
                     .Join(db.products, h => h.ProductId, p => p.ProductId,
                           (h, p) => new { h, p })
                     .Join(db.users, hp => hp.h.UserId, u => u.UserId,
                           (hp, u) => new { hp, u })
                     .Select(x => new Order {
                       Id = x.hp.h.Id,
                       UserName = x.u.Username,
                       ProductName = x.hp.p.Name,
                       Price = x.hp.p.Price,
                       TranscationDate = x.hp.h.Date,
                       TranscationId = x.hp.h.TranscationId,
                       Quantity = x.hp.h.Quantity,
                     })
                     .ToList();
    return View(orders);
  }

  [HttpGet("Employees/EditProduct/{id}")]
  public IActionResult EditProduct(int id) {
    var product = db.products.FirstOrDefault(p => p.ProductId == id);
    return View(product);
  }
  [HttpGet("Employees/DeleteProduct/{id}")]
  public IActionResult DeleteProduct(int id) {
    var product = db.products.FirstOrDefault(p => p.ProductId == id);
    if (product == null) {
      return NotFound("Product not found");
    }
    product.IsDeleted = true;
    db.products.Update(product);
    db.SaveChanges();
    return RedirectToAction("Products");
  }
  [HttpGet("Employees/AddProduct")]
  public IActionResult AddProduct() { return View(); }


}
