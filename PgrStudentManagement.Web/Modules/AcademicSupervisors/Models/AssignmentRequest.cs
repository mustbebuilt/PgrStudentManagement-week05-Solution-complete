using System.ComponentModel.DataAnnotations;

namespace PgrStudentManagement.Web.Modules.AcademicSupervisors.Models;

public class AssignmentRequest
{
    [Required]
    public string StudentNumber { get; set; }
        = string.Empty;

    [Required]
    public string StaffNumber { get; set; }
        = string.Empty;

    [Required]
    public SupervisoryRole Role { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
        = DateTime.Today;

    public bool ExceptionAuthorised { get; set; }
}