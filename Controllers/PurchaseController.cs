using System.Security.Claims;
using System.Text.Json;
using gammingStore.Data;
using gammingStore.Models;
using gammingStore.Services.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gammingStore.Controllers;

internal class Payment_ {
  string apiKey;
  string HMAC;
  string privateKey;
  string publicKey;
  int integrationId;

  public Payment_(string apiKey, string HMAC, string privateKey,
                  string publicKey, int integrationId) {
    this.apiKey = apiKey;
    this.HMAC = HMAC;
    this.privateKey = privateKey;
    this.publicKey = publicKey;
    this.integrationId = integrationId;
  }

  public Payment createPayment(List<CartDTO> cart, int amount, int userId) {
    return new Payment(cart, apiKey ?? "", HMAC ?? "", privateKey ?? "",
                       publicKey ?? "", (int)amount * 100, integrationId,
                       userId);
  }
}

[Authorize]
public class PurchaseController : Controller {
  private readonly DB db;
  private readonly IConfiguration configuration;
  private readonly Dictionary<string, Payment> payments =
      new Dictionary<string, Payment>();
  Payment_ payment_;

  public PurchaseController(DB db, IConfiguration configuration) {
    this.db = db;
    this.configuration = configuration;
    this.payment_ =
        new Payment_(configuration["Payment:ApiKey"] ?? "",
                     configuration["Payment:HMAC"] ?? "",
                     configuration["Payment:PrivateKey"] ?? "",
                     configuration["Payment:PublicKey"] ?? "",
                     Convert.ToInt32(configuration["Payment:IntegrationId"]));
  }

  public IActionResult Index() { return View(); }

  [HttpPost]
  public ActionResult Index([FromBody] List<CartDTO> cart) {
    if (cart == null || cart.Count == 0) {
      return BadRequest(new { message = "Cart is empty" });
    }
    var cartResponse = new List<CartResponse>();
    foreach (var item in cart) {
      var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
      if (product == null) {
        return NotFound(new { message = "Product not found" });
      }
      if (item.quantity < 1) {
        return BadRequest(new { message = "Quantity must be greater than 0" });
      }
      cartResponse.Add(new CartResponse {
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
  public IActionResult CashCheckout([FromBody] List<CartDTO> cart) {
    if (cart == null || cart.Count == 0) {
      return BadRequest(new { message = "Cart is empty" });
    }
    double totalCost = 0;
    var userId = Convert.ToInt32(
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value);
    // get random transcation id that is unique
    int transcationId;
    do {
      transcationId = new Random().Next(100000, 999999);
    } while (db.historys.Any(h => h.TranscationId == transcationId));

    foreach (var item in cart) {
      var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
      if (product == null || product.IsDeleted) {
        return NotFound(new { message = "Product not found" });
      }
      if (product.Stock < item.quantity) {
        db.Entry(product).Reload();
        return BadRequest(
            new { message = $"Not enough quantity of {product.Name}" });
      }
      if (product.Stock == item.quantity) {
        product.IsDeleted = true;
      }
      product.Stock -= item.quantity;
      totalCost += product.Price * item.quantity;
      db.products.Update(product);
      var history = new TranscationHistory {
        UserId = userId,
        ProductId = product.ProductId,
        Quantity = item.quantity,
        Date = DateTime.Now,
        TranscationId = transcationId,
      };
      db.historys.Add(history);
    }
    db.SaveChanges();
    return Ok(
        new { message = "CashCheckout successful", TotalCost = totalCost });
  }

  [HttpPost]
  public async Task<IActionResult> CardCheckout([FromBody] List<CartDTO> cart) {
    if (cart == null || cart.Count == 0) {
      return BadRequest(new { message = "Cart is empty" });
    }
    var userId = Convert.ToInt32(
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value);
    // get random transcation id that is unique
    double amount = 0;
    foreach (var item in cart) {
      var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
      if (product == null || product.IsDeleted) {
        return NotFound(new { message = "Product not found" });
      }
      if (product.Stock < item.quantity) {
        db.Entry(product).Reload();
        return BadRequest(
            new { message = $"Not enough quantity of {product.Name}" });
      }
      if (product.Stock == item.quantity) {
        product.IsDeleted = true;
      }
      product.Stock -= item.quantity;
      amount += product.Price * item.quantity;
      db.products.Update(product);
      Console.WriteLine(amount);
    }
    Payment payment = payment_.createPayment(cart, (int)amount * 100, userId);
    var link = await payment.MakePayment();
    payments.Add(payment.clientSecret, payment);
    return Ok(new { link = await payment.MakePayment() });
  }

  [HttpPost]
  public IActionResult PaymentCallback() {
    string json = "";
    using (var reader = new StreamReader(Request.Body)) {
      json = reader.ReadToEnd();
    }

    // Deserialize the JSON to a dynamic object
    var body = JsonDocument.Parse(json);
    // intention.client_secret is the key to the payment object
    payments[body.RootElement.GetProperty("intention.client_secret")
                 .GetString()]
        .verifyPayment(body);

    return Ok();
  }
}
