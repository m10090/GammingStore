using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using gammingStore.Data;
using gammingStore.Models;

namespace gammingStore.Services.Payment;

public class Payment {
  HttpClient client = new HttpClient();
  string Currency, ApiKey, PrivateKey, PublicKey, HMAC;
  string authToken = "";
  int UserId;
  int intergrationId;
  int Amount;
  int ee;
  public string clientSecret { set; get; } = "";
  int TransactionId; 
  IEnumerable<CartDTO> Cart;

  public Payment(IEnumerable<CartDTO> Cart, string ApiKey, string HMAC,
                 string PrivateKey, string PublicKey, int Amount, int intergrationId,
                 int UserId = 0, string Currency = "EGP") {
    this.Cart = Cart;
    this.Amount = Amount;
    this.Currency = Currency;
    this.ApiKey = ApiKey;
    this.PrivateKey = PrivateKey;
    this.HMAC = HMAC;
    this.PublicKey = PublicKey;
    this.UserId = UserId;
    this.intergrationId = intergrationId;
    getToken();
  }

  public async Task<string> MakePayment() {
    var request = new HttpRequestMessage(
        HttpMethod.Post, "https://accept.paymob.com/v1/intention/");
    Console.WriteLine(this.intergrationId);
    var payload =
        new { amount = this.Amount, currency = Currency,
              payment_methods = new dynamic[] { "cart", this.intergrationId },
              shipping_data =
                  new { apartment = "6", first_name = "Ammar",
                        last_name = "Sadek", street = "938, Al-J",
                        building = "939", phone_number = "+96824480228",
                        country = "EG", floor = "1", state = "" },
              billing_data = new {
                first_name = "Ammar",
                last_name = "Sadek",
                email = "AmmarSadek@gmail.com",
                phone_number = "+96824480228",
                extras = new { re = "22" },
              } };
    var jsonPayload = JsonSerializer.Serialize(payload);

    request.Content = new StringContent(jsonPayload, null, "application/json");
    request.Headers.Add("Authorization", "Token " + PrivateKey);
    var response = await client.SendAsync(request);
    if (!response.IsSuccessStatusCode || response.Content == null) {
      Console.WriteLine(
          $"Failed to create payment link {await response.Content.ReadAsStringAsync()} ");
      return "";
    }
    var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    //  print the body as a json string
    Console.WriteLine(body.RootElement.ToString());
    this.clientSecret =
        body.RootElement.GetProperty("client_secret").GetString();
    var res =
        $"https://accept.paymob.com/unifiedcheckout/?publicKey={PublicKey}&clientSecret={clientSecret}";
    return res;
  }

  private async void getToken() {
    var request = new HttpRequestMessage(
        HttpMethod.Post, "https://accept.paymob.com/api/auth/tokens");
    request.Content = new StringContent("{\"api_key\":\"" + this.ApiKey + "\"}",
                                        null, "application/json");

    var response = await client.SendAsync(request);
    var res = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    this.authToken = res.RootElement.GetProperty("token").GetString();
  }

  public bool verifyPayment(JsonDocument body) {
    // use this data in this order for the HMAC
    string bodyString = "";
    // (Amount in cents / 100) formatted to 2 decimal places +intention.id
    bodyString += (this.Amount / 100).ToString("0.00") +
                  body.RootElement.GetProperty("intention.id").GetString();
    this.TransactionId = Convert.ToInt32(body.RootElement.GetProperty("transaction.id").GetString());
    using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(this.HMAC))) {
      var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(bodyString));
      // Convert the resultant HMAC is Hex (base 16) lowercase.
      var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
      if (hashString != body.RootElement.GetProperty("hmac").GetString()) {
        return false;
      } else {
        instertIntoTransactionTable();
        return true;
      }
    }
  }

  private void instertIntoTransactionTable() {
    var db = new DB();
    foreach (var item in Cart) {
      db.historys.Add(new TranscationHistory {
        UserId = this.UserId,
        TranscationId = Convert.ToInt32(this.TransactionId),
        ProductId = item.id,
        Quantity = item.quantity,
      });
      var product = db.products.FirstOrDefault(p => p.ProductId == item.id);
      if (product == null || product.IsDeleted) {
        return;
      }
      product.Stock -= item.quantity;
      if (product.Stock == 0) {
        product.IsDeleted = true;
      }
      db.products.Update(product);
    }
    db.SaveChangesAsync();
  }
}
