using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;

namespace Sakhaa.Controllers
{
    public class Admin2Controller : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public Admin2Controller(MyDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private bool IsAdmin2User()
        {
            return HttpContext.Session.GetString("IsAdmin2") == "true";
        }

        private IActionResult RedirectToLoginIfNeeded()
        {
            if (!IsAdmin2User())
            {
                return RedirectToAction("Login", "User");
            }
            return null;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product,
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TopProducts = topProducts.Select(tp => new
            {
                tp.Product.Id,
                ProductName = tp.Product.ProductName,
                tp.Product.Price,
                ProductImage = tp.Product.ProductImage,
                tp.OrderCount
            }).ToList();

            return View();
        }

        public async Task<IActionResult> Products()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sponsor)
                .ToListAsync();

            return View(products);
        }

        public IActionResult CreateProduct()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Sponsors = new SelectList(_context.Sponsors, "SponsorId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile Image)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            Console.WriteLine($"Received IsAvailable value: {product.IsAvailable}, Type: {(product.IsAvailable == null ? "null" : product.IsAvailable.GetType().FullName)}");
            Console.WriteLine($"Form values: {string.Join(", ", Request.Form.Select(f => $"{f.Key}={f.Value}"))}");

            if (Request.Form.TryGetValue("IsAvailable", out var isAvailableValue))
            {
                string strValue = isAvailableValue.ToString();
                if (bool.TryParse(strValue, out bool boolValue))
                {
                    product.IsAvailable = boolValue;
                    Console.WriteLine($"Parsed IsAvailable from form: {boolValue}");
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                
                if (ModelState.TryGetValue("IsAvailable", out var modelStateEntry))
                {
                    Console.WriteLine($"IsAvailable ModelState: {modelStateEntry.AttemptedValue}, Errors: {modelStateEntry.Errors.Count}");
                    foreach (var error in modelStateEntry.Errors)
                    {
                        Console.WriteLine($"IsAvailable Error: {error.ErrorMessage}");
                    }
                }
                else
                {
                    Console.WriteLine("IsAvailable not found in ModelState");
                }
            }

            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    product.ProductImage = "/uploads/products/" + uniqueFileName;
                }

                product.CreatedDate = DateTime.Now;
                
                product.IsAvailable = product.IsAvailable ?? false;
                Console.WriteLine($"Final IsAvailable value: {product.IsAvailable}");
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة المنتج بنجاح";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Sponsors = new SelectList(_context.Sponsors, "SponsorId", "Name", product.SponsorId);
            return View(product);
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Error"] = "المنتج غير موجود";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Sponsors = new SelectList(_context.Sponsors, "SponsorId", "Name", product.SponsorId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile Image)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            Console.WriteLine($"Received IsAvailable value: {product.IsAvailable}, Type: {(product.IsAvailable == null ? "null" : product.IsAvailable.GetType().FullName)}");
            Console.WriteLine($"Form values: {string.Join(", ", Request.Form.Select(f => $"{f.Key}={f.Value}"))}");

            if (Request.Form.TryGetValue("IsAvailable", out var isAvailableValue))
            {
                string strValue = isAvailableValue.ToString();
                if (bool.TryParse(strValue, out bool boolValue))
                {
                    product.IsAvailable = boolValue;
                    Console.WriteLine($"Parsed IsAvailable from form: {boolValue}");
                }
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                
                if (ModelState.TryGetValue("IsAvailable", out var modelStateEntry))
                {
                    Console.WriteLine($"IsAvailable ModelState: {modelStateEntry.AttemptedValue}, Errors: {modelStateEntry.Errors.Count}");
                    foreach (var error in modelStateEntry.Errors)
                    {
                        Console.WriteLine($"IsAvailable Error: {error.ErrorMessage}");
                    }
                }
                else
                {
                    Console.WriteLine("IsAvailable not found in ModelState");
                }
            }

            if (id != product.Id)
            {
                TempData["Error"] = "حدث خطأ أثناء تحديث المنتج";
                return RedirectToAction(nameof(Products));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

                    if (Image != null && Image.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(existingProduct.ProductImage))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ProductImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        product.ProductImage = "/uploads/products/" + uniqueFileName;
                    }
                    else
                    {
                        product.ProductImage = existingProduct.ProductImage;
                    }

                    product.IsAvailable = product.IsAvailable ?? false;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث المنتج بنجاح";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"DbUpdateConcurrencyException: {ex.Message}");
                    
                    if (!ProductExists(product.Id))
                    {
                        TempData["Error"] = "المنتج غير موجود";
                        return RedirectToAction(nameof(Products));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in EditProduct: {ex.Message}");
                    throw;
                }
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Sponsors = new SelectList(_context.Sponsors, "SponsorId", "Name", product.SponsorId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleProductAvailability([FromForm] int id)
        {            
            try
            {

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "المنتج غير موجود" });
                }

                product.IsAvailable = !(product.IsAvailable ?? false);
                
                await _context.SaveChangesAsync();

                var statusMessage = product.IsAvailable == true ? "تم تفعيل المنتج بنجاح" : "تم إلغاء تفعيل المنتج بنجاح";
                return Json(new { success = true, message = statusMessage, isAvailable = product.IsAvailable });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث حالة المنتج" });
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Categories()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة التصنيف بنجاح";
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["Error"] = "التصنيف غير موجود";
                return RedirectToAction(nameof(Categories));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            if (id != category.Id)
            {
                TempData["Error"] = "حدث خطأ أثناء تحديث التصنيف";
                return RedirectToAction(nameof(Categories));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث التصنيف بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        TempData["Error"] = "التصنيف غير موجود";
                        return RedirectToAction(nameof(Categories));
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> CheckCategoryProducts(int id)
        {

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "التصنيف غير موجود" });
            }

            var productsCount = category.Products?.Count ?? 0;
            return Json(new { success = true, hasProducts = productsCount > 0, count = productsCount });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, bool deactivateProducts = false)
        {

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Json(new { success = false, message = "التصنيف غير موجود" });
            }

            if (deactivateProducts && category.Products != null && category.Products.Count > 0)
            {
                foreach (var product in category.Products)
                {
                    product.IsAvailable = false;
                }
                await _context.SaveChangesAsync();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف التصنيف بنجاح" });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Orders()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "الطلب غير موجود";
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "الطلب غير موجود" });
            }

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                return Json(new { success = false, message = "حالة الطلب غير صالحة" });
            }

            order.OrderStatus = status;
            await _context.SaveChangesAsync();

            string statusMessage = "";
            switch (status)
            {
                case "Pending":
                    statusMessage = "قيد الانتظار";
                    break;
                case "Processing":
                    statusMessage = "قيد المعالجة";
                    break;
                case "Shipped":
                    statusMessage = "تم الشحن";
                    break;
                case "Delivered":
                    statusMessage = "تم التسليم";
                    break;
                case "Cancelled":
                    statusMessage = "ملغي";
                    break;
            }

            return Json(new { success = true, message = $"تم تحديث حالة الطلب إلى {statusMessage}" });
        }
        public async Task<IActionResult> Sponsors()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var sponsors = await _context.Sponsors.ToListAsync();
            return View(sponsors);
        }

        public IActionResult CreateSponsor()
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSponsor(Sponsor sponsor, IFormFile Logo)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            if (ModelState.IsValid)
            {
                if (Logo != null && Logo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "sponsors");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Logo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Logo.CopyToAsync(fileStream);
                    }

                    sponsor.LogoPath = "/uploads/sponsors/" + uniqueFileName;
                }

                _context.Sponsors.Add(sponsor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة الراعي بنجاح";
                return RedirectToAction(nameof(Sponsors));
            }
            return View(sponsor);
        }

        public async Task<IActionResult> EditSponsor(int id)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
            {
                TempData["Error"] = "الراعي غير موجود";
                return RedirectToAction(nameof(Sponsors));
            }

            return View(sponsor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSponsor(int id, Sponsor sponsor, IFormFile Logo)
        {
            var redirectResult = RedirectToLoginIfNeeded();
            if (redirectResult != null)
                return redirectResult;

            if (id != sponsor.SponsorId)
            {
                TempData["Error"] = "حدث خطأ أثناء تحديث الراعي";
                return RedirectToAction(nameof(Sponsors));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSponsor = await _context.Sponsors.AsNoTracking().FirstOrDefaultAsync(s => s.SponsorId == id);

                    if (Logo != null && Logo.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "sponsors");
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Logo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Logo.CopyToAsync(fileStream);
                        }

                        if (!string.IsNullOrEmpty(existingSponsor.LogoPath))
                        {
                            var oldLogoPath = Path.Combine(_hostEnvironment.WebRootPath, existingSponsor.LogoPath.TrimStart('/'));
                            if (System.IO.File.Exists(oldLogoPath))
                            {
                                System.IO.File.Delete(oldLogoPath);
                            }
                        }

                        sponsor.LogoPath = "/uploads/sponsors/" + uniqueFileName;
                    }
                    else
                    {
                        sponsor.LogoPath = existingSponsor.LogoPath;
                    }

                    _context.Update(sponsor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث الراعي بنجاح";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SponsorExists(sponsor.SponsorId))
                    {
                        TempData["Error"] = "الراعي غير موجود";
                        return RedirectToAction(nameof(Sponsors));
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Sponsors));
            }
            return View(sponsor);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSponsor(int id)
        {
            if (!IsAdmin2User())
            {
                return Json(new { success = false, message = "غير مصرح لك بهذه العملية" });
            }

            var sponsor = await _context.Sponsors
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SponsorId == id);

            if (sponsor == null)
            {
                return Json(new { success = false, message = "الراعي غير موجود" });
            }

            if (sponsor.Products.Count > 0)
            {
                return Json(new { success = false, message = "لا يمكن حذف هذا الراعي لأنه مرتبط بمنتجات", productsCount = sponsor.Products.Count });
            }

            if (!string.IsNullOrEmpty(sponsor.LogoPath))
            {
                var logoPath = Path.Combine(_hostEnvironment.WebRootPath, sponsor.LogoPath.TrimStart('/'));
                if (System.IO.File.Exists(logoPath))
                {
                    System.IO.File.Delete(logoPath);
                }
            }

            _context.Sponsors.Remove(sponsor);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف الراعي بنجاح" });
        }

        private bool SponsorExists(int id)
        {
            return _context.Sponsors.Any(e => e.SponsorId == id);
        }
    }
} 