using ReqresIntegration.Core.Models;

namespace ReqresIntegration.Core.Interfaces
{
    public interface IExternalUserService
    {
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}
