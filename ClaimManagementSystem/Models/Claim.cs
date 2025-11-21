using System;
using System.ComponentModel.DataAnnotations;

namespace ClaimManagementSystem.Models
{
    public class Claim
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string MonthYear { get; set; } = string.Empty;

        [Range(1, 744)]
        public decimal Hours { get; set; }

        [Range(0.01, 1000)]
        public decimal HourlyRate { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

        public DateTime? SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? PaidDate { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        public string? ReviewedBy { get; set; }
        public string? ApprovedBy { get; set; }

        public string FilePaths { get; set; } = string.Empty;
    }

    public enum ClaimStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Paid,
        Rejected
    }
}