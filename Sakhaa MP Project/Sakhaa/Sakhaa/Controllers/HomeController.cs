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
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
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

        //////////////////////// I have a problem here it is not sending the email
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
