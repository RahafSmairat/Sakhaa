using Microsoft.AspNetCore.Mvc;
using Sakhaa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sakhaa.Controllers
{
    public class DonationController : Controller
    {
        private readonly MyDbContext _context;

        public DonationController(MyDbContext context)
        {
            _context = context;
        }

        
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserEmail") != null;
        }

        private int GetCurrentUserId()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null)
                {
                    return user.Id;
                }
            }

            return 0;
        }

        public IActionResult DonationPrograms()
        {
            
            
            
            var programs = _context.DonationPrograms.ToList();
            return View(programs);
        }

        public async Task<IActionResult> DonationProgramDetails(int id)
        {
            var program = await _context.DonationPrograms
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        public IActionResult InitiateDonation(int programId)
        {
            
            HttpContext.Session.SetInt32("SelectedProgramId", programId);

            
            if (!IsLoggedIn())
            {
                
                HttpContext.Session.SetString("ReturnUrl", "/Donation/DonationApplicationForm");
                return RedirectToAction("Login", "User");
            }

            
            return RedirectToAction("DonationApplicationForm");
        }

        public IActionResult DonationApplicationForm(int? projectId = null, decimal? amount = null)
        {
            
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            
            if (projectId.HasValue && amount.HasValue)
            {
                
                HttpContext.Session.SetInt32("SelectedProjectId", projectId.Value);
                HttpContext.Session.SetString("DonationAmount", amount.Value.ToString());
                
                
                var project = _context.Projects.Find(projectId.Value);
                if (project == null)
                {
                    return RedirectToAction("Index", "Projects");
                }
                
                ViewBag.Project = project;
                ViewBag.Amount = amount.Value;
            }
            else
            {
                
                int? programId = HttpContext.Session.GetInt32("SelectedProgramId");
                if (programId == null)
                {
                    return RedirectToAction("DonationPrograms");
                }

                var program = _context.DonationPrograms.Find(programId);
                if (program == null)
                {
                    return RedirectToAction("DonationPrograms");
                }
                
                ViewBag.Program = program;
            }

            
            int userId = GetCurrentUserId();
            var user = _context.Users.Find(userId);
            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public IActionResult DonationApplicationForm(string firstName, string lastName, string country, string city, string address)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            
            int userId = GetCurrentUserId();
            var user = _context.Users.Find(userId);

            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Country = country;
                _context.SaveChanges();
            }

            
            HttpContext.Session.SetString("DonatorFirstName", firstName);
            HttpContext.Session.SetString("DonatorLastName", lastName);
            HttpContext.Session.SetString("DonatorCountry", country);
            HttpContext.Session.SetString("DonatorCity", city);
            HttpContext.Session.SetString("DonatorAddress", address);
            

            
            return RedirectToAction("PaymentForm");
        }

        public IActionResult PaymentForm()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            // Check if this is a gift donation
            int? giftDonationId = HttpContext.Session.GetInt32("GiftDonationId");
            if (giftDonationId.HasValue)
            {
                var giftDonation = _context.GiftDonations.Find(giftDonationId.Value);
                if (giftDonation != null)
                {
                    ViewBag.GiftDonation = giftDonation;
                    ViewBag.Amount = giftDonation.Amount;
                    ViewBag.IsGiftDonation = true;
                    return View();
                }
            }
            
            // Check if project donation
            int? projectId = HttpContext.Session.GetInt32("SelectedProjectId");
            if (projectId.HasValue)
            {
                var project = _context.Projects.Find(projectId.Value);
                if (project == null)
                {
                    return RedirectToAction("Index", "Projects");
                }

                decimal amount = 0;
                if (decimal.TryParse(HttpContext.Session.GetString("DonationAmount"), out decimal parsedAmount))
                {
                    amount = parsedAmount;
                }
                else
                {
                    return RedirectToAction("Details", "Projects", new { id = projectId.Value });
                }

                
                ViewBag.Project = project;
                ViewBag.Amount = amount;
                ViewBag.IsProjectDonation = true;
                return View();
            }
            
            // Check if program donation
            int? programId = HttpContext.Session.GetInt32("SelectedProgramId");
            if (programId == null)
            {
                return RedirectToAction("DonationPrograms");
            }

            var program = _context.DonationPrograms.Find(programId);
            if (program == null)
            {
                return RedirectToAction("DonationPrograms");
            }

            ViewBag.Program = program;
            ViewBag.IsProjectDonation = false;
            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(string cardNumber, string cardHolder, int expiryMonth, int expiryYear, string cvc, bool isGiftDonation = false)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            int userId = GetCurrentUserId();
            
            // Handle gift donation payment
            if (isGiftDonation)
            {
                int? giftDonationId = HttpContext.Session.GetInt32("GiftDonationId");
                if (!giftDonationId.HasValue)
                {
                    return RedirectToAction("CustomGiftForm");
                }
                
                var giftDonation = _context.GiftDonations.Find(giftDonationId.Value);
                if (giftDonation == null)
                {
                    return RedirectToAction("CustomGiftForm");
                }
                
                Console.WriteLine($"Processing gift payment for donation ID: {giftDonationId}, Amount: {giftDonation.Amount}");
                
                // Update payment status to completed
                var payment = _context.Payments.FirstOrDefault(p => p.GiftDonationId == giftDonationId.Value);
                if (payment != null)
                {
                    payment.Status = "Completed";
                    // Double-check the amount is set correctly
                    payment.Amount = giftDonation.Amount;
                    _context.Update(payment);
                    _context.SaveChanges();
                    
                    Console.WriteLine($"Payment updated: ID {payment.Id}, Amount: {payment.Amount}, Status: {payment.Status}");
                }
                
                // Store amount in session to ensure it's available on success page
                HttpContext.Session.SetString("GiftAmount", giftDonation.Amount.ToString());
                
                // DO NOT remove session variables, they might be needed as fallback
                // HttpContext.Session.Remove("SelectedOccasion");
                // HttpContext.Session.Remove("SelectedColor");
                // HttpContext.Session.Remove("SelectedDecoration");
                
                // Redirect to gift success page
                return RedirectToAction("GiftSuccess", new { id = giftDonationId.Value });
            }
            
            // Handle project donation
            int? projectId = HttpContext.Session.GetInt32("SelectedProjectId");
            decimal donationAmount = 0;
            
            
            if (projectId.HasValue)
            {
                var project = _context.Projects.Find(projectId.Value);
                if (project == null)
                {
                    return RedirectToAction("Index", "Projects");
                }

                
                if (decimal.TryParse(HttpContext.Session.GetString("DonationAmount"), out decimal amount))
                {
                    donationAmount = amount;
                }
                else
                {
                    return RedirectToAction("Details", "Projects", new { id = projectId.Value });
                }

                
                var donation = new Donation
                {
                    UserId = userId,
                    ProjectId = project.Id,
                    Amount = donationAmount,
                    DonationStartDate = DateTime.Now,
                    DonationEndDate = DateTime.Now
                };

                _context.Donations.Add(donation);
                _context.SaveChanges();

                
                if (project.CurrentAmount.HasValue)
                {
                    project.CurrentAmount += donationAmount;
                }
                else
                {
                    project.CurrentAmount = donationAmount;
                }
                _context.Update(project);
                _context.SaveChanges();

                
                var payment = new Payment
                {
                    UserId = userId,
                    DonationId = donation.Id,
                    PaymentMethod = "Credit Card",
                    TransactionId = "PRJ" + DateTime.Now.Ticks.ToString(),
                    Amount = donationAmount,
                    Status = "Completed",
                    PaymentDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                _context.SaveChanges();

                
                HttpContext.Session.Remove("SelectedProjectId");
                HttpContext.Session.Remove("DonationAmount");
            }
            else
            {
                
                int? programId = HttpContext.Session.GetInt32("SelectedProgramId");
                if (programId == null)
                {
                    return RedirectToAction("DonationPrograms");
                }

                var program = _context.DonationPrograms.Find(programId);
                if (program == null)
                {
                    return RedirectToAction("DonationPrograms");
                }

                
                var donation = new Donation();
                
                if (program.IsSubscibtion == true)
                {
                    donation = new Donation
                    {
                        UserId = userId,
                        ProgramId = program.Id,
                        Amount = program.MinimumDonationAmount ?? 0,
                        DonationStartDate = DateTime.Now,
                    };
                }
                else
                {
                    donation = new Donation
                    {
                        UserId = userId,
                        ProgramId = program.Id,
                        Amount = program.MinimumDonationAmount ?? 0,
                        DonationStartDate = DateTime.Now,
                        DonationEndDate = DateTime.Now
                    };
                }

                _context.Donations.Add(donation);
                _context.SaveChanges();

                
                var payment = new Payment
                {
                    UserId = userId,
                    DonationId = donation.Id,
                    PaymentMethod = "Credit Card",
                    TransactionId = "TXN" + DateTime.Now.Ticks.ToString(),
                    Amount = program.MinimumDonationAmount ?? 0,
                    Status = "Completed",
                    PaymentDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                _context.SaveChanges();

                
                HttpContext.Session.Remove("SelectedProgramId");
            }
            
            HttpContext.Session.Remove("DonatorFirstName");
            HttpContext.Session.Remove("DonatorLastName");
            HttpContext.Session.Remove("DonatorCountry");
            HttpContext.Session.Remove("DonatorCity");
            HttpContext.Session.Remove("DonatorAddress");

            
            return RedirectToAction("DonationSuccess");
        }

        public IActionResult DonationSuccess()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        public IActionResult GiftApplicationForm()
        {
            return View();
        }

        public IActionResult CustomGiftForm()
        {
            // Check if user is logged in
            if (!IsLoggedIn())
            {
                // Store return URL for redirecting back after login
                HttpContext.Session.SetString("ReturnUrl", "/Donation/CustomGiftForm");
                return RedirectToAction("Login", "User");
            }

            // Create directory for gift card images if it doesn't exist
            var giftCardsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "giftcards");
            if (!Directory.Exists(giftCardsDir))
            {
                Directory.CreateDirectory(giftCardsDir);
            }

            return View();
        }

        [HttpPost]
        public IActionResult ProcessGiftDonation(string giverName, string giverEmail, string giverPhone,
            string receiverName, string receiverEmail, string receiverPhone,
            decimal amount, bool showAmount, string personalMessage,
            string selectedOccasion = "general", string selectedColor = "#000000", string selectedDecoration = "none", 
            string giftCardImage = null)
        {
            // Check if user is logged in
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }
            
            // Get current user ID
            int userId = GetCurrentUserId();
            
            // Log the amount received
            Console.WriteLine($"Received donation amount: {amount}");
            
            // Ensure the amount is valid (minimum 10)
            if (amount < 10)
            {
                amount = 50; // Default to 50 if invalid amount is provided
                Console.WriteLine("Invalid amount detected, defaulting to 50");
            }
            
            // Create the gift donation record
            var giftDonation = new GiftDonation
            {
                GiverName = giverName,
                GiverEmail = giverEmail,
                GiverPhone = giverPhone,
                ReceiverName = receiverName,
                ReceiverEmail = receiverEmail,
                ReceiverPhone = receiverPhone,
                Amount = amount,
                ShowAmount = showAmount,
                PersonalMessage = personalMessage,
                CreatedAt = DateTime.Now,
                UserId = userId
            };
            
            _context.GiftDonations.Add(giftDonation);
            _context.SaveChanges();
            
            Console.WriteLine($"Gift donation created with ID: {giftDonation.Id}, Amount: {giftDonation.Amount}");
            
            // Save gift card customization
            var giftCardCustomization = new GiftCardCustomization
            {
                GiftDonationId = giftDonation.Id,
                OccasionImageUrl = "/images/gift/" + selectedOccasion + ".jpg",
                DecorationImageUrl = selectedDecoration == "none" ? null : "/images/gift/decoration-" + selectedDecoration + ".png",
                TextColor = selectedColor,
                CreatedAt = DateTime.Now
            };
            
            _context.GiftCardCustomizations.Add(giftCardCustomization);
            
            // Save the gift card image if provided
            if (!string.IsNullOrEmpty(giftCardImage) && giftCardImage.Contains("data:image"))
            {
                try
                {
                    // Extract the base64 data
                    string base64Data = giftCardImage.Substring(giftCardImage.IndexOf(',') + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    
                    // Create a unique filename
                    string fileName = $"giftcard_{giftDonation.Id}_{DateTime.Now.Ticks}.png";
                    string filePath = Path.Combine("images", "giftcards", fileName);
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
                    
                    // Save the image file
                    System.IO.File.WriteAllBytes(fullPath, imageBytes);
                    
                    // Update gift donation with the image URL
                    giftDonation.GiftCardUrl = "/" + filePath.Replace("\\", "/");
                    _context.Update(giftDonation);
                }
                catch (Exception ex)
                {
                    // Log error but continue with the process
                    Console.WriteLine("Error saving gift card image: " + ex.Message);
                }
            }
            
            // Create payment record
            var payment = new Payment
            {
                UserId = userId,
                GiftDonationId = giftDonation.Id,
                PaymentMethod = "Credit Card",
                TransactionId = "GIFT" + DateTime.Now.Ticks.ToString(),
                Amount = giftDonation.Amount,
                Status = "Pending", // Initial status is pending until payment is completed
                PaymentDate = DateTime.Now
            };
            
            _context.Payments.Add(payment);
            _context.SaveChanges();
            
            Console.WriteLine($"Payment record created with Amount: {payment.Amount}");
            
            // Store gift customization details in session
            HttpContext.Session.SetInt32("GiftDonationId", giftDonation.Id);
            HttpContext.Session.SetString("SelectedOccasion", selectedOccasion);
            HttpContext.Session.SetString("SelectedColor", selectedColor);
            HttpContext.Session.SetString("SelectedDecoration", selectedDecoration);
            
            // Explicitly store the amount in session as well
            HttpContext.Session.SetString("GiftAmount", amount.ToString());
            
            // Set ViewBag values for the payment form
            ViewBag.IsGiftDonation = true;
            ViewBag.GiftDonation = giftDonation;
            ViewBag.Amount = giftDonation.Amount;
            
            Console.WriteLine($"Redirecting to payment form with amount: {ViewBag.Amount}");
            
            return RedirectToAction("PaymentForm");
        }

        public IActionResult GiftPayment(int id)
        {
            var giftDonation = _context.GiftDonations.FirstOrDefault(g => g.Id == id);
            if (giftDonation == null)
            {
                return NotFound();
            }

            return View(giftDonation);
        }

        [HttpPost]
        public IActionResult CompleteGiftPayment(int giftDonationId, string cardNumber = null, string cardHolder = null, 
            int? expiryMonth = null, int? expiryYear = null, string cvv = null, string walletType = null, 
            string walletNumber = null)
        {
            var giftDonation = _context.GiftDonations.FirstOrDefault(g => g.Id == giftDonationId);
            if (giftDonation == null)
            {
                return NotFound();
            }

            // Update payment status to completed
            var payment = _context.Payments.FirstOrDefault(p => p.GiftDonationId == giftDonationId);
            if (payment != null)
            {
                payment.Status = "Completed";
                _context.Update(payment);
                _context.SaveChanges();
            }

            // In a real application, payment processing would happen here
            // This is a simplified simulation of payment processing

            // Redirect to success page with the gift card details
            return RedirectToAction("GiftSuccess", new { id = giftDonationId });
        }

        public IActionResult GiftSuccess(int id)
        {
            var giftDonation = _context.GiftDonations
                .Include(g => g.Payments)
                .Include(g => g.GiftCardCustomization)
                .FirstOrDefault(g => g.Id == id);
                
            if (giftDonation == null)
            {
                return NotFound();
            }
            
            Console.WriteLine($"Gift Success page for donation ID: {id}, Amount: {giftDonation.Amount}");

            // Get customization from database
            if (giftDonation.GiftCardCustomization != null)
            {
                var customization = giftDonation.GiftCardCustomization;
                
                // Extract occasion from the URL
                string occasion = "general";
                if (!string.IsNullOrEmpty(customization.OccasionImageUrl))
                {
                    var occasionPath = customization.OccasionImageUrl.Split('/').LastOrDefault();
                    if (occasionPath != null)
                    {
                        occasion = Path.GetFileNameWithoutExtension(occasionPath);
                    }
                }
                
                // Extract decoration from the URL
                string decoration = "none";
                if (!string.IsNullOrEmpty(customization.DecorationImageUrl))
                {
                    var decorationPath = customization.DecorationImageUrl.Split('/').LastOrDefault();
                    if (decorationPath != null)
                    {
                        // Safely extract the decoration name
                        string fileName = Path.GetFileNameWithoutExtension(decorationPath);
                        if (fileName.StartsWith("decoration-"))
                        {
                            decoration = fileName.Substring("decoration-".Length);
                        }
                    }
                }
                
                ViewBag.SelectedOccasion = occasion;
                ViewBag.SelectedColor = customization.TextColor ?? "#000000";
                ViewBag.SelectedDecoration = decoration;
                
                Console.WriteLine($"Decoration set from database: {decoration}, Occasion: {occasion}");
            }
            else
            {
                // Fallback to session or defaults
                ViewBag.SelectedOccasion = HttpContext.Session.GetString("SelectedOccasion") ?? "general";
                ViewBag.SelectedColor = HttpContext.Session.GetString("SelectedColor") ?? "#000000";
                ViewBag.SelectedDecoration = HttpContext.Session.GetString("SelectedDecoration") ?? "none";
                
                Console.WriteLine($"Using fallback decoration: {ViewBag.SelectedDecoration}");
            }
            
            // Explicitly set the amount in ViewBag as well
            ViewBag.Amount = giftDonation.Amount;
            
            // Get the associated payment for verification
            var payment = giftDonation.Payments.FirstOrDefault();
            if (payment != null)
            {
                Console.WriteLine($"Associated payment: ID {payment.Id}, Amount: {payment.Amount}, Status: {payment.Status}");
            }

            return View(giftDonation);
        }
    }
}