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

        [HttpPost]
        public IActionResult ProcessGiftDonation(string giverName, string giverEmail, string giverPhone,
            string receiverName, string receiverEmail, string receiverPhone,
            string donationType, decimal amount, bool includeReport, bool showAmount, string personalMessage)
        {
            
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
                ShowAmount = showAmount,
                PersonalMessage = personalMessage,
                CreatedAt = DateTime.Now
            };

            _context.GiftDonations.Add(giftDonation);
            _context.SaveChanges();

            int userId = GetCurrentUserId();

            
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
