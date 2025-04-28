using System.ComponentModel.DataAnnotations;

namespace UniversityPortal.Models
{
    public class Student
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [StringLength(10)]
        public string StudentId { get; set; } 
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public int? AcquiredCredits { get; set; }
        public int? CurrentSemester { get; set; }
        [StringLength(25)]
        public string? EducationLevel { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
