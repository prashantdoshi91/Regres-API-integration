using ReqresIntegration.Core.Models;

namespace ReqresIntegration.Infrastructure.Interfaces
{
    public interface IReqresApiClient
    {
        Task<UserDto> GetUserAsync(int userId);
        Task<List<UserDto>> GetUsersAsync(int page);
    }
}
