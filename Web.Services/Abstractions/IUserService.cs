using Web.Models;

namespace Web.Services.Implementations;

public interface IUserService
{
    Task RegisterUserAsync(UserRegisterModel model, CancellationToken token);
    Task<bool> CheckPassword(string email, string password, CancellationToken token);
    Task<bool> CheckIsEmailRegisteredAsync(string email, CancellationToken token);
    Task<Guid?> GetUserIdByEmailAsync(string modelEmail, CancellationToken token);
    Task AddRoleToUserAsync(string email, string roleName, CancellationToken token);
    Task<List<string>> GetUserRolesByEmailAsync(string email, CancellationToken token);
}