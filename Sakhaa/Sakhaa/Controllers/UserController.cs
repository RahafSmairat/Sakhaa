using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Helpers;
using Sakhaa.Models;
using System;
using System.Linq;

namespace Sakhaa.Controllers
{
    public class UserController : Controller
    {

        private readonly MyDbContext _context;

        public UserController(MyDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (user != null && PasswordHelper.VerifyPassword(Password, user.Password))
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.FirstName);
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                if (user.Email.ToLower() == "admin@gmail.com")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (user.Email.ToLower() == "admin2@gmail.com")
                {
                    HttpContext.Session.SetString("IsAdmin2", "true");
                    return RedirectToAction("Dashboard", "Admin2");
                }

                
                var cartController = new CartController(_context);
                cartController.ControllerContext = ControllerContext;
                cartController.MigrateCart(user.Id);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "خطأ في كلمة المرور أو البريد الإلكتروني.";
            return View();
        }

        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(string FirstName, string LastName, string Email, string PhoneNumber, string Country, string Password, string ConfirmPassword)
        {
            if (String.IsNullOrEmpty(FirstName) || String.IsNullOrEmpty(LastName) || String.IsNullOrEmpty(PhoneNumber) ||
                String.IsNullOrEmpty(Country) || String.IsNullOrEmpty(Password) || String.IsNullOrEmpty(ConfirmPassword))
            {
                ViewBag.Error = "الرجاء تعبئة جميع الحقول";
                return View();
            }
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "كلمة السر غير متطابقة.";
                return View();
            }

            if (_context.Users.Any(u => u.Email == Email))
            {
                ViewBag.Error = "البريد الإلكتروني مستخدم بالفعل.";
                return View();
            }

