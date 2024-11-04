using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using BookStoreAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Fixtures;

namespace UnitTest.Systems.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IAuthRepository> _mockAuthRepository;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _mockUserManager = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockAuthRepository = new Mock<IAuthRepository>();

            _service = new AuthService(
                _mockUserManager.Object,
                _mockConfiguration.Object,
                _mockAuthRepository.Object);
        }

        [Fact]
        public async Task LoginUser_WithSuccessfullLogin_ReturnsTrue()
        {
            //Arrange
            var appUser = RefreshFixtures.GetAppUser();
            var loginData = LoginFixtures.GetLoginData();

            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(appUser, It.IsAny<string>()))
                .ReturnsAsync(true);
            //Act
            var result = await _service.LoginUser(loginData);

            //Assert
            Assert.True(result);

            _mockUserManager.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Once);
            _mockUserManager.Verify(u => u.CheckPasswordAsync(appUser, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoginUser_WithFailedLogin_ReturnsFalse()
        {
            //Arrange
            var appUser = RefreshFixtures.GetAppUser();
            var loginData = LoginFixtures.GetLoginData();

            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(appUser, It.IsAny<string>()))
                .ReturnsAsync(false);
            //Act
            var result = await _service.LoginUser(loginData);

            //Assert
            Assert.False(result);

            _mockUserManager.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Once);
            _mockUserManager.Verify(u => u.CheckPasswordAsync(appUser, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoginUser_WithUserNotFound_ReturnsFalse()
        {
            //Arrange
            AppUser? appUser = null;
            var loginData = LoginFixtures.GetLoginData();

            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            //Act
            var result = await _service.LoginUser(loginData);

            //Assert
            Assert.False(result);

            _mockUserManager.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUser_OnSuccessfullRegister_ReturnsTrue()
        {
            //Arrange

            //Act

            //Assert
        }
    }
}
