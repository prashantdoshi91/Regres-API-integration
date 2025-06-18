using Moq;
using ReqresIntegration.Core.Models;
using ReqresIntegration.Infrastructure.Interfaces;
using ReqresIntegration.Infrastructure.Services;

namespace ReqresIntegration.Tests
{
    public class ExternalUserServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            // Arrange
            var mockClient = new Mock<IReqresApiClient>();
            var expectedUser = new UserDto { Id = 1, First_Name = "John", Last_Name = "Doe", Email = "john@doe.com" };

            mockClient.Setup(c => c.GetUserAsync(1)).ReturnsAsync(expectedUser);

            var service = new ExternalUserService(mockClient.Object);

            // Act
            var user = await service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(expectedUser.Id, user.Id);
            Assert.Equal(expectedUser.Email, user.Email);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsMultiplePages()
        {
            // Arrange
            var mockClient = new Mock<IReqresApiClient>();

            mockClient.Setup(c => c.GetUsersAsync(1)).ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = 1 },
                new UserDto { Id = 2 }
            });

            mockClient.Setup(c => c.GetUsersAsync(2)).ReturnsAsync(new List<UserDto>
            {
                new UserDto { Id = 3 }
            });

            mockClient.Setup(c => c.GetUsersAsync(3)).ReturnsAsync(new List<UserDto>()); // No more users

            var service = new ExternalUserService(mockClient.Object);

            // Act
            var allUsers = (await service.GetAllUsersAsync()).ToList();

            // Assert
            Assert.Equal(3, allUsers.Count);
            Assert.Contains(allUsers, u => u.Id == 1);
            Assert.Contains(allUsers, u => u.Id == 3);
        }
    }
}
