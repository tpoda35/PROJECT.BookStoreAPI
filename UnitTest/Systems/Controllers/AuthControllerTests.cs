using BookStoreAPI.Controllers;
using BookStoreAPI.Dtos;
using BookStoreAPI.Repositories;
using BookStoreAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Fixtures;

namespace UnitTest.Systems.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly Mock<IAuthRepository> _mockAuthRepository;
        private readonly AuthController _controller;

        private readonly LoginDto _loginData;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockRoleService = new Mock<IRoleService>();
            _mockAuthRepository = new Mock<IAuthRepository>();

            _controller = new AuthController(
                _mockAuthService.Object,
                _mockRoleService.Object,
                _mockAuthRepository.Object);

            _loginData = LoginFixtures.GetLoginData();
        }

        //Login
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkObjectResult()
        {
            //Arrange
            var role = "test";
            var accessToken = LoginFixtures.GetAccessTokenDto();
            var loginResponseDto = LoginFixtures.GetLoginResponsedto();

            _mockAuthService.Setup(a => a.LoginUser(_loginData))
                .ReturnsAsync(true);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(_loginData.Email))
                .ReturnsAsync(role);
            _mockAuthService.Setup(a => a.GenerateJwtToken(_loginData.Email, role))
                .Returns(accessToken);
            _mockAuthService.Setup(a => a.CheckRefreshToken(_loginData.Email))
                .ReturnsAsync(true);
            _mockAuthService.Setup(a => a.GetAuthDataFromUser(_loginData.Email, accessToken))
                .ReturnsAsync(loginResponseDto);
            //Act
            var result = await _controller.Login(_loginData);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.NotNull(actualResponse);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequestResult()
        {
            //Arrange
            _mockAuthService.Setup(a => a.LoginUser(_loginData))
                .ReturnsAsync(false);
            //Act
            var result = await _controller.Login(_loginData);

            //Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Login_WithValidCredentialsButRoleNotFound_ReturnsNotFoundObjectResult()
        {
            //Arrange
            _mockAuthService.Setup(a => a.LoginUser(_loginData))
                .ReturnsAsync(true);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(_loginData.Email))
                .ReturnsAsync((string?)null);
            //Act
            var result = await _controller.Login(_loginData);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithValidCredentialsButNoRefresh_ReturnsOkObjectResult()
        {
            //Arrange
            var role = "test";
            var accessToken = LoginFixtures.GetAccessTokenDto();
            var loginResponseDto = LoginFixtures.GetLoginResponsedto();
            (bool IsSuccess, RefreshTokenResponseDto dto) tokenToUser = (true, new RefreshTokenResponseDto());

            _mockAuthService.Setup(a => a.LoginUser(_loginData))
                .ReturnsAsync(true);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(_loginData.Email))
                .ReturnsAsync(role);
            _mockAuthService.Setup(a => a.GenerateJwtToken(_loginData.Email, role))
                .Returns(accessToken);
            _mockAuthService.Setup(a => a.CheckRefreshToken(_loginData.Email))
                .ReturnsAsync(false);
            _mockAuthService.Setup(a => a.AddTokenToUser(_loginData.Email, It.IsAny<string>()))
                .ReturnsAsync(tokenToUser);
            _mockAuthService.Setup(a => a.GetAuthDataFromUser(_loginData.Email, accessToken))
                .ReturnsAsync(loginResponseDto);
            //Act
            var result = await _controller.Login(_loginData);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.NotNull(actualResponse);
        }

        [Fact]
        public async Task Login_WithValidCredentialsButNoRefreshFalseAddToken_ReturnsBadRequestObjectResult()
        {
            //Arrange
            var role = "test";
            var accessToken = LoginFixtures.GetAccessTokenDto();
            (bool IsSuccess, RefreshTokenResponseDto dto) tokenToUser = (false, new RefreshTokenResponseDto());

            _mockAuthService.Setup(a => a.LoginUser(_loginData))
                .ReturnsAsync(true);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(_loginData.Email))
                .ReturnsAsync(role);
            _mockAuthService.Setup(a => a.GenerateJwtToken(_loginData.Email, role))
                .Returns(accessToken);
            _mockAuthService.Setup(a => a.CheckRefreshToken(_loginData.Email))
                .ReturnsAsync(false);
            _mockAuthService.Setup(a => a.AddTokenToUser(_loginData.Email, It.IsAny<string>()))
                .ReturnsAsync(tokenToUser);
            //Act
            var result = await _controller.Login(_loginData);

            //Assert
            var BadRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var actualResponse = Assert.IsType<string>(BadRequestResult.Value);
        }

        //Register
        [Fact]
        public async Task Register_WithValidCredentials_ReturnsOkObjectResult()
        {
            //Arrange
            var registerData = new RegisterDto();

            _mockAuthService.Setup(a => a.RegisterUser(registerData))
                .ReturnsAsync(true);
            //Act
            var result = await _controller.Register(registerData);

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithInvalidCredentials_ReturnsBadRequestResult()
        {
            //Arrange
            var registerData = new RegisterDto();

            _mockAuthService.Setup(a => a.RegisterUser(registerData))
                .ReturnsAsync(false);
            //Act
            var result = await _controller.Register(registerData);

            //Assert
            Assert.IsType<BadRequestResult>(result);
        }

        //Refresh
        [Fact]
        public async Task Refresh_WithValidCredentials_ReturnsOkObjectResult()
        {
            //Arrange

            //Act

            //Assert
        }
    }
}