namespace Web.Services.Abstractions
{
    public interface ITokenService
    {
        public Task<string?> GenerateJwtTokenStringAsync(Guid userId, List<string> roles, CancellationToken token = default);
        public Task<string?> GenerateRefreshTokenAsync(Guid userId, string deviceInfo, CancellationToken token = default);
        Task<bool> CheckIsRefreshTokenCorrectByIdAsync(Guid tokenId, CancellationToken token = default);
    }
}
