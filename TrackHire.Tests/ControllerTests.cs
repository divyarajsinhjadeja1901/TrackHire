using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrackHire.API.Controllers;
using TrackHire.API.DTOs;
using TrackHire.API.Services;
using Xunit;

namespace TrackHire.Tests;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _service = new();

    private ApplicationsController CreateController(int userId = 1)
    {
        var controller = new ApplicationsController(_service.Object);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    [Fact]
    public async Task GetById_Returns404_WhenServiceReturnsNull()
    {
        _service.Setup(s => s.GetByIdAsync(9, 1)).ReturnsAsync((JobApplicationDto?)null);

        var result = await CreateController().GetById(9);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_Returns200_WithDto_WhenFound()
    {
        _service.Setup(s => s.GetByIdAsync(9, 1)).ReturnsAsync(new JobApplicationDto { Id = 9 });

        var result = await CreateController().GetById(9);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(9, ((JobApplicationDto)ok.Value!).Id);
    }

    [Fact]
    public async Task Create_Returns201_AndUsesUserIdFromJwtClaim()
    {
        _service.Setup(s => s.CreateAsync(77, It.IsAny<CreateApplicationDto>()))
                .ReturnsAsync(new JobApplicationDto { Id = 3 });

        var result = await CreateController(userId: 77).Create(new CreateApplicationDto { CompanyName = "Acme" });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        _service.Verify(s => s.CreateAsync(77, It.IsAny<CreateApplicationDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_Returns204_WhenUpdated_And404_WhenNot()
    {
        _service.Setup(s => s.UpdateStatusAsync(1, 1, "Offer")).ReturnsAsync(true);
        _service.Setup(s => s.UpdateStatusAsync(2, 1, "Offer")).ReturnsAsync(false);
        var controller = CreateController();

        Assert.IsType<NoContentResult>(await controller.UpdateStatus(1, "Offer"));
        Assert.IsType<NotFoundResult>(await controller.UpdateStatus(2, "Offer"));
    }

    [Fact]
    public async Task Delete_Returns404_WhenNothingDeleted()
    {
        _service.Setup(s => s.DeleteAsync(5, 1)).ReturnsAsync(false);

        Assert.IsType<NotFoundResult>(await CreateController().Delete(5));
    }
}

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _auth = new();

    [Fact]
    public async Task Register_Returns400_WhenEmailAlreadyExists()
    {
        _auth.Setup(a => a.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync((string?)null);

        var result = await new AuthController(_auth.Object).Register(new RegisterDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_Returns200_WhenRegistrationSucceeds()
    {
        _auth.Setup(a => a.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync("token");

        var result = await new AuthController(_auth.Object).Register(new RegisterDto());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_Returns401_ForInvalidCredentials()
    {
        _auth.Setup(a => a.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((string?)null);

        var result = await new AuthController(_auth.Object).Login(new LoginDto());

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_Returns200_ForValidCredentials()
    {
        _auth.Setup(a => a.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync("token");

        var result = await new AuthController(_auth.Object).Login(new LoginDto());

        Assert.IsType<OkObjectResult>(result);
    }
}
