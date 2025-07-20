using Moq;
using Microsoft.AspNetCore.Mvc;
using OrdersApi.Controllers;
using OrdersApi.Models.DTOs;
using OrdersApi.Services;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace OrdersApi.OrdersApi.Tests
{
    [TestFixture]
    public class OrdersControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private Mock<ILogger<OrdersController>> _mockLogger;
        private OrdersController _controller;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);

            // Setup HttpContext for Controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Test]
        public async Task CreateOrder_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var expectedResponse = new CreateOrderResponse
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "John Doe",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemResponse>
                {
                    new() { Id = 1, ProductId = request.Items[0].ProductId, Quantity = 2 },
                    new() { Id = 2, ProductId = request.Items[1].ProductId, Quantity = 1 }
                }
            };

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.AreEqual(StatusCodes.Status201Created, createdResult!.StatusCode);
            Assert.AreEqual(expectedResponse.OrderId, ((CreateOrderResponse)createdResult.Value!).OrderId);
        }

        [Test]
        public async Task CreateOrder_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "",
                Items = new List<CreateOrderItemRequest>()
            };

            _controller.ModelState.AddModelError("CustomerName", "Customer name is required");

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult!.StatusCode);
        }

        [Test]
        public async Task CreateOrder_ServiceThrowsValidationException_ReturnsBadRequestWithProblemDetails()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 0 }
                }
            };

            var validationException = new ValidationException("Quantity must be greater than zero");
            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(validationException);

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult!.StatusCode);

            var problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.NotNull(problemDetails);
            Assert.AreEqual("Data validation failed", problemDetails!.Title);
            Assert.AreEqual("Quantity must be greater than zero", problemDetails.Detail);
        }

        [Test]
        public async Task CreateOrder_ServiceThrowsGenericException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var exception = new Exception("Database connection failed");
            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CreateOrder(request);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult!.StatusCode);

            var problemDetails = objectResult.Value as ProblemDetails;
            Assert.NotNull(problemDetails);
            Assert.AreEqual("Internal server error", problemDetails!.Title);
        }

        [Test]
        public async Task CreateOrder_CallsOrderService()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var expectedResponse = new CreateOrderResponse
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "John Doe",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItemResponse>()
            };

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            await _controller.CreateOrder(request);

            // Assert
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>()), Times.Once);
        }
    }
}