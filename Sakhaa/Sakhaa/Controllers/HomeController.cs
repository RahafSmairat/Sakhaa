using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Models;

namespace Sakhaa.Controllers;

public class HomeController : Controller
{

    private readonly MyDbContext _context;

    public HomeController(MyDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        var latestNews = _context.NewsEvents
                               .OrderByDescending(n => n.CreatedAt)
                               .Take(6)
                               .ToList();
        
        
        
        var latestFeedbacks = _context.BeneficiaryFeedbacks
                                    .Where(f => f.BeneficiaryName != null && f.BeneficiaryName != "" && 
                                                f.Feedback != null && f.Feedback != "")
                                    .OrderByDescending(f => f.CreatedAt)
                                    .Take(3)
                                    .ToList();
        
        var activeProjects = _context.Projects
                                   .Where(p => p.IsActive == true)
                                   .OrderByDescending(p => p.CreatedAt)
                                   .Take(6)
                                   .ToList();
        
        ViewBag.LatestFeedbacks = latestFeedbacks;
        ViewBag.ActiveProjects = activeProjects;
        
        return View(latestNews);
    }

    public IActionResult NewsDetails(int id)
    {
        var newsEvent = _context.NewsEvents.FirstOrDefault(n => n.Id == id);
        if (newsEvent == null)
        {
            return NotFound();
        }
        return View(newsEvent);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult BeneficiaryApplication()
    {
        Console.WriteLine("GET BeneficiaryApplication called");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BeneficiaryApplication(BeneficiaryApplication application, IFormFile FamilyBookFile, IFormFile AidVerificationFile, IFormFile InsuranceStatusFile)
    {
        Console.WriteLine("POST BeneficiaryApplication called");
        try
        {
            
            if (!ModelState.IsValid)
            {
                
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToList();
                
                
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error in {error.Key}: {string.Join(", ", error.Errors.Select(e => e.ErrorMessage))}");
                }
                
                
                var errorDetails = string.Join(", ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Errors.Select(err => err.ErrorMessage))}"));
                ViewBag.FormError = $"هناك بعض المشاكل في النموذج. يرجى التحقق من البيانات المدخلة. التفاصيل: {errorDetails}";
                return View(application);
            }

            
            if (string.IsNullOrEmpty(application.FullName) || string.IsNullOrEmpty(application.PhoneNumber))
            {
                ViewBag.FormError = "الرجاء إدخال جميع البيانات المطلوبة.";
                return View(application);
            }

            
            if (FamilyBookFile != null && FamilyBookFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "beneficiaries");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FamilyBookFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await FamilyBookFile.CopyToAsync(fileStream);
                }

                application.FamilyBookUrl = uniqueFileName;
            }
            else
            {
                
                ViewBag.FormError = "الرجاء إرفاق دفتر العائلة.";
                return View(application);
            }

            
            if (AidVerificationFile != null && AidVerificationFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "beneficiaries");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AidVerificationFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AidVerificationFile.CopyToAsync(fileStream);
                }

                application.AidVerificationDocument = uniqueFileName;
            }
            else
            {
                
                ViewBag.FormError = "الرجاء إرفاق وثيقة إثبات الحاجة للمساعدة.";
                return View(application);
            }

            
            if (InsuranceStatusFile != null && InsuranceStatusFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "beneficiaries");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(InsuranceStatusFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await InsuranceStatusFile.CopyToAsync(fileStream);
                }

                application.InsuranceStatusDocument = uniqueFileName;
            }
            else
            {
                
                ViewBag.FormError = "الرجاء إرفاق وثيقة الوضع التأميني.";
                return View(application);
            }

            
            application.Status = "Pending";

            try
            {
                
                _context.BeneficiaryApplications.Add(application);
                await _context.SaveChangesAsync();
                
                
                return RedirectToAction("ApplicationSubmitted");
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Database error: {ex.Message}");
                ViewBag.FormError = "حدث خطأ أثناء حفظ البيانات. يرجى المحاولة مرة أخرى.";
                return View(application);
            }
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Unexpected error: {ex.Message}");
            ViewBag.FormError = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.";
            return View(application);
        }
    }

    public IActionResult ApplicationSubmitted()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SendMessage(string FullName, string Email, string Subject, string Message)
    {
        var contactMessage = new ContactMessage
        {
            FullName = FullName,
            Email = Email,
            Subject = Subject,
            Message = Message,
            CreatedAt = DateTime.Now
        };

        _context.ContactMessages.Add(contactMessage);
        _context.SaveChanges();

        try
        {
            var mail = new MailMessage();
            mail.From = new MailAddress("rahaf.alsmairat@gmail.com", "موقع سخاء");
            mail.To.Add("rahaf.alsmairat@gmail.com");
            mail.Subject = $"{Subject}";
            mail.Body = $"الاسم: {FullName}\nالبريد: {Email}\n\nالرسالة:\n{Message}";
            mail.ReplyToList.Add(new MailAddress(Email));

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("rahaf.alsmairat@gmail.com", "bynd hacu xoiu whgq"),
                EnableSsl = true
            };

            smtp.Send(mail);
        }
        catch (SmtpException smtpEx)
        {
            ViewBag.Error = smtpEx.Message;
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }
        return View("Contact");
    }

    public IActionResult PublicReports(int? year)
    {
        var years = _context.PublicReports.Select(r => r.ReportYear).Distinct().ToList();
        ViewBag.Years = years;
        ViewBag.SelectedYear = year;

        var reports = year.HasValue ?
            _context.PublicReports.Where(r => r.ReportYear == year.Value).ToList() :
            _context.PublicReports.ToList(); 

        return View(reports);
    }

    public IActionResult SuccessStories()
    {
        var stories = _context.SuccessStories.ToList();
        return View(stories);
    }
}
