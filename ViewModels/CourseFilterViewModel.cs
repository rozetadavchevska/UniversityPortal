using UniversityPortal.Models;

namespace UniversityPortal.ViewModels
{
    public class CourseFilterViewModel
    {
        public string? Title { get; set; }
        public int? Semester { get; set; }
        public string? Programme { get; set; }

        public List<Course>? Courses { get; set; }
    }
}
