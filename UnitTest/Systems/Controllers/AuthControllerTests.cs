using BookStoreAPI.Controllers;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using BookStoreAPI.Repositories;
using BookStoreAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        private void SetupHttpContext(string username)
        {
            var principal = RefreshFixtures.GetPrincipal();

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
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

            _mockAuthService.Verify(a => a.LoginUser(_loginData), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.GenerateJwtToken(_loginData.Email, role), Times.Once);
            _mockAuthService.Verify(a => a.CheckRefreshToken(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.GetAuthDataFromUser(_loginData.Email, accessToken), Times.Once);
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

            _mockAuthService.Verify(a => a.LoginUser(_loginData), Times.Once);
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

            _mockAuthService.Verify(a => a.LoginUser(_loginData), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(_loginData.Email), Times.Once);
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

            _mockAuthService.Verify(a => a.LoginUser(_loginData), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.GenerateJwtToken(_loginData.Email, role), Times.Once);
            _mockAuthService.Verify(a => a.CheckRefreshToken(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.AddTokenToUser(_loginData.Email, It.IsAny<string>()), Times.Once);
            _mockAuthService.Verify(a => a.GetAuthDataFromUser(_loginData.Email, accessToken), Times.Once);
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

            _mockAuthService.Verify(a => a.LoginUser(_loginData), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.GenerateJwtToken(_loginData.Email, role), Times.Once);
            _mockAuthService.Verify(a => a.CheckRefreshToken(_loginData.Email), Times.Once);
            _mockAuthService.Verify(a => a.AddTokenToUser(_loginData.Email, It.IsAny<string>()), Times.Once);
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

            _mockAuthService.Verify(a => a.RegisterUser(registerData), Times.Once);
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

            _mockAuthService.Verify(a => a.RegisterUser(registerData), Times.Once);
        }

        //Refresh
        [Fact]
        public async Task Refresh_WithValidCredentials_ReturnsOkObjectResult()
        {
            //Arrange
            var principal = RefreshFixtures.GetPrincipal();
            var refreshModel = RefreshFixtures.GetRefreshModel();
            var appUser = RefreshFixtures.GetAppUser();
            var role = "role";
            var accessTokenDto = LoginFixtures.GetAccessTokenDto();

            _mockAuthService.Setup(a => a.GetClaimsPrincipal(refreshModel.AccessToken))
                .Returns(principal);
            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(It.IsAny<string>()))
                .ReturnsAsync(role);
            _mockAuthService.Setup(a => a.GenerateJwtToken(It.IsAny<string>(), role))
                .Returns(accessTokenDto);
            //Act
            var result = await _controller.Refresh(refreshModel);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<LoginResponseDto>(okResult.Value);

            Assert.NotNull(actualResponse);

            _mockAuthService.Verify(a => a.GetClaimsPrincipal(refreshModel.AccessToken), Times.Once);
            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(It.IsAny<string>()), Times.Once);
            _mockAuthService.Verify(a => a.GenerateJwtToken(It.IsAny<string>(), role), Times.Once);
        }

        [Fact]
        public async Task Refresh_WithInvalidPrincipals_ReturnsUnauthorizedResult()
        {
            //Arrange
            var invalidPrincipals = new ClaimsPrincipal();
            var refreshModel = RefreshFixtures.GetRefreshModel();

            _mockAuthService.Setup(a => a.GetClaimsPrincipal(refreshModel.AccessToken))
                .Returns(invalidPrincipals);
            //Act
            var result = await _controller.Refresh(refreshModel);

            //Assert
            Assert.IsType<UnauthorizedResult>(result);

            _mockAuthService.Verify(a => a.GetClaimsPrincipal(refreshModel.AccessToken), Times.Once);
        }

        [Fact]
        public async Task Refresh_WithUserIsNull_ReturnsUnauthorizedResult()
        {
            //Arrange
            var principal = RefreshFixtures.GetPrincipal();
            var refreshModel = RefreshFixtures.GetRefreshModel();
            AppUser? user = null;

            _mockAuthService.Setup(a => a.GetClaimsPrincipal(refreshModel.AccessToken))
                .Returns(principal);
            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(user);
            //Act
            var result = await _controller.Refresh(refreshModel);

            //Assert
            Assert.IsType<UnauthorizedResult>(result);

            _mockAuthService.Verify(a => a.GetClaimsPrincipal(refreshModel.AccessToken), Times.Once);
            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Refresh_WithRoleNotFound_ReturnsNotFoundObjectResult()
        {
            //Arrange
            var principal = RefreshFixtures.GetPrincipal();
            var refreshModel = RefreshFixtures.GetRefreshModel();
            var appUser = RefreshFixtures.GetAppUser();
            string? role = null;

            _mockAuthService.Setup(a => a.GetClaimsPrincipal(refreshModel.AccessToken))
                .Returns(principal);
            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockRoleService.Setup(r => r.GetUserRoleAsync(It.IsAny<string>()))
                .ReturnsAsync(role);
            //Act
            var result = await _controller.Refresh(refreshModel);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _mockAuthService.Verify(a => a.GetClaimsPrincipal(refreshModel.AccessToken), Times.Once);
            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
            _mockRoleService.Verify(r => r.GetUserRoleAsync(It.IsAny<string>()), Times.Once);
        }

        //Revoke
        [Fact]
        public async Task Revoke_WithValidCredentials_ReturnOkResult()
        {
            //Arrange
            string username = "testname";
            SetupHttpContext(username);
            var appUser = RefreshFixtures.GetAppUser();

            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockAuthRepository.Setup(a => a.RevokeToken(appUser))
                .ReturnsAsync(true);
            //Act
            var result = await _controller.Revoke();

            //Assert
            Assert.IsType<OkResult>(result);

            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
            _mockAuthRepository.Verify(a => a.RevokeToken(appUser), Times.Once);
        }

        [Fact]
        public async Task Revoke_WithUsernameNull_ReturnsUnauthorizedResult()
        {
            //Arrange
            string? username = null;
            SetupHttpContext(username);

            //Act
            var result = await _controller.Revoke();

            //Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Revoke_WithUserIsNull_ReturnsUnauthorizedResult()
        {
            string username = "testname";
            SetupHttpContext(username);
            AppUser? appUser = null;

            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            //Act
            var result = await _controller.Revoke();

            //Assert
            Assert.IsType<UnauthorizedResult>(result);

            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Revoke_WithFailedRevokeToken_ReturnsBadRequestObjectResult()
        {
            string username = "testname";
            SetupHttpContext(username);
            var appUser = RefreshFixtures.GetAppUser();

            _mockAuthService.Setup(a => a.GetUser(It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _mockAuthRepository.Setup(a => a.RevokeToken(appUser))
                .ReturnsAsync(false);
            //Act
            var result = await _controller.Revoke();

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);

            _mockAuthService.Verify(a => a.GetUser(It.IsAny<string>()), Times.Once);
            _mockAuthRepository.Verify(a => a.RevokeToken(appUser), Times.Once);
        }
    }
}