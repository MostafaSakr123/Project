namespace Project.Models
{
    public class RegisteredCourse
    {
        public int StudentId { get; set; }
        public string CourseCode { get; set; }
        public int SectionNumber { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}