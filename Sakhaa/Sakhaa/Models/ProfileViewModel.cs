using Microsoft.AspNetCore.Mvc;

namespace Sakhaa.Models
{
    public class ProfileViewModel 
    {
        public User userInfo { get; set; }
        public List<Donation> PreviousDonations { get; set; }
        public List<Donation> CurrentDonations { get; set; }
        public List<DonationReport> DonationReports { get; set; }
    }
}