            var hashedPassword = PasswordHelper.HashPassword(Password);

            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Country = Country,
                Password = hashedPassword,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        
        public IActionResult Profile()
        {
            
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

            var user = _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.PaymentMethods)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Address)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.PaymentMethod)
                .FirstOrDefault(u => u.Email == userEmail);
                
            if (user == null) return NotFound();

            
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var previousDonations = _context.Donations.Where(d => d.UserId == user.Id && d.DonationEndDate != null)
                .Include(d => d.Program).Include(p => p.Project)
                .ToList();

            
            var currentDonations = _context.Donations.Where(d => d.UserId == user.Id && d.DonationEndDate == null)
                .Include(d => d.Program)
                .ToList();

            var vm = new ProfileViewModel
            {
                userInfo = user,
                PreviousDonations = previousDonations,
                CurrentDonations = currentDonations
            };
            return View(vm);
        }

        
        [HttpPost]
        public IActionResult UpdateProfile(string FullName, string Email, string PhoneNumber, string CurrentPassword, string Password, string ConfirmPassword)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(FullName))
            {
                var nameParts = FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    user.FirstName = nameParts[0];
                    user.LastName = nameParts[1];
                }
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber))
            {
                user.PhoneNumber = PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    TempData["Error"] = "يرجى إدخال كلمة السر الحالية أولاً.";
                    return RedirectToAction("Profile");
                }

                bool passwordMatches = PasswordHelper.VerifyPassword(CurrentPassword, user.Password);
                if (!passwordMatches)
                {
                    TempData["Error"] = "كلمة السر الحالية غير صحيحة.";
                    return RedirectToAction("Profile");
                }

                if (Password != ConfirmPassword)
                {
                    TempData["Error"] = "كلمة السر الجديدة غير متطابقة.";
                    return RedirectToAction("Profile");
                }

                user.Password = PasswordHelper.HashPassword(Password);
            }

            _context.SaveChanges();

            TempData["Success"] = "تم حفظ التعديلات بنجاح";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult CancelDonation(int id)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return Json(new { success = false, message = "غير مسجل دخول" });

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            var donation = _context.Donations.FirstOrDefault(d => d.Id == id && d.UserId == user.Id);
            if (donation == null)
                return Json(new { success = false, message = "التبرع غير موجود" });

            
            donation.DonationEndDate = DateTime.Now;

            try
            {
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء إلغاء الاشتراك: " + ex.Message });
            }
        }

        
        [HttpPost]
        public IActionResult AddAddress(string Title, string Street, string City, string Country, string PhoneNumber, bool IsDefault = false)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "لم يتم العثور على المستخدم" });

                var address = new Address
                {
                    UserId = user.Id,
                    Title = Title,
                    Street = Street,
                    City = City,
                    Country = Country,
                    PhoneNumber = PhoneNumber,
                    IsDefault = IsDefault,
                    CreatedAt = DateTime.Now
                };

                
                if (IsDefault)
                {
                    var userAddresses = _context.Addresses.Where(a => a.UserId == user.Id);
                    foreach (var a in userAddresses)
                    {
                        a.IsDefault = false;
                    }
                }

                _context.Addresses.Add(address);
                _context.SaveChanges();

                TempData["Success"] = "تمت إضافة العنوان بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء إضافة العنوان: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult UpdateAddress(int AddressId, string Title, string Street, string City, string Country, string PhoneNumber)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "لم يتم العثور على المستخدم" });

                var address = _context.Addresses.FirstOrDefault(a => a.AddressId == AddressId && a.UserId == user.Id);
                if (address == null)
                    return Json(new { success = false, message = "لم يتم العثور على العنوان" });

                address.Title = Title;
                address.Street = Street;
                address.City = City;
                address.Country = Country;
                address.PhoneNumber = PhoneNumber;

                _context.SaveChanges();

                TempData["Success"] = "تم تحديث العنوان بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحديث العنوان: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult DeleteAddress(int addressId)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login");

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return NotFound();

                var address = _context.Addresses.FirstOrDefault(a => a.AddressId == addressId && a.UserId == user.Id);
                if (address == null)
                    return NotFound();

                _context.Addresses.Remove(address);
                _context.SaveChanges();

                TempData["Success"] = "تم حذف العنوان بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء حذف العنوان: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult SetDefaultAddress(int addressId)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login");

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return NotFound();

                
                var userAddresses = _context.Addresses.Where(a => a.UserId == user.Id);
                foreach (var address in userAddresses)
                {
                    address.IsDefault = false;
                }

                
                var selectedAddress = userAddresses.FirstOrDefault(a => a.AddressId == addressId);
                if (selectedAddress == null)
                    return NotFound();

                selectedAddress.IsDefault = true;
                _context.SaveChanges();

                TempData["Success"] = "تم تعيين العنوان الافتراضي بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تعيين العنوان الافتراضي: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        
        [HttpPost]
        public IActionResult AddPaymentMethod(string CardHolderName, string CardNumber, string ExpiryDate, string Cvv, bool IsDefault = false)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "لم يتم العثور على المستخدم" });

                var paymentMethod = new PaymentMethod
                {
                    UserId = user.Id,
                    CardHolderName = CardHolderName,
                    CardNumber = CardNumber,
                    ExpiryDate = ExpiryDate,
                    Cvv = Cvv,
                    IsDefault = IsDefault,
                    CreatedAt = DateTime.Now
                };

                
                if (IsDefault)
                {
                    var userPaymentMethods = _context.PaymentMethods.Where(p => p.UserId == user.Id);
                    foreach (var p in userPaymentMethods)
                    {
                        p.IsDefault = false;
                    }
                }

                _context.PaymentMethods.Add(paymentMethod);
                _context.SaveChanges();

                TempData["Success"] = "تمت إضافة وسيلة الدفع بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء إضافة وسيلة الدفع: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult UpdatePaymentMethod(int PaymentMethodId, string CardHolderName, string CardNumber, string ExpiryDate)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return Json(new { success = false, message = "لم يتم العثور على المستخدم" });

                var paymentMethod = _context.PaymentMethods.FirstOrDefault(p => p.Id == PaymentMethodId && p.UserId == user.Id);
                if (paymentMethod == null)
                    return Json(new { success = false, message = "لم يتم العثور على وسيلة الدفع" });

                paymentMethod.CardHolderName = CardHolderName;
                paymentMethod.CardNumber = CardNumber;
                paymentMethod.ExpiryDate = ExpiryDate;

                _context.SaveChanges();

                TempData["Success"] = "تم تحديث وسيلة الدفع بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحديث وسيلة الدفع: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult DeletePaymentMethod(int paymentMethodId)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login");

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return NotFound();

                var paymentMethod = _context.PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId && p.UserId == user.Id);
                if (paymentMethod == null)
                    return NotFound();

                _context.PaymentMethods.Remove(paymentMethod);
                _context.SaveChanges();

                TempData["Success"] = "تم حذف وسيلة الدفع بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء حذف وسيلة الدفع: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        public IActionResult SetDefaultPaymentMethod(int paymentMethodId)
        {
            try
            {
                string userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                    return RedirectToAction("Login");

                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return NotFound();

                
                var userPaymentMethods = _context.PaymentMethods.Where(p => p.UserId == user.Id);
                foreach (var paymentMethod in userPaymentMethods)
                {
                    paymentMethod.IsDefault = false;
                }

                
                var selectedPaymentMethod = userPaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
                if (selectedPaymentMethod == null)
                    return NotFound();

                selectedPaymentMethod.IsDefault = true;
                _context.SaveChanges();

                TempData["Success"] = "تم تعيين وسيلة الدفع الافتراضية بنجاح";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تعيين وسيلة الدفع الافتراضية: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }
        
        
        public IActionResult GetOrderDetails(int orderId)
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound();

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.PaymentMethod)
                .FirstOrDefault(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
                return NotFound();

            return PartialView("_OrderDetailsPartial", order);
        }
    }
}
