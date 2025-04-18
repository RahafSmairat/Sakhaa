using Microsoft.AspNetCore.Mvc;
using Sakhaa.Models;

namespace Sakhaa.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AdminController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult AddYearlyReport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddYearlyReport(IFormFile ReportFile, int ReportYear)
        {
            if (ReportFile != null && ReportFile.Length > 0)
            {
                // اسم الملف
                var fileName = Path.GetFileName(ReportFile.FileName);

                // تحديد المسار الذي سيتم تخزين الملف فيه داخل publicReports داخل wwwroot
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "publicReports", fileName);

                // التأكد من وجود المجلد
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "publicReports");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // حفظ الملف على الخادم
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ReportFile.CopyToAsync(stream);
                }

                // إنشاء الكائن وتخزينه في قاعدة البيانات
                var report = new PublicReport
                {
                    ReportYear = ReportYear,
                    CreatedAt = DateTime.Now,
                    FilePath = filePath,  // مسار الملف
                    ReportFileName = fileName  // اسم الملف
                };

                _context.PublicReports.Add(report);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحميل التقرير بنجاح!";
                return RedirectToAction("AddYearlyReport");
            }

            TempData["ErrorMessage"] = "حدث خطأ أثناء تحميل الملف.";
            return RedirectToAction("AddYearlyReport");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
