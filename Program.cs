using System.Security.Claims;
using gammingStore.Data;
using gammingStore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DataBase
// ensure database is created
builder.Services.AddDbContext<DB>();

var db = builder.Services?.BuildServiceProvider()?.GetService<DB>();

// add admin to the database
if (db != null && db.users.FirstOrDefault(u => u.Username == "admin") == null)
{
    var admin = new User { };
    builder.Configuration.GetSection("Admin").Bind(admin);
    var passwordHasher = new PasswordHasher<User>();
    admin.Password = passwordHasher.HashPassword(admin, admin.Password);
    if (db.users.FirstOrDefault(u => u.Username == "admin") == null)
    {
        db.users.Add(admin);
        db.SaveChanges();
    }
}

// PasswordHasher
builder.Services?.AddScoped<PasswordHasher<User>>();

// AddAuthentication
builder.Services?
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.Cookie.HttpOnly = true;
        options.LoginPath = "/user/Login";
    });

// add policy
builder.Services?.AddAuthorization(options =>
{
    options.AddPolicy("Employees",
                      policy => policy.RequireClaim(ClaimTypes.Role, "Employees",
                                                    "Admin", "Manager", "Owner",
                                                    "Accountant", "HR", "Sales"));
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role,
                                                             "Admin", "Owner"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
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
