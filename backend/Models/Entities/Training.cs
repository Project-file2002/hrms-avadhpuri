namespace HRMS.API.Models.Entities;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Technical";
    public string? Instructor { get; set; }
    public double DurationHours { get; set; }
    public int MaxCapacity { get; set; } = 20;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; }

    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

public class TrainingEnrollment
{
    public int Id { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Enrolled";
    public double? Score { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}

public class Certification
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Issuer { get; set; }
    public int ExpiryDays { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EmployeeCertification
{
    public int Id { get; set; }
    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? CredentialUrl { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int CertificationId { get; set; }
    public Certification Certification { get; set; } = null!;
}
