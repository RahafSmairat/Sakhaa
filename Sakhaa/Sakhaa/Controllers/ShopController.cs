using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Sakhaa.Controllers
{
    public class ShopController : Controller
    {
        private readonly MyDbContext _context;

        public ShopController(MyDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index(int? categoryId = null, int? sponsorId = null, string searchTerm = null)
        {
            
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sponsor)
                .Where(p => p.IsAvailable == true);

            
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            
            if (sponsorId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.SponsorId == sponsorId.Value);
            }

            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                productsQuery = productsQuery.Where(p => 
                    p.ProductName.ToLower().Contains(searchTerm) || 
                    p.ProductDescription.ToLower().Contains(searchTerm) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(searchTerm))
                );
            }

            
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Sponsors = await _context.Sponsors.ToListAsync();
            
            
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSponsorId = sponsorId;
            ViewBag.SearchTerm = searchTerm;

            
            var products = await productsQuery.OrderByDescending(p => p.SponsorId.HasValue).ThenBy(p => p.ProductName).ToListAsync();
            
            return View(products);
        }

        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sponsor)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable == true);

            if (product == null)
            {
                return NotFound();
            }

            
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsAvailable == true)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
        
        
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Sponsor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsAvailable == true);

                if (product == null)
                {
                    return NotFound(new { error = "Product not found" });
                }

                
                var relatedProducts = await _context.Products
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsAvailable == true)
                    .AsNoTracking()
                    .Take(4)
                    .ToListAsync();

                
                var productData = new
                {
                    id = product.Id,
                    productName = product.ProductName,
                    productDescription = product.ProductDescription,
                    productImage = product.ProductImage,
                    price = product.Price,
                    category = product.Category != null ? new { id = product.Category.Id, name = product.Category.Name } : null,
                    sponsor = product.Sponsor != null ? new
                    {
                        id = product.Sponsor.SponsorId,
                        name = product.Sponsor.Name,
                        logoPath = product.Sponsor.LogoPath,
                        website = product.Sponsor.Website
                    } : null
                };

                var relatedProductsData = relatedProducts.Select(p => new
                {
                    id = p.Id,
                    productName = p.ProductName,
                    productImage = p.ProductImage,
                    price = p.Price
                }).ToList();

                return Json(new
                {
                    product = productData,
                    relatedProducts = relatedProductsData
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while fetching product details", details = ex.Message });
            }
        }
    }
} 