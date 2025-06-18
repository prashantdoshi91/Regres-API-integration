using Microsoft.Extensions.Caching.Memory;
using Moq;
using ReqresIntegration.Core.Interfaces;
using ReqresIntegration.Core.Models;
using ReqresIntegration.Infrastructure.Services;

namespace ReqresIntegration.Tests
{
    public class CachedExternalUserServiceTests
    {
        private readonly MemoryCache _memoryCache;
        private readonly Mock<IExternalUserService> _mockBaseService;

        public CachedExternalUserServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockBaseService = new Mock<IExternalUserService>();
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsFromCache_IfExists()
        {
            // Arrange
            var expectedUser = new UserDto { Id = 1, First_Name = "George", Last_Name = "Bluth", Email = "george.bluth@reqres.in" };
            string cacheKey = $"user_1";

            _memoryCache.Set(cacheKey, expectedUser, TimeSpan.FromMinutes(5));

            var service = new CachedExternalUserService(_mockBaseService.Object, _memoryCache);

            // Act
            var result = await service.GetUserByIdAsync(1);

            // Assert
            Assert.Equal(expectedUser.Id, result.Id);
            _mockBaseService.Verify(s => s.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_CallsBaseService_AndCaches_WhenNotInCache()
        {
            // Arrange
            var expectedUser = new UserDto { Id = 2, First_Name = "Janet", Last_Name = "Weaver", Email = "janet.weaver@reqres.in" };

            _mockBaseService.Setup(s => s.GetUserByIdAsync(2)).ReturnsAsync(expectedUser);

            var service = new CachedExternalUserService(_mockBaseService.Object, _memoryCache);

            // Act
            var result = await service.GetUserByIdAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser.Email, result.Email);
            _mockBaseService.Verify(s => s.GetUserByIdAsync(2), Times.Once);

            // Verify it's now cached
            var cached = _memoryCache.TryGetValue("user_2", out UserDto cachedUser);
            Assert.True(cached);
            Assert.Equal("Janet", cachedUser.First_Name);
        }

        [Fact]
        public async Task GetAllUsersAsync_CallsBaseService_DoesNotUseCache()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, First_Name = "George" },
                new UserDto { Id = 2, First_Name = "Janet" }
            };

            _mockBaseService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            var service = new CachedExternalUserService(_mockBaseService.Object, _memoryCache);

            // Act
            var result = await service.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, (result as List<UserDto>).Count);
            _mockBaseService.Verify(s => s.GetAllUsersAsync(), Times.Once);
        }
    }
}
