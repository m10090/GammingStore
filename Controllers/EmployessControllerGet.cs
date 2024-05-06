using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

[Authorize(Policy = "Employees")]
public partial class EmployeesController
{
    private readonly DB db = null!;
    private readonly PasswordHasher<User> passwordHasher =
        new PasswordHasher<User>();

    public EmployeesController(DB db) { this.db = db; }

    // Views for Employees
    public IActionResult Index() { return View(); }

    public IActionResult Users()
    {
        var users = db.users.Where(x => !x.IsDeleted).Take(10).ToList();
        return View(users);
    }

    [HttpGet("Employees/EditProduct/{id}")]
    public IActionResult EditProduct(int id)
    {
        var product = db.products.FirstOrDefault(p => p.ProductId == id);
        return View(product);
    }

    [HttpGet("Employees/EditUser/{id}")]
    public IActionResult EditUser(int id)
    {
        var user = db.users.FirstOrDefault(p => p.UserId == id);
        return View(user);
    }

    public IActionResult Products()
    {
        var products = db.products.Where((x) => !x.IsDeleted).ToList();
        return View(products);
    }

    public IActionResult Orders()
    {
        var orders = db.historys
                         .Join(db.products, h => h.ProductId, p => p.ProductId,
                               (h, p) => new { h, p })
                         .Join(db.users, hp => hp.h.UserId, u => u.UserId,
                               (hp, u) => new { hp, u })
                         .Select(x => new Order
                         {
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

    [Authorize(Policy = "Admin")]
    public IActionResult AddUser() { return View(); }

    [HttpGet("Employees/DeleteProduct/{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = db.products.FirstOrDefault(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound("Product not found");
        }
        product.IsDeleted = true;
        db.products.Update(product);
        db.SaveChanges();
        return RedirectToAction("Products");
    }

    public IActionResult AddProduct() { return View(); }

    public IActionResult UnderDelivery()
    {
        var undelivered = db.historys.Where((x) => !x.Delivered)
                              .Join(db.products, h => h.ProductId, p => p.ProductId,
                                    (h, p) => new { h, p })
                              .Join(db.users, hp => hp.h.UserId, u => u.UserId,
                                    (hp, u) => new { hp, u })
                              .Select(x => new Order
                              {
                                  Id = x.hp.h.Id,
                                  UserName = x.u.Username,
                                  ProductName = x.hp.p.Name,
                                  Price = x.hp.p.Price,
                                  TranscationDate = x.hp.h.Date,
                                  TranscationId = x.hp.h.TranscationId,
                                  Quantity = x.hp.h.Quantity,
                              })
                              .ToList();

        return View(undelivered);
    }

    [HttpGet("Employees/Delivered/{id}")]
    public IActionResult Delivered(int id)
    {
        var historyObj =
            db.historys.FirstOrDefault(x => x.Id == id);
        if (historyObj == null)
        {
            return NotFound("Order not found");
        }
        if (historyObj.Delivered)
        {
            return BadRequest("Order already delivered");
        }
        historyObj.Delivered = true;
        db.historys.Update(historyObj);
        db.SaveChanges();
        return RedirectToAction("UnderDelivery");
    }

    [HttpGet("Employees/DeleteUser/{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = db.users.FirstOrDefault(p => p.UserId == id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        user.IsDeleted = true;
        db.users.Update(user);
        db.SaveChanges();
        return RedirectToAction("Users", "Employees");
    }
}
