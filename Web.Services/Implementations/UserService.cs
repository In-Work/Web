using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Web.Data;
using Web.Data.Entities;
using Web.Models;
using Web.Services.Abstractions;

namespace Web.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationContext _context;

    public UserService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task RegisterUserAsync(UserRegisterModel userModel, CancellationToken token)
    {
        var userRole = await _context.Roles
            .SingleOrDefaultAsync(r => r.RoleName.Equals("User"), token);

        if (userRole != null)
        {
            var secStamp = Guid.NewGuid().ToString("N");
            var passwordHash = await GetPasswordHash(userModel.Password, secStamp);

            var user = new User
            {
                Name = userModel.Name,
                Email = userModel.Email,
                PasswordHash = passwordHash,
                SecurityStamp = secStamp
            };

            user.UserRoles.Add(userRole);

            await _context.Users.AddAsync(user, token);
            await _context.SaveChangesAsync(token);

            // TODO: to improve: email confirm, sms, etc
        }
    }

    public async Task<bool> CheckPassword(string email, string password, CancellationToken token)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Email.Equals(email), cancellationToken: token);

        if (user == null) return false;
        var passwordHash = await GetPasswordHash(password, user.SecurityStamp);

        return user.PasswordHash.Equals(passwordHash);
    }

    private async Task<string> GetPasswordHash(string password, string secStamp)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{password}{secStamp}");
        var memoryStream = new MemoryStream(inputBytes);
        var hashBytes = await md5.ComputeHashAsync(memoryStream);
        var hashedPassword = Encoding.UTF8.GetString(hashBytes);
        return hashedPassword;
    }

    public async Task<bool> CheckIsEmailRegisteredAsync(string email, CancellationToken token)
    {
        var result = await _context.Users
            .AnyAsync(user => user.Email.Equals(email), token);
        return result;
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken token)
    {
        return await _context.Users
            .Where(u => u.Email.Equals(email))
            .Select(u => u.Id)
            .FirstOrDefaultAsync(token);
    }

    public async Task AddRoleToUserAsync(string email, string roleName, CancellationToken token = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.RoleName)
            .FirstOrDefaultAsync(u => u.Email == email, token);

        if (user == null) throw new Exception("User not found");
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName, token);

        if (userRole == null) throw new Exception("Role not found");
        if (user.UserRoles.All(ur => ur.Id != userRole.Id))
        {
            user.UserRoles.Add( new Role
            {
                Id = userRole.Id,
                RoleName = roleName
            });

            await _context.SaveChangesAsync(token);
        }
    }

    public async Task<List<string>> GetUserRolesByEmailAsync(string email, CancellationToken token = default)
    {
        var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Email == email, token);
        if (user == null) throw new Exception("User not found");
        return user.UserRoles.Select(ur=>ur.RoleName).ToList();
    }
}