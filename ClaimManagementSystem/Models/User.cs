using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClaimManagementSystem.Models
{
    public class User
    {
        [Key]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public bool IsAuthenticated { get; set; }
    }

    public enum UserRole
    {
        Lecturer,
        ProgramCoordinator,
        AcademicManager,
        HRManager
    }
}