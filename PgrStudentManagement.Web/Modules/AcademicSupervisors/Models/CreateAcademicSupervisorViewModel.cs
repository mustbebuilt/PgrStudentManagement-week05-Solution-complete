using System.ComponentModel.DataAnnotations;

namespace PgrStudentManagement.Web.Modules
    .AcademicSupervisors.Models;

public class CreateAcademicSupervisorViewModel
{
    [Required(ErrorMessage = "Staff number is required.")]
    [StringLength(
        20,
        ErrorMessage =
            "Staff number must not exceed 20 characters.")]
    [Display(Name = "Staff number")]
    public string StaffNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(
        100,
        ErrorMessage =
            "First name must not exceed 100 characters.")]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(
        100,
        ErrorMessage =
            "Last name must not exceed 100 characters.")]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(
        255,
        ErrorMessage =
            "Email address must not exceed 255 characters.")]
    [Display(Name = "Email address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employment type is required.")]
    [Display(Name = "Employment type")]
    public SupervisorEmploymentType? EmploymentType
    {
        get;
        set;
    }

    [Display(Name = "Required supervisor training completed")]
    public bool TrainingCompleted { get; set; }

    [Display(Name = "Eligible supervisory roles")]
    public List<SupervisoryRole> EligibleRoles { get; set; }
        = [];
}