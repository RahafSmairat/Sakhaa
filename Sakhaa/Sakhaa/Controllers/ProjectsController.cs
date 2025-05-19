using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sakhaa.Models;
using System.Threading.Tasks;

namespace Sakhaa.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly MyDbContext _context;

        public ProjectsController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Where(p => p.IsActive == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public IActionResult Donate(int id, decimal amount)
        {
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound();
            }

            HttpContext.Session.Remove("SelectedProgramId");
            
            return RedirectToAction("DonationApplicationForm", "Donation", new { projectId = id, amount = amount });
        }
    }
}