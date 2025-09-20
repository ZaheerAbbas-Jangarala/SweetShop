using Xunit;
using Moq;
using SweetShop.Mvc.Controllers;
using SweetShop.Mvc.Models;
using SweetShop.Mvc.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SweetShop.Tests
{
    public class MvcSweetsControllerTests
    {
        private Mock<SweetService> GetSweetServiceMock()
        {
            var mock = new Mock<SweetService>(null); // db context null because methods are mocked
            return mock;
        }

        private IWebHostEnvironment GetEnvMock()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(System.IO.Path.GetTempPath());
            return envMock.Object;
        }

        [Fact]
        public async Task Shop_ShouldReturnAllSweets()
        {
            var mockService = GetSweetServiceMock();
            mockService.Setup(s => s.GetAllSweetsAsync())
                .ReturnsAsync(new List<Sweet>
                {
                    new Sweet { Id = 1, Name = "Gulab Jamun" },
                    new Sweet { Id = 2, Name = "Ladoo" }
                });

            var controller = new SweetsController(mockService.Object, GetEnvMock());
            var result = await controller.Shop(null, null, null, null) as ViewResult;
            var sweets = result.Model as List<Sweet>;

            Assert.Equal(2, sweets.Count);
        }

        [Fact]
        public async Task Create_Post_ShouldCallAddSweetAsync()
        {
            var mockService = GetSweetServiceMock();
            mockService.Setup(s => s.AddSweetAsync(It.IsAny<Sweet>())).ReturnsAsync(true);

            var controller = new SweetsController(mockService.Object, GetEnvMock());
            var sweet = new Sweet { Name = "Barfi", Price = 40 };

            var result = await controller.Create(sweet) as RedirectToActionResult;

            Assert.Equal("AdminIndex", result.ActionName);
            mockService.Verify(s => s.AddSweetAsync(It.Is<Sweet>(sw => sw.Name == "Barfi")), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ShouldCallUpdateSweetAsync()
        {
            var mockService = GetSweetServiceMock();
            mockService.Setup(s => s.UpdateSweetAsync(It.IsAny<Sweet>())).ReturnsAsync(true);

            var controller = new SweetsController(mockService.Object, GetEnvMock());
            var sweet = new Sweet { Id = 1, Name = "Ladoo Special" };

            var result = await controller.Edit(sweet) as RedirectToActionResult;

            Assert.Equal("AdminIndex", result.ActionName);
            mockService.Verify(s => s.UpdateSweetAsync(It.Is<Sweet>(sw => sw.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_ShouldCallDeleteSweetAsync()
        {
            var mockService = GetSweetServiceMock();
            mockService.Setup(s => s.DeleteSweetAsync(It.IsAny<int>())).ReturnsAsync(true);

            var controller = new SweetsController(mockService.Object, GetEnvMock());
            var result = await controller.DeleteConfirmed(1) as RedirectToActionResult;

            Assert.Equal("AdminIndex", result.ActionName);
            mockService.Verify(s => s.DeleteSweetAsync(1), Times.Once);
        }
    }
}
