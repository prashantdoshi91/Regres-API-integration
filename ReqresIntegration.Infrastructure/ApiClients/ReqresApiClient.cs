using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReqresIntegration.Core.Models;
using ReqresIntegration.Infrastructure.Configuration;
using ReqresIntegration.Infrastructure.Exceptions;
using ReqresIntegration.Infrastructure.Interfaces;
using System.Net;

namespace ReqresIntegration.Infrastructure.ApiClients
{
    public class ReqresApiClient : IReqresApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReqresApiOptions _options;
        private readonly HttpClient _client;

        public ReqresApiClient(IHttpClientFactory httpClientFactory, IOptions<ReqresApiOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;

            _client = _httpClientFactory.CreateClient("ReqresClient");
            _client.BaseAddress = new Uri(_options.BaseUrl);
            _client.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        public async Task<UserDto> GetUserAsync(int userId)
        {
            var response = await _client.GetAsync($"users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new NotFoundException($"User with ID {userId} was not found.");

                throw new HttpRequestException($"Failed to fetch user. Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<SingleUserResponse>(content);
            return user.Data;
        }

        public async Task<List<UserDto>> GetUsersAsync(int page)
        {
            var response = await _client.GetAsync($"users?page={page}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch users on page {page}");

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<PaginatedUserResponse>(content);
            return result?.Data ?? [];
        }
    }
}
