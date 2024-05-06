using System.Security.Claims;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

[Authorize]
public class PurchaseController : Controller
{
    private readonly DB db;

    public PurchaseController(DB db) { this.db = db; }

    public IActionResult Index() { return View(); }

    [HttpPost]
    public ActionResult Index([FromBody] List<CartDTO> cart)
    {
        if (cart == null || cart.Count == 0 )
        {
            return BadRequest(new { message = "Cart is empty" });
        }
        var cartResponse = new List<CartResponse>();
        foreach (var item in cart)
        {
            var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }
            if (item.quantity < 1)
            {
                return BadRequest(new { message = "Quantity must be greater than 0" });
            }
            cartResponse.Add(new CartResponse
            {
                id = item.id,
                quantity = item.quantity,
                name = product.Name,
                price = product.Price,
                description = product.Description,
            });
        }
        return Ok(cartResponse);
    }

    [HttpPost]
    public IActionResult Checkout([FromBody] List<CartDTO> cart)
    {
        if (cart == null || cart.Count == 0 )
        {
            return BadRequest(new { message = "Cart is empty" });
        }
        double totalCost = 0;
        var userId = Convert.ToInt32(
            User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value);
        // get random transcation id that is unique
        int transcationId;
        do
        {
            transcationId = new Random().Next(100000, 999999);
        } while (db.historys.Any(h => h.TranscationId == transcationId));

        foreach (var item in cart)
        {
            var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
            if (product == null || product.IsDeleted)
            {
                return NotFound(new { message = "Product not found" });
            }
            if (product.Stock < item.quantity)
            {
                return BadRequest(
                    new { message = $"Not enough quantity of {product.Name}" });
            }
            if (product.Stock == item.quantity)
            {
                product.IsDeleted = true;
            }
            product.Stock -= item.quantity;
            totalCost += product.Price * item.quantity;
            db.products.Update(product);
            var history = new TranscationHistory
            {
                UserId = userId,
                ProductId = product.ProductId,
                Quantity = item.quantity,
                Date = DateTime.Now,
                TranscationId = transcationId,
            };
            db.historys.Add(history);
        }
        db.SaveChanges();
        return Ok(new { message = "Checkout successful", TotalCost = totalCost });
    }
}
