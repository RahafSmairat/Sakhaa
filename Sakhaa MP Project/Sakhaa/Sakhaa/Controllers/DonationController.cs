using Microsoft.AspNetCore.Mvc;
using Sakhaa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Sakhaa.Controllers
{
    public class DonationController : Controller
    {
        private readonly MyDbContext _context;

        public DonationController(MyDbContext context)
        {
            _context = context;
        }

        // Helper method to check if user is logged in
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
            //var programs = _context.DonationPrograms.ToList();
            //ViewBag.Programs = programs;
            //return View();
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
            // Store program ID in session for the next steps
            HttpContext.Session.SetInt32("SelectedProgramId", programId);

            // Check if user is logged in
            if (!IsLoggedIn())
            {
                // Store return URL in session
                HttpContext.Session.SetString("ReturnUrl", "/Donation/DonationApplicationForm");
                return RedirectToAction("Login", "User");
            }

            // User is logged in, proceed to application form
            return RedirectToAction("DonationApplicationForm");
        }

        public IActionResult DonationApplicationForm(int? projectId = null, decimal? amount = null)
        {
            // Check if user is logged in
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            // Check if coming from program donation or project donation
            if (projectId.HasValue && amount.HasValue)
            {
                // Store project ID and amount in session for the next steps
                HttpContext.Session.SetInt32("SelectedProjectId", projectId.Value);
                HttpContext.Session.SetString("DonationAmount", amount.Value.ToString());
                
                // Get project info
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
                // Continue with program donation flow
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

            // Get user info
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

            // Update user information if changed
            int userId = GetCurrentUserId();
            var user = _context.Users.Find(userId);

            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Country = country;
                _context.SaveChanges();
            }

            // Store application info in session
            HttpContext.Session.SetString("DonatorFirstName", firstName);
            HttpContext.Session.SetString("DonatorLastName", lastName);
            HttpContext.Session.SetString("DonatorCountry", country);
            HttpContext.Session.SetString("DonatorCity", city);
            HttpContext.Session.SetString("DonatorAddress", address);
            //HttpContext.Session.SetString("IsDonatingForBenefactor", isDonatingForBenefactor.ToString());

            // Proceed to payment form
            return RedirectToAction("PaymentForm");
        }

        public IActionResult PaymentForm()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            // Check if it's a project donation
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

                // Set ViewBag for project donation
                ViewBag.Project = project;
                ViewBag.Amount = amount;
                ViewBag.IsProjectDonation = true;
                return View();
            }
            
            // Handle program donation
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
        public IActionResult ProcessPayment(string cardNumber, string cardHolder, int expiryMonth, int expiryYear, string cvc)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "User");
            }

            int userId = GetCurrentUserId();
            int? projectId = HttpContext.Session.GetInt32("SelectedProjectId");
            decimal donationAmount = 0;
            
            // Check if it's a project donation
            if (projectId.HasValue)
            {
                var project = _context.Projects.Find(projectId.Value);
                if (project == null)
                {
                    return RedirectToAction("Index", "Projects");
                }

                // Get donation amount from session
                if (decimal.TryParse(HttpContext.Session.GetString("DonationAmount"), out decimal amount))
                {
                    donationAmount = amount;
                }
                else
                {
                    return RedirectToAction("Details", "Projects", new { id = projectId.Value });
                }

                // Create new donation record for project
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

                // Update project's current amount
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

                // Create payment record
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

                // Clear session data
                HttpContext.Session.Remove("SelectedProjectId");
                HttpContext.Session.Remove("DonationAmount");
            }
            else
            {
                // Handle regular program donation
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

                // Create new donation record
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

                // Create payment record
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

                // Clear session data
                HttpContext.Session.Remove("SelectedProgramId");
            }
            
            HttpContext.Session.Remove("DonatorFirstName");
            HttpContext.Session.Remove("DonatorLastName");
            HttpContext.Session.Remove("DonatorCountry");
            HttpContext.Session.Remove("DonatorCity");
            HttpContext.Session.Remove("DonatorAddress");

            // Redirect to success page
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

        [HttpPost]
        public IActionResult ProcessGiftDonation(string giverName, string giverEmail, string giverPhone,
            string receiverName, string receiverEmail, string receiverPhone,
            string donationType, decimal amount, bool includeReport, bool showAmount, string personalMessage)
        {
            // Create gift donation record
            var giftDonation = new GiftDonation
            {
                GiverName = giverName,
                GiverEmail = giverEmail,
                GiverPhone = giverPhone,
                ReceiverName = receiverName,
                ReceiverEmail = receiverEmail,
                ReceiverPhone = receiverPhone,
                DonationType = donationType,
                Amount = amount,
                IncludeReport = includeReport,
                ShowAmount = showAmount,
                PersonalMessage = personalMessage,
                CreatedAt = DateTime.Now
            };

            _context.GiftDonations.Add(giftDonation);
            _context.SaveChanges();

            int userId = GetCurrentUserId();

            // Create payment record for gift donation
            var payment = new Payment
            {
                UserId = userId,
                GiftDonationId = giftDonation.Id,
                PaymentMethod = "Credit Card",
                TransactionId = "GIFT" + DateTime.Now.Ticks.ToString(),
                Amount = amount,
                Status = "Completed",
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return RedirectToAction("DonationSuccess");
        }
    }
}
