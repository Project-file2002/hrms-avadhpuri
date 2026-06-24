using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HRMS.API.Data;
using HRMS.API.Models.DTOs.Auth;
using HRMS.API.Models.Entities;
using HRMS.API.Models.Mapping;
using HRMS.API.Services.Interfaces;

namespace HRMS.API.Services;

public class AuthService : IAuthService
{
    private readonly HRMSDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(HRMSDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCryptVerify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.UserRoles.All(ur => ur.Role.Name == "Candidate"))
            throw new UnauthorizedAccessException("Invalid credentials");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return GenerateLoginResponse(user);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCryptHash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var employeeRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
        if (employeeRole != null)
        {
            user.UserRoles.Add(new UserRole { Role = employeeRole });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user).Collection(u => u.UserRoles).Query().Include(ur => ur.Role).LoadAsync();

        return GenerateLoginResponse(user);
    }

    private LoginResponse GenerateLoginResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "SuperSecretKeyForHRMS2024Minimum32Chars!");
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _configuration["Jwt:Issuer"] ?? "HRMS.API",
            Audience = _configuration["Jwt:Audience"] ?? "HRMS.App",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = "",
            ExpiresAt = expiresAt,
            User = user.ToDto()
        };
    }

    private static string BCryptHash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    private static bool BCryptVerify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
