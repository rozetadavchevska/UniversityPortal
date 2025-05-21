using System.ComponentModel.DataAnnotations;

namespace UniversityPortal.Models
{
    public class Student
    {
        public string Id { get; set; }
        [Display(Name = "Student ID")]
        [Required]
        [StringLength(10)]
        public string StudentId { get; set; }
        [Display(Name = "First Name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Display(Name = "Enrollment Date")]
        public DateTime? EnrollmentDate { get; set; }
        [Display(Name = "Acquired Credits")]
        public int? AcquiredCredits { get; set; }
        [Display(Name = "Current Semester")]
        public int? CurrentSemester { get; set; }
        [Display(Name = "Education Level")]
        [StringLength(25)]
        public string? EducationLevel { get; set; }
        [Display(Name = "Profile Image")]
        public string? ProfileImageUrl { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
