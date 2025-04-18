using Microsoft.AspNetCore.Mvc;

namespace Sakhaa.Controllers
{
    public class DonationController : Controller
    {
        public IActionResult DonationPrograms()
        {
            return View();
        }

        public IActionResult DonationProgramsDetails()
        {
            return View();
        }

        public IActionResult DonationApplicationForm()
        {
            return View();
        }

        public IActionResult PaymentForm()
        {
            return View();
        }

        public IActionResult GiftApplicationForm()
        {
            return View();
        }
    }
}
