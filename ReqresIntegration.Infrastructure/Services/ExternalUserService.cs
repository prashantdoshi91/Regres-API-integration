using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Core.Models;
using ReqresIntegration.Infrastructure.Interfaces;

namespace ReqresIntegration.Infrastructure.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IReqresApiClient _client;

        public ExternalUserService(IReqresApiClient client)
        {
            _client = client;
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            return await _client.GetUserAsync(userId);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var allUsers = new List<UserDto>();
            int page = 1;
            List<UserDto> users;
            do
            {
                users = await _client.GetUsersAsync(page++);
                allUsers.AddRange(users);
            } while (users.Any());

            return allUsers;
        }
    }
}
