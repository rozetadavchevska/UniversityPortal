using UniversityPortal.Models;

namespace UniversityPortal.ViewModels
{
    public class TeacherFilterViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Degree { get; set; }
        public string? AcademicRank { get; set; }

        public List<Teacher>? Teachers { get; set; }
    }

}
