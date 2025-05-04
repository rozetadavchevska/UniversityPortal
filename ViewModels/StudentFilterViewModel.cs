using UniversityPortal.Models;

namespace UniversityPortal.ViewModels
{
    public class StudentFilterViewModel
    {
        public string? StudentId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public List<Student>? Students { get; set; }
    }

}
