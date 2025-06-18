using Microsoft.Extensions.Caching.Memory;
using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Core.Models;

namespace ReqresIntegration.Infrastructure.Services
{
    public class CachedExternalUserService : IExternalUserService
    {
        private readonly IExternalUserService _inner;
        private readonly IMemoryCache _cache;

        public CachedExternalUserService(IExternalUserService inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            return await _cache.GetOrCreateAsync($"user_{userId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return _inner.GetUserByIdAsync(userId);
            });
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _cache.GetOrCreateAsync("all_users", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return _inner.GetAllUsersAsync();
            });
        }
    }
}
