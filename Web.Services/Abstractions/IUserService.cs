using Web.Data.Entities;
using Web.Data.Migrations;
using Web.DTOs;
using Web.Models;

namespace Web.Services.Abstractions;

public interface IUserService
{
    Task<Role?> GetUserRoleByNameAsync(string name, CancellationToken token);
    Task AddAdminRoleByUserIdAsync(Guid userId, CancellationToken token);
    Task<List<User>?> GetAllUsersAsync(CancellationToken token);
    Task RegisterUserAsync(UserRegisterModel model, CancellationToken token);
    Task<bool> CheckPasswordAsync(string email, string password, CancellationToken token);
    Task ResetPasswordAsync(string email, string password, CancellationToken token);
    Task<bool> CheckIsEmailRegisteredAsync(string email, CancellationToken token);
    Task<Guid?> GetUserIdByEmailAsync(string modelEmail, CancellationToken token);
    Task<User?> GetUserByEmailAsync(string modelEmail, CancellationToken token);
    Task RemoveUserByUserIdAsync(Guid userId, CancellationToken token);
    Task AddRoleToUserAsync(string email, string roleName, CancellationToken token);
    Task<List<string>> GetUserRolesByEmailAsync(string email, CancellationToken token);
    Task<UserTokenDto> GetUserDataByRefreshTokenIdAsync(Guid refreshTokenId, CancellationToken token);
    Task ChangeUserSettingsRankAsync(string userEmail, UserSettingsModel model, CancellationToken token);
}