using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using ReqresIntegration.Core.Models;
using ReqresIntegration.Infrastructure.ApiClients;
using ReqresIntegration.Infrastructure.Configuration;
using ReqresIntegration.Infrastructure.Exceptions;
using ReqresIntegration.Infrastructure.Interfaces;
using System.Net;
using System.Text.Json;

namespace ReqresIntegration.Tests
{
    public class ReqresApiClientTests
    {
        private IReqresApiClient CreateClient(HttpResponseMessage response, string baseUrl)
        {
            // Mock HttpMessageHandler
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(baseUrl)
            };

            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.CreateClient("ReqresClient")).Returns(httpClient);

            var options = Options.Create(new ReqresApiOptions
            {
                BaseUrl = baseUrl
            });

            return new ReqresApiClient(factoryMock.Object, options);
        }

        [Fact]
        public async Task GetUserAsync_ReturnsUser()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var user = new SingleUserResponse
            {
                Data = new UserDto
                {
                    Id = 1,
                    First_Name = "George",
                    Last_Name = "Bluth",
                    Email = "george.bluth@reqres.in"
                }
            };

            var json = JsonSerializer.Serialize(user);
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };

            var client = CreateClient(response, baseUrl);

            // Act
            var result = await client.GetUserAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("George", result.First_Name);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsPaginatedList()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var users = new PaginatedUserResponse
            {
                Data = new List<UserDto>
                {
                    new UserDto { Id = 1, First_Name = "George" },
                    new UserDto { Id = 2, First_Name = "Janet" }
                }
            };

            var json = JsonSerializer.Serialize(users);
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };

            var client = CreateClient(response, baseUrl);

            // Act
            var result = await client.GetUsersAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsNoUsersList()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var users = new PaginatedUserResponse
            {
                Data = null
            };

            var json = JsonSerializer.Serialize(users);
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };

            var client = CreateClient(response, baseUrl);

            // Act
            var result = await client.GetUsersAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsNullUsersList()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            PaginatedUserResponse users = null;

            var json = JsonSerializer.Serialize(users);
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };

            var client = CreateClient(response, baseUrl);

            // Act
            var result = await client.GetUsersAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserAsync_WhenNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            var client = CreateClient(response, baseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => client.GetUserAsync(99));
        }

        [Fact]
        public async Task GetUserAsync_WhenHttpFails_ThrowsHttpRequestException()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            var client = CreateClient(response, baseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetUserAsync(2));
        }

        [Fact]
        public async Task GetUsersAsync_WhenHttpFails_ThrowsHttpRequestException()
        {
            // Arrange
            var baseUrl = "https://reqres.in/api/";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            var client = CreateClient(response, baseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetUsersAsync(2));
        }
    }
}
