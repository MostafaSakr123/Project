using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Section
    {
        public string CourseCode { get; set; }
        public int SectionNumber { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "The day must be a valid string with a reasonable length.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "The day must be a string with letters only.")]
        public string Day { get; set; }

        [Required(ErrorMessage = "Start Time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Professor is required")]
        public int ProfID { get; set; }

        [Required(ErrorMessage = "Professor is required")]

        public string ProfessorName { get; set; }
    }
}