using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sakhaa.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sakhaa.Controllers
{
    public class CartController : Controller
    {
        private readonly MyDbContext _context;
        private const string CartCookieName = "SakhaaCart";

        public CartController(MyDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Index()
        {
            var cartViewModel = new CartViewModel
            {
                CartItems = new List<CartItemViewModel>(),
                TotalAmount = 0
            };

            
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .Where(c => c.User.Id == user.Id)
                        .FirstOrDefault();

                    if (cart != null && cart.CartItems.Any())
                    {
                        foreach (var item in cart.CartItems)
                        {
                            if (item.Product != null)
                            {
                                cartViewModel.CartItems.Add(new CartItemViewModel
                                {
                                    Id = item.Id,
                                    ProductId = item.ProductId.Value,
                                    ProductName = item.Product.ProductName,
                                    ProductImage = item.Product.ProductImage,
                                    Price = item.Product.Price.Value,
                                    Quantity = item.Quantity.Value
                                });

                                cartViewModel.TotalAmount += item.Product.Price.Value * item.Quantity.Value;
                            }
                        }
                    }
                }
            }
            else
            {
                
                var cartCookie = Request.Cookies[CartCookieName];
                if (!string.IsNullOrEmpty(cartCookie))
                {
                    try
                    {
                        var cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                        if (cookieCartItems != null && cookieCartItems.Any())
                        {
                            
                            var productIds = cookieCartItems.Select(i => i.ProductId).ToList();
                            var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();

                            foreach (var cookieItem in cookieCartItems)
                            {
                                var product = products.FirstOrDefault(p => p.Id == cookieItem.ProductId);
                                if (product != null)
                                {
                                    cartViewModel.CartItems.Add(new CartItemViewModel
                                    {
                                        ProductId = product.Id,
                                        ProductName = product.ProductName,
                                        ProductImage = product.ProductImage,
                                        Price = product.Price.Value,
                                        Quantity = cookieItem.Quantity
                                    });

                                    cartViewModel.TotalAmount += product.Price.Value * cookieItem.Quantity;
                                }
                            }
                        }
                    }
                    catch
                    {
                        
                        Response.Cookies.Delete(CartCookieName);
                    }
                }
            }

            return View(cartViewModel);
        }

        
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            
            var product = _context.Products.FirstOrDefault(p => p.Id == productId && p.IsAvailable == true);
            if (product == null)
            {
                return Json(new { success = false, message = "المنتج غير متوفر" });
            }

            string userEmail = HttpContext.Session.GetString("UserEmail");
            
            if (!string.IsNullOrEmpty(userEmail))
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .Where(c => c.User.Id == user.Id)
                        .FirstOrDefault();

                    
                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            User = user, 
                            CreatedDate = DateTime.Now
                        };
                        _context.Carts.Add(cart);
                        _context.SaveChanges();
                    }

                    
                    var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                    if (existingItem != null)
                    {
                        
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        
                        cart.CartItems.Add(new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = productId,
                            Quantity = quantity
                        });
                    }

                    _context.SaveChanges();
                    return Json(new { success = true, message = "تمت إضافة المنتج إلى السلة", cartCount = cart.CartItems.Sum(c => c.Quantity) });
                }
            }
            else
            {
                
                var cartCookie = Request.Cookies[CartCookieName];
                List<CookieCartItem> cookieCartItems = new List<CookieCartItem>();

                if (!string.IsNullOrEmpty(cartCookie))
                {
                    try
                    {
                        cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                    }
                    catch
                    {
                        
                        cookieCartItems = new List<CookieCartItem>();
                    }
                }

                if (cookieCartItems == null)
                {
                    cookieCartItems = new List<CookieCartItem>();
                }

                
                var existingItem = cookieCartItems.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    
                    existingItem.Quantity += quantity;
                }
                else
                {
                    
                    cookieCartItems.Add(new CookieCartItem
                    {
                        ProductId = productId,
                        Quantity = quantity
                    });
                }

                
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append(CartCookieName, JsonConvert.SerializeObject(cookieCartItems), cookieOptions);
                return Json(new { success = true, message = "تمت إضافة المنتج إلى السلة", cartCount = cookieCartItems.Sum(c => c.Quantity) });
            }

            return Json(new { success = false, message = "حدث خطأ أثناء إضافة المنتج إلى السلة" });
        }

        
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveFromCart(productId);
            }

            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .Where(c => c.User.Id == user.Id)
                        .FirstOrDefault();

                    if (cart != null)
                    {
                        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                        if (cartItem != null)
                        {
                            cartItem.Quantity = quantity;
                            _context.SaveChanges();

                            
                            decimal itemTotal = cartItem.Product.Price.Value * quantity;
                            decimal cartTotal = cart.CartItems.Sum(ci => ci.Product.Price.Value * ci.Quantity.Value);

                            return Json(new { 
                                success = true, 
                                message = "تم تحديث الكمية", 
                                itemTotal, 
                                cartTotal,
                                cartCount = cart.CartItems.Sum(c => c.Quantity)
                            });
                        }
                    }
                }
            }
            else
            {
                
                var cartCookie = Request.Cookies[CartCookieName];
                if (!string.IsNullOrEmpty(cartCookie))
                {
                    try
                    {
                        var cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                        if (cookieCartItems != null)
                        {
                            var cookieItem = cookieCartItems.FirstOrDefault(i => i.ProductId == productId);
                            if (cookieItem != null)
                            {
                                cookieItem.Quantity = quantity;

                                
                                var cookieOptions = new CookieOptions
                                {
                                    Expires = DateTime.Now.AddDays(7),
                                    IsEssential = true,
                                    SameSite = SameSiteMode.Lax
                                };

                                Response.Cookies.Append(CartCookieName, JsonConvert.SerializeObject(cookieCartItems), cookieOptions);

                                
                                var product = _context.Products.FirstOrDefault(p => p.Id == productId);
                                if (product != null)
                                {
                                    decimal itemTotal = product.Price.Value * quantity;
                                    
                                    
                                    decimal cartTotal = 0;
                                    var productIds = cookieCartItems.Select(i => i.ProductId).ToList();
                                    var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();
                                    
                                    foreach (var item in cookieCartItems)
                                    {
                                        var p = products.FirstOrDefault(p => p.Id == item.ProductId);
                                        if (p != null)
                                        {
                                            cartTotal += p.Price.Value * item.Quantity;
                                        }
                                    }

                                    return Json(new { 
                                        success = true, 
                                        message = "تم تحديث الكمية", 
                                        itemTotal, 
                                        cartTotal,
                                        cartCount = cookieCartItems.Sum(c => c.Quantity)
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {
                        
                    }
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء تحديث الكمية" });
        }

        
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrEmpty(userEmail))
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .Where(c => c.User.Id == user.Id)
                        .FirstOrDefault();

                    if (cart != null)
                    {
                        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                        if (cartItem != null)
                        {
                            _context.CartItems.Remove(cartItem);
                            _context.SaveChanges();
                            return Json(new { 
                                success = true, 
                                message = "تم إزالة المنتج من السلة",
                                cartCount = cart.CartItems.Sum(c => c.Quantity)
                            });
                        }
                    }
                }
            }
            else
            {
                
                var cartCookie = Request.Cookies[CartCookieName];
                if (!string.IsNullOrEmpty(cartCookie))
                {
                    try
                    {
                        var cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                        if (cookieCartItems != null)
                        {
                            var cookieItem = cookieCartItems.FirstOrDefault(i => i.ProductId == productId);
                            if (cookieItem != null)
                            {
                                cookieCartItems.Remove(cookieItem);

                                
                                if (cookieCartItems.Any())
                                {
                                    var cookieOptions = new CookieOptions
                                    {
                                        Expires = DateTime.Now.AddDays(7),
                                        IsEssential = true,
                                        SameSite = SameSiteMode.Lax
                                    };

                                    Response.Cookies.Append(CartCookieName, JsonConvert.SerializeObject(cookieCartItems), cookieOptions);
                                }
                                else
                                {
                                    Response.Cookies.Delete(CartCookieName);
                                }

                                return Json(new { 
                                    success = true, 
                                    message = "تم إزالة المنتج من السلة",
                                    cartCount = cookieCartItems.Sum(c => c.Quantity)
                                });
                            }
                        }
                    }
                    catch
                    {
                        
                        Response.Cookies.Delete(CartCookieName);
                    }
                }
            }

            return Json(new { success = false, message = "حدث خطأ أثناء إزالة المنتج من السلة" });
        }

        
        [HttpGet]
        public IActionResult CartCount()
        {
            int count = 0;
            string userEmail = HttpContext.Session.GetString("UserEmail");
            
            if (!string.IsNullOrEmpty(userEmail))
            {
                
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .Where(c => c.User.Id == user.Id)
                        .FirstOrDefault();

                    if (cart != null)
                    {
                        count = cart.CartItems.Sum(ci => ci.Quantity ?? 0);
                    }
                }
            }
            else
            {
                
                var cartCookie = Request.Cookies[CartCookieName];
                if (!string.IsNullOrEmpty(cartCookie))
                {
                    try
                    {
                        var cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                        if (cookieCartItems != null)
                        {
                            count = cookieCartItems.Sum(i => i.Quantity);
                        }
                    }
                    catch
                    {
                        
                        Response.Cookies.Delete(CartCookieName);
                    }
                }
            }

            return Json(new { cartCount = count });
        }

        
        public void MigrateCart(int userId)
        {
            var cartCookie = Request.Cookies[CartCookieName];
            if (string.IsNullOrEmpty(cartCookie))
            {
                return;
            }

            try
            {
                var cookieCartItems = JsonConvert.DeserializeObject<List<CookieCartItem>>(cartCookie);
                if (cookieCartItems == null || !cookieCartItems.Any())
                {
                    return;
                }

                
                var user = _context.Users.Find(userId);
                if (user == null) return;
                
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .Where(c => c.User.Id == userId)
                    .FirstOrDefault();

                if (cart == null)
                {
                    cart = new Cart
                    {
                        User = user, 
                        CreatedDate = DateTime.Now
                    };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }

                
                foreach (var cookieItem in cookieCartItems)
                {
                    var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cookieItem.ProductId);
                    if (existingItem != null)
                    {
                        
                        existingItem.Quantity += cookieItem.Quantity;
                    }
                    else
                    {
                        
                        cart.CartItems.Add(new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = cookieItem.ProductId,
                            Quantity = cookieItem.Quantity
                        });
                    }
                }

                _context.SaveChanges();

                
                Response.Cookies.Delete(CartCookieName);
            }
            catch
            {
                
                Response.Cookies.Delete(CartCookieName);
            }
        }

        
        public IActionResult Checkout()
        {
            
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User", new { returnUrl = "/Cart/Checkout" });
            }

            
            var user = _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.PaymentMethods)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

            
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.User.Id == user.Id)
                .FirstOrDefault();

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                UserAddresses = user.Addresses.ToList(),
                PaymentMethods = user.PaymentMethods.ToList(),
                CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId.Value,
                    ProductName = ci.Product.ProductName,
                    ProductImage = ci.Product.ProductImage,
                    Price = ci.Product.Price.Value,
                    Quantity = ci.Quantity.Value
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price.Value * ci.Quantity.Value)
            };

            return View(checkoutViewModel);
        }

        
        [HttpPost]
        public IActionResult Checkout(int addressId, int paymentMethodId)
        {
            
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            
            var address = _context.Addresses.FirstOrDefault(a => a.AddressId == addressId && a.UserId == user.Id);
            var paymentMethod = _context.PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId && p.UserId == user.Id);

            if (address == null || paymentMethod == null)
            {
                return BadRequest("العنوان أو طريقة الدفع غير صالحة");
            }

            
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.User.Id == user.Id)
                .FirstOrDefault();

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            
            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price.Value * ci.Quantity.Value),
                OrderDate = DateTime.Now,
                OrderStatus = "تم الطلب",
                AddressId = addressId,
                PaymentMethodId = paymentMethodId
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.SaveChanges();

            
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        
        public IActionResult OrderConfirmation(int orderId)
        {
            
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.PaymentMethod)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        
        public class CheckoutViewModel
        {
            public List<Address> UserAddresses { get; set; }
            public List<PaymentMethod> PaymentMethods { get; set; }
            public List<CartItemViewModel> CartItems { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }

    
    public class CartViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    
    public class CookieCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 