using System.Collections.Generic;

namespace Project.Models
{
    public class Course
    {
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string MajorCode { get; set; }
        public int NumberOfSections { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
    }
}


