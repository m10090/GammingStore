using System.Security.Claims;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DataBase
// ensure database is created
builder.Services.AddDbContext<DB>();

// add admin to database
var db = builder.Services.BuildServiceProvider().GetService<DB>();
if (db.users.FirstOrDefault(u => u.Username == "admin") == null) {
  var admin = new User {
  };
  builder.Configuration.GetSection("Admin").Bind(admin);
  var passwordHasher = new PasswordHasher<User>();
  admin.Password = passwordHasher.HashPassword(admin, admin.Password);
  db.users.Add(admin);
  db.SaveChanges();
}
// PasswordHasher
builder.Services.AddScoped<PasswordHasher<User>>();

// AddAuthentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
      options.Cookie.Name = "AuthCookie";
      options.Cookie.HttpOnly = true;
      options.LoginPath = "/user/Login";
    });

// add policy
builder.Services.AddAuthorization(options => {
  options.AddPolicy("Employees", policy => {
    policy.RequireClaim(ClaimTypes.Role, "Employees");
    policy.RequireClaim(ClaimTypes.Role, "Admin");
    policy.RequireClaim(ClaimTypes.Role, "Manager");
    policy.RequireClaim(ClaimTypes.Role, "Owner");
    policy.RequireClaim(ClaimTypes.Role, "Accountant");
    policy.RequireClaim(ClaimTypes.Role, "HR");
    policy.RequireClaim(ClaimTypes.Role, "Sales");
  });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for
  // production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default",
                       pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
