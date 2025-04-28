using System;
using System.ComponentModel.DataAnnotations;

namespace Sakhaa.Models.ViewModels
{
    public class ReportViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى إدخال اسم التقرير")]
        [Display(Name = "اسم التقرير")]
        public string ReportFileName { get; set; }

        [Required(ErrorMessage = "يرجى إدخال سنة التقرير")]
        [Display(Name = "سنة التقرير")]
        [Range(2000, 2100, ErrorMessage = "يرجى إدخال سنة صحيحة بين 2000 و 2100")]
        public int ReportYear { get; set; }

        // Optional - will be set by controller
        public string FilePath { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
} 