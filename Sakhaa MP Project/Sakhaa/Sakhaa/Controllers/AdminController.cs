using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Sakhaa.Models.ViewModels;

namespace Sakhaa.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }

        // Admin access verification
        private bool IsAdmin()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return false;

            string isAdmin = HttpContext.Session.GetString("IsAdmin");
            return isAdmin == "true";
        }

        private IActionResult CheckAdminAccess()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "User");
            }
            return null;
        }

        // Dashboard
        public IActionResult Dashboard()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "لوحة التحكم";
            ViewData["ActivePage"] = "Dashboard";

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalDonations = _context.Donations.Count();
            ViewBag.TotalAmount = _context.Donations.Sum(d => d.Amount);
            ViewBag.TotalPrograms = _context.DonationPrograms.Count();
            ViewBag.RecentDonations = _context.Donations
                .Include(d => d.User)
                .Include(d => d.Program)
                .Include(d => d.Project)
                .OrderByDescending(d => d.DonationStartDate)
                .Take(5)
                .ToList();
            ViewBag.DonationPrograms = _context.DonationPrograms.ToList();

            // Get monthly donation amounts for the chart
            var monthlyDonations = _context.Donations
                .Where(d => d.DonationStartDate.HasValue && d.DonationStartDate.Value.Year == DateTime.Now.Year)
                .GroupBy(d => d.DonationStartDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalAmount = g.Sum(d => d.Amount)
                })
                .ToList();

            // Create an array of length 12 for all months
            decimal[] monthlyAmounts = new decimal[12];
            foreach (var data in monthlyDonations)
            {
                monthlyAmounts[data.Month - 1] = data.TotalAmount;
            }

            ViewBag.MonthlyDonations = monthlyAmounts;

            return View();
        }

        // Users Management
        public IActionResult Users()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة المستخدمين";
            ViewData["ActivePage"] = "Users";

            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult UserDetails(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تفاصيل المستخدم";
            ViewData["ActivePage"] = "Users";

            var user = _context.Users
                .Include(u => u.Donations)
                .ThenInclude(d => d.Program)
                .Include(u => u.Payments)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Donation Programs Management
        public IActionResult DonationPrograms()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة برامج التبرع";
            ViewData["ActivePage"] = "DonationPrograms";

            var programs = _context.DonationPrograms.ToList();
            return View(programs);
        }

        public IActionResult CreateProgram()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة برنامج جديد";
            ViewData["ActivePage"] = "DonationPrograms";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram(DonationProgram program, IFormFile ProgramImage)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                if (ProgramImage != null && ProgramImage.Length > 0)
                {
                    // Generate a unique filename
                    string fileExtension = Path.GetExtension(ProgramImage.FileName);
                    string fileName = $"program_{DateTime.Now.Ticks}{fileExtension}";

                    // Save the file
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProgramImage.CopyToAsync(stream);
                    }

                    // Update the program with the image path
                    program.ImagePath = fileName;
                }

                _context.DonationPrograms.Add(program);
                _context.SaveChanges();
                return RedirectToAction("DonationPrograms");
            }

            ViewData["Title"] = "إضافة برنامج جديد";
            ViewData["ActivePage"] = "DonationPrograms";

            return View(program);
        }

        public IActionResult EditProgram(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تعديل برنامج التبرع";
            ViewData["ActivePage"] = "DonationPrograms";

            var program = _context.DonationPrograms.Find(id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        [HttpPost]
        public async Task<IActionResult> EditProgram(DonationProgram program, IFormFile ProgramImage)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                if (ProgramImage != null && ProgramImage.Length > 0)
                {
                    // Delete old image if exists
                    var oldProgram = _context.DonationPrograms.AsNoTracking().FirstOrDefault(p => p.Id == program.Id);
                    if (oldProgram != null && !string.IsNullOrEmpty(oldProgram.ImagePath))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", oldProgram.ImagePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate a unique filename
                    string fileExtension = Path.GetExtension(ProgramImage.FileName);
                    string fileName = $"program_{DateTime.Now.Ticks}{fileExtension}";

                    // Save the file
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProgramImage.CopyToAsync(stream);
                    }

                    // Update the program with the new image path
                    program.ImagePath = fileName;
                }

                _context.Update(program);
                _context.SaveChanges();
                return RedirectToAction("DonationPrograms");
            }

            ViewData["Title"] = "تعديل برنامج التبرع";
            ViewData["ActivePage"] = "DonationPrograms";

            return View(program);
        }

        [HttpPost]
        public IActionResult DeleteProgram(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var program = _context.DonationPrograms.Find(id);
            if (program == null)
            {
                return NotFound();
            }

            _context.DonationPrograms.Remove(program);
            _context.SaveChanges();
            return RedirectToAction("DonationPrograms");
        }

        // Donations Management
        public IActionResult Donations()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة التبرعات";
            ViewData["ActivePage"] = "Donations";

            var donations = _context.Donations
                .Include(d => d.User)
                .Include(d => d.Program)
                .OrderByDescending(d => d.DonationStartDate)
                .ToList();

            return View(donations);
        }

        public IActionResult DonationDetails(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تفاصيل التبرع";
            ViewData["ActivePage"] = "Donations";

            var donation = _context.Donations
                .Include(d => d.User)
                .Include(d => d.Program)
                .Include(d => d.Payments)
                .Include(d => d.DonationReports)
                .FirstOrDefault(d => d.Id == id);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // Beneficiaries Management
        public IActionResult Beneficiaries()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة المستفيدين";
            ViewData["ActivePage"] = "Beneficiaries";

            var applications = _context.BeneficiaryApplications.ToList();
            return View(applications);
        }

        public IActionResult BeneficiaryDetails(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تفاصيل المستفيد";
            ViewData["ActivePage"] = "Beneficiaries";

            var application = _context.BeneficiaryApplications.Find(id);
            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        [HttpPost]
        public IActionResult DeleteBeneficiaryApplication(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var application = _context.BeneficiaryApplications.Find(id);
            if (application == null)
            {
                return NotFound();
            }

            // Delete associated files
            if (!string.IsNullOrEmpty(application.FamilyBookUrl))
            {
                string filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "documents",
                    "beneficiaries",
                    application.FamilyBookUrl);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        // Log error if needed, but continue with the delete
                    }
                }
            }

            if (!string.IsNullOrEmpty(application.AidVerificationDocument))
            {
                string filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "documents",
                    "beneficiaries",
                    application.AidVerificationDocument);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        // Log error if needed, but continue with the delete
                    }
                }
            }

            if (!string.IsNullOrEmpty(application.InsuranceStatusDocument))
            {
                string filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "documents",
                    "beneficiaries",
                    application.InsuranceStatusDocument);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        // Log error if needed, but continue with the delete
                    }
                }
            }

            _context.BeneficiaryApplications.Remove(application);
            _context.SaveChanges();

            return RedirectToAction("Beneficiaries");
        }

        public IActionResult DownloadBeneficiaryAsPdf(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var application = _context.BeneficiaryApplications.Find(id);
            if (application == null)
            {
                return NotFound();
            }

            // For this implementation, we'll return the view as it would typically
            // be converted to PDF using a library like DinkToPdf, Rotativa, or similar
            // In a real implementation, you would use a PDF library to generate the PDF
            
            // Here we're just returning a view that can be styled for PDF download
            ViewData["Title"] = "طباعة طلب المستفيد";
            ViewData["ActivePage"] = "Beneficiaries";
            ViewData["IsPdfView"] = true;

            return View("BeneficiaryDetailsPdf", application);
        }

        [HttpPost]
        public IActionResult UpdateBeneficiaryStatus(int id, string status, string notes)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var application = _context.BeneficiaryApplications.Find(id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = status;
            application.Notes = notes;
            _context.SaveChanges();

            return RedirectToAction("BeneficiaryDetails", new { id = id });
        }

        // Success Stories Management
        public IActionResult SuccessStories()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة قصص النجاح";
            ViewData["ActivePage"] = "SuccessStories";

            var stories = _context.SuccessStories.ToList();
            return View(stories);
        }

        public IActionResult CreateStory()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة قصة نجاح جديدة";
            ViewData["ActivePage"] = "SuccessStories";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(SuccessStory story, IFormFile ImageFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                // Set creation date
                story.CreatedAt = DateTime.Now;

                // Handle file upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Ensure the directory exists
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Create a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Store only the filename in the database
                    story.ImagePath = uniqueFileName;
                }

                _context.SuccessStories.Add(story);
                await _context.SaveChangesAsync();
                return RedirectToAction("SuccessStories");
            }

            ViewData["Title"] = "إضافة قصة نجاح جديدة";
            ViewData["ActivePage"] = "SuccessStories";

            return View(story);
        }

        public IActionResult EditStory(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تعديل قصة النجاح";
            ViewData["ActivePage"] = "SuccessStories";

            var story = _context.SuccessStories.Find(id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        [HttpPost]
        public async Task<IActionResult> EditStory(SuccessStory story, IFormFile ImageFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                // Handle file upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Get the original story to find the old image path
                    var originalStory = await _context.SuccessStories.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == story.Id);

                    // Delete old image if it exists
                    if (originalStory != null && !string.IsNullOrEmpty(originalStory.ImagePath))
                    {
                        string oldImagePath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            originalStory.ImagePath);

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (Exception)
                            {
                                // Log error if needed, but continue with the update
                            }
                        }
                    }

                    // Ensure the directory exists
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Create a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Store only the filename in the database
                    story.ImagePath = uniqueFileName;
                }

                _context.Update(story);
                await _context.SaveChangesAsync();
                return RedirectToAction("SuccessStories");
            }

            ViewData["Title"] = "تعديل قصة النجاح";
            ViewData["ActivePage"] = "SuccessStories";

            return View(story);
        }

        [HttpPost]
        public IActionResult DeleteStory(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var story = _context.SuccessStories.Find(id);
            if (story == null)
            {
                return NotFound();
            }

            _context.SuccessStories.Remove(story);
            _context.SaveChanges();
            return RedirectToAction("SuccessStories");
        }

        // Reports Management
        public IActionResult Reports()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة التقارير";
            ViewData["ActivePage"] = "Reports";

            var reports = _context.PublicReports.ToList();
            return View(reports);
        }

        public IActionResult CreateReport()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة تقرير جديد";
            ViewData["ActivePage"] = "Reports";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(PublicReport report, IFormFile ReportFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة تقرير جديد";
            ViewData["ActivePage"] = "Reports";

            // Remove model state errors for file path since we'll handle it separately
            ModelState.Remove("FilePath");

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "يرجى التحقق من صحة البيانات المدخلة.";
                    return View(report);
                }

                // Ensure the reports directory exists
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "reports");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Process the report file
                if (ReportFile != null && ReportFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ReportFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to disk
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReportFile.CopyToAsync(fileStream);
                    }

                    // Set the file path in the report object
                    report.FilePath = uniqueFileName;
                }
                else
                {
                    TempData["ErrorMessage"] = "الرجاء اختيار ملف التقرير.";
                    return View(report);
                }

                // Set creation date
                report.CreatedAt = DateTime.Now;

                // Save to database
                _context.PublicReports.Add(report);
                await _context.SaveChangesAsync();

                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ أثناء حفظ التقرير: {ex.Message}";
                return View(report);
            }
        }

        public IActionResult EditReport(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تعديل التقرير";
            ViewData["ActivePage"] = "Reports";

            var report = _context.PublicReports.Find(id);
            if (report == null)
            {
                return NotFound();
            }

            var viewModel = new ReportViewModel
            {
                Id = report.Id,
                ReportFileName = report.ReportFileName,
                ReportYear = report.ReportYear,
                FilePath = report.FilePath,
                CreatedAt = report.CreatedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditReport(ReportViewModel viewModel, IFormFile ReportFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                try
                {
                    // Find existing report
                    var report = await _context.PublicReports.FindAsync(viewModel.Id);
                    if (report == null)
                    {
                        return NotFound();
                    }

                    // Update report properties
                    report.ReportFileName = viewModel.ReportFileName;
                    report.ReportYear = viewModel.ReportYear;

                    // Handle file upload if a new file is provided
                    if (ReportFile != null && ReportFile.Length > 0)
                    {
                        // Ensure directories exist
                        string documentsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                        if (!Directory.Exists(documentsFolder))
                        {
                            Directory.CreateDirectory(documentsFolder);
                        }

                        string reportsFolder = Path.Combine(documentsFolder, "reports");
                        if (!Directory.Exists(reportsFolder))
                        {
                            Directory.CreateDirectory(reportsFolder);
                        }

                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(report.FilePath))
                        {
                            string oldFilePath = Path.Combine(reportsFolder, report.FilePath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generate a unique filename
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ReportFile.FileName);
                        string filePath = Path.Combine(reportsFolder, fileName);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ReportFile.CopyToAsync(fileStream);
                        }

                        // Update the file path
                        report.FilePath = fileName;
                    }

                    _context.Update(report);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("Reports");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"حدث خطأ أثناء تحديث التقرير: {ex.Message}");
                }
            }

            ViewData["Title"] = "تعديل التقرير";
            ViewData["ActivePage"] = "Reports";
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var report = _context.PublicReports.Find(id);
            if (report == null)
            {
                return NotFound();
            }

            // Delete the file if it exists
            if (!string.IsNullOrEmpty(report.FilePath))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "reports", report.FilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.PublicReports.Remove(report);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Reports");
        }

        // Messages Management
        public IActionResult Messages()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة الرسائل";
            ViewData["ActivePage"] = "Messages";

            var messages = _context.ContactMessages.OrderByDescending(m => m.CreatedAt).ToList();
            return View(messages);
        }

        public IActionResult MessageDetails(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تفاصيل الرسالة";
            ViewData["ActivePage"] = "Messages";

            var message = _context.ContactMessages.Find(id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // News & Events Management
        public IActionResult NewsEvents()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة الأخبار والفعاليات";
            ViewData["ActivePage"] = "NewsEvents";

            var newsEvents = _context.NewsEvents.OrderByDescending(n => n.EventDate).ToList();
            return View(newsEvents);
        }

        public IActionResult CreateNewsEvent()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة خبر أو فعالية جديدة";
            ViewData["ActivePage"] = "NewsEvents";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewsEvent(NewsEvent newsEvent, IFormFile ImageFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                newsEvent.CreatedAt = DateTime.Now;
                
                // Handle image upload if provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Generate a unique filename
                    string fileExtension = Path.GetExtension(ImageFile.FileName);
                    string fileName = $"news_{DateTime.Now.Ticks}{fileExtension}";

                    // Ensure directory exists
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "news");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Save the file
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Update the news event with the image path
                    newsEvent.Image = $"news/{fileName}";
                }
                
                _context.NewsEvents.Add(newsEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction("NewsEvents");
            }

            ViewData["Title"] = "إضافة خبر أو فعالية جديدة";
            ViewData["ActivePage"] = "NewsEvents";

            return View(newsEvent);
        }

        public IActionResult EditNewsEvent(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تعديل الخبر أو الفعالية";
            ViewData["ActivePage"] = "NewsEvents";

            var newsEvent = _context.NewsEvents.Find(id);
            if (newsEvent == null)
            {
                return NotFound();
            }

            return View(newsEvent);
        }

        [HttpPost]
        public async Task<IActionResult> EditNewsEvent(NewsEvent newsEvent, IFormFile ImageFile)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            try
            {
                if (ModelState.IsValid)
                {
                    // Log the received data for debugging
                    Console.WriteLine($"Received NewsEvent - Id: {newsEvent.Id}, Title: {newsEvent.Title}, Content length: {newsEvent.Content?.Length}, EventDate: {newsEvent.EventDate}");

                    // Handle image upload if provided
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        var oldNewsEvent = _context.NewsEvents.AsNoTracking().FirstOrDefault(n => n.Id == newsEvent.Id);
                        if (oldNewsEvent != null && !string.IsNullOrEmpty(oldNewsEvent.Image))
                        {
                            string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", oldNewsEvent.Image);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generate a unique filename
                        string fileExtension = Path.GetExtension(ImageFile.FileName);
                        string fileName = $"news_{DateTime.Now.Ticks}{fileExtension}";

                        // Ensure directory exists
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "news");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Save the file
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Update the news event with the image path
                        newsEvent.Image = $"news/{fileName}";
                    }
                    else
                    {
                        // Preserve the existing image if no new image was uploaded
                        var existingNewsEvent = await _context.NewsEvents.AsNoTracking()
                            .FirstOrDefaultAsync(n => n.Id == newsEvent.Id);
                            
                        if (existingNewsEvent != null)
                        {
                            newsEvent.Image = existingNewsEvent.Image;
                        }
                    }

                    // Make sure we're updating the entity properly
                    _context.Entry(newsEvent).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("NewsEvents");
                }

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model error: {error.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating news event: {ex.Message}");
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ التغييرات. يرجى المحاولة مرة أخرى.");
            }

            ViewData["Title"] = "تعديل الخبر أو الفعالية";
            ViewData["ActivePage"] = "NewsEvents";

            return View(newsEvent);
        }

        [HttpPost]
        public IActionResult DeleteNewsEvent(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var newsEvent = _context.NewsEvents.Find(id);
            if (newsEvent == null)
            {
                return NotFound();
            }

            // Delete the image file if it exists
            if (!string.IsNullOrEmpty(newsEvent.Image))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", newsEvent.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.NewsEvents.Remove(newsEvent);
            _context.SaveChanges();
            return RedirectToAction("NewsEvents");
        }

        // Projects Management
        public IActionResult Projects()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إدارة المشاريع";
            ViewData["ActivePage"] = "Projects";

            var projects = _context.Projects.OrderByDescending(p => p.CreatedAt).ToList();
            return View(projects);
        }

        public IActionResult CreateProject()
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "إضافة مشروع جديد";
            ViewData["ActivePage"] = "Projects";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project, IFormFile ProjectImage)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                // Set creation date
                project.CreatedAt = DateTime.Now;

                // Set default values if not provided
                if (!project.CurrentAmount.HasValue)
                    project.CurrentAmount = 0;

                if (!project.IsActive.HasValue)
                    project.IsActive = true;

                // Handle file upload if provided
                if (ProjectImage != null && ProjectImage.Length > 0)
                {
                    // Generate a unique filename
                    string fileExtension = Path.GetExtension(ProjectImage.FileName);
                    string fileName = $"project_{DateTime.Now.Ticks}{fileExtension}";

                    // Save the file
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProjectImage.CopyToAsync(stream);
                    }

                    // Update the project with the image path
                    project.ImageUrl = fileName;
                }

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Projects");
            }

            ViewData["Title"] = "إضافة مشروع جديد";
            ViewData["ActivePage"] = "Projects";

            return View(project);
        }

        public IActionResult EditProject(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            ViewData["Title"] = "تعديل المشروع";
            ViewData["ActivePage"] = "Projects";

            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> EditProject(Project project, IFormFile ProjectImage)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            if (ModelState.IsValid)
            {
                // Handle file upload if provided
                if (ProjectImage != null && ProjectImage.Length > 0)
                {
                    // Delete old image if exists
                    var oldProject = _context.Projects.AsNoTracking().FirstOrDefault(p => p.Id == project.Id);
                    if (oldProject != null && !string.IsNullOrEmpty(oldProject.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", oldProject.ImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate a unique filename
                    string fileExtension = Path.GetExtension(ProjectImage.FileName);
                    string fileName = $"project_{DateTime.Now.Ticks}{fileExtension}";

                    // Save the file
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProjectImage.CopyToAsync(stream);
                    }

                    // Update the project with the new image path
                    project.ImageUrl = fileName;
                }

                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Projects");
            }

            ViewData["Title"] = "تعديل المشروع";
            ViewData["ActivePage"] = "Projects";

            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var checkResult = CheckAdminAccess();
            if (checkResult != null) return checkResult;

            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound();
            }

            // Delete project image if exists
            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", project.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Projects");
        }
    }
}
