using Moq;
using OrdersApi.Services;
using OrdersApi.Models.DTOs;
using OrdersApi.Models;
using OrdersApi.Repositories;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace OrdersApi.OrdersApi.Tests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<IOrderRepository> _mockOrderRepository = null!;
        private Mock<ILogger<OrderService>> _mockLogger = null!;
        private OrderService _orderService = null!;

        [SetUp]
        public void Setup()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            _orderService = new OrderService(_mockOrderRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task CreateOrderAsync_ValidRequest_ReturnsCorrectResponse()
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

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "John Doe",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, ProductId = request.Items[0].ProductId, Quantity = 2 },
                    new() { Id = 2, ProductId = request.Items[1].ProductId, Quantity = 1 }
                }
            };

            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdOrder.OrderId, result.OrderId);
            Assert.AreEqual(createdOrder.CustomerName, result.CustomerName);
            Assert.AreEqual(createdOrder.CreatedAt, result.CreatedAt);
            Assert.AreEqual(2, result.Items.Count);
            Assert.AreEqual(createdOrder.Items.First().Id, result.Items.First().Id);
            Assert.AreEqual(createdOrder.Items.First().ProductId, result.Items.First().ProductId);
            Assert.AreEqual(createdOrder.Items.First().Quantity, result.Items.First().Quantity);
        }

        [Test]
        public async Task CreateOrderAsync_ValidRequest_CallsRepositoryWithCorrectOrder()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Jane Smith",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 3 }
                }
            };

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Jane Smith",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, ProductId = request.Items[0].ProductId, Quantity = 3 }
                }
            };

            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            // Act
            await _orderService.CreateOrderAsync(request);

            // Assert
            _mockOrderRepository.Verify(r => r.CreateOrderAsync(It.Is<Order>(o =>
                o.CustomerName == request.CustomerName &&
                o.Items.Count == request.Items.Count &&
                o.Items.First().ProductId == request.Items.First().ProductId &&
                o.Items.First().Quantity == request.Items.First().Quantity
            )), Times.Once);
        }

        [Test]
        public async Task CreateOrderAsync_RepositoryThrowsException_LogsErrorAndRethrows()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Error Customer",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var exception = new Exception("Database connection failed");
            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ThrowsAsync(exception);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(request));
            
            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to create order")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task CreateOrderAsync_ValidRequest_LogsStartAndSuccess()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Log Test Customer",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Log Test Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, ProductId = request.Items[0].ProductId, Quantity = 1 }
                }
            };

            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            // Act
            await _orderService.CreateOrderAsync(request);

            // Assert - Verify start logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting to create order")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Assert - Verify success logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Order created successfully")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task CreateOrderAsync_MultipleItems_MapsAllItemsCorrectly()
        {
            // Arrange
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();

            var request = new CreateOrderRequest
            {
                CustomerName = "Multi Item Customer",
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = productId1, Quantity = 1 },
                    new() { ProductId = productId2, Quantity = 5 },
                    new() { ProductId = productId3, Quantity = 3 }
                }
            };

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Multi Item Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { Id = 1, ProductId = productId1, Quantity = 1 },
                    new() { Id = 2, ProductId = productId2, Quantity = 5 },
                    new() { Id = 3, ProductId = productId3, Quantity = 3 }
                }
            };

            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.AreEqual(3, result.Items.Count);
            
            var resultItem1 = result.Items.FirstOrDefault(i => i.ProductId == productId1);
            var resultItem2 = result.Items.FirstOrDefault(i => i.ProductId == productId2);
            var resultItem3 = result.Items.FirstOrDefault(i => i.ProductId == productId3);

            Assert.IsNotNull(resultItem1);
            Assert.AreEqual(1, resultItem1!.Quantity);
            
            Assert.IsNotNull(resultItem2);
            Assert.AreEqual(5, resultItem2!.Quantity);
            
            Assert.IsNotNull(resultItem3);
            Assert.AreEqual(3, resultItem3!.Quantity);
        }

        [Test]
        public async Task CreateOrderAsync_EmptyItemsList_HandlesCorrectly()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "Empty Items Customer",
                Items = new List<CreateOrderItemRequest>()
            };

            var createdOrder = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerName = "Empty Items Customer",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            _mockOrderRepository
                .Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Items.Count);
            Assert.AreEqual("Empty Items Customer", result.CustomerName);
        }
    }
} 