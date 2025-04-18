using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Helpers;
using Sakhaa.Models;

namespace Sakhaa.Controllers
{
    public class UserController : Controller
    {

        private readonly MyDbContext _context;

        public UserController(MyDbContext context)
        {
            _context = context;
        }

        //LogIn
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

                if (user.Email.ToLower() == "admin@gmail.com")
                    HttpContext.Session.SetString("IsAdmin", "true");

                return RedirectToAction("Profile");
            }

            ViewBag.Error = "خطأ في كلمة المرور أو البريد الإلكتروني.";
            return View();
        }

        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //SignUp
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

        //Profile
        public IActionResult Profile()
        {
            //User Info
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();

            //Donation History

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var previousDonations = _context.Donations.Where(d => d.UserId == user.Id && d.DonationEndDate != null)
                .Include(d => d.Program)
                .ToList();

            //Current Donations
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

        //Edit Profile
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

    }
}
