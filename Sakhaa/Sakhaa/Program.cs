using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sakhaa.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Connection 
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Create necessary directories if they don't exist
try
{
    // For beneficiaries documents
    var beneficiaryDocsDir = Path.Combine(app.Environment.WebRootPath, "uploads", "beneficiaries");
    if (!Directory.Exists(beneficiaryDocsDir))
    {
        Directory.CreateDirectory(beneficiaryDocsDir);
        Console.WriteLine($"Created directory: {beneficiaryDocsDir}");
    }

    // For product images
    var productImagesDir = Path.Combine(app.Environment.WebRootPath, "uploads", "products");
    if (!Directory.Exists(productImagesDir))
    {
        Directory.CreateDirectory(productImagesDir);
        Console.WriteLine($"Created directory: {productImagesDir}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating directories: {ex.Message}");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
