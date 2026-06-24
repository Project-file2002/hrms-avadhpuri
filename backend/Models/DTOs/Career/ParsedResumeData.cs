namespace HRMS.API.Models.DTOs.Career;

public class ParsedResumeData
{
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? CurrentAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? ProfessionalStatus { get; set; }
    public string? CurrentCompany { get; set; }
    public string? CurrentDesignation { get; set; }
    public int? TotalExperienceMonths { get; set; }
    public decimal? CurrentCtc { get; set; }
    public decimal? ExpectedCtc { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public List<ParsedEducation>? Education { get; set; }
    public List<ParsedExperience>? Experience { get; set; }
    public List<ParsedProject>? Projects { get; set; }
    public List<ParsedInternship>? Internships { get; set; }
    public List<ParsedSkill>? Skills { get; set; }
    public List<ParsedCertification>? Certifications { get; set; }
    public List<ParsedTraining>? Training { get; set; }
    public List<ParsedAchievement>? Achievements { get; set; }
    public List<ParsedLanguage>? Languages { get; set; }
    public List<ParsedReference>? References { get; set; }
}

public class ParsedEducation
{
    public string? Degree { get; set; }
    public string? Institution { get; set; }
    public int? YearOfPassing { get; set; }
    public string? Percentage { get; set; }
    public string? Specialization { get; set; }
    public bool? IsPursuing { get; set; }
}

public class ParsedExperience
{
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
    public string? Responsibilities { get; set; }
}

public class ParsedProject
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? TechStack { get; set; }
    public string? Url { get; set; }
    public string? Duration { get; set; }
}

public class ParsedInternship
{
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? Duration { get; set; }
    public string? Description { get; set; }
}

public class ParsedSkill
{
    public string? Skill { get; set; }
    public string? Proficiency { get; set; }
    public int? YearsOfExperience { get; set; }
}

public class ParsedCertification
{
    public string? Name { get; set; }
    public string? Issuer { get; set; }
    public int? Year { get; set; }
    public string? Url { get; set; }
}

public class ParsedTraining
{
    public string? Title { get; set; }
    public string? Provider { get; set; }
    public int? Year { get; set; }
    public string? Duration { get; set; }
}

public class ParsedAchievement
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public int? Year { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
}

public class ParsedLanguage
{
    public string? Language { get; set; }
    public string? Proficiency { get; set; }
    public bool? CanRead { get; set; }
    public bool? CanWrite { get; set; }
    public bool? CanSpeak { get; set; }
}

public class ParsedReference
{
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
