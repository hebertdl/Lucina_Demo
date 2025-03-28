﻿using System.Net;
using System.Net.Http.Headers;
using Collectors;
using Core.Interfaces;
using Moq;
using Moq.Protected;

namespace Lucina_Demo_Tests.Collectors;

public class BatchPusherTests
{
    private readonly BatchPusher _batchPusher;
    private readonly Mock<IAuthHeaderBuilder> _mockAuthHeaderBuilder;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger> _mockLogger;

    public BatchPusherTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockAuthHeaderBuilder = new Mock<IAuthHeaderBuilder>();
        _mockLogger = new Mock<ILogger>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _batchPusher = new BatchPusher(httpClient, _mockAuthHeaderBuilder.Object);
    }

    [Fact]
    public async Task PostResults_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string postUrl = "https://reqres.in/api/users";
        const string jsonData = "{\"name\": \"John Doe\", \"job\": \"Developer\"}";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _batchPusher.PostResults(postUrl, jsonData, null));
        Assert.Equal("logger", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    public async Task PostResults_NullOrEmptyJsonData_LogsErrorAndThrowsArgumentNullException(string jsonData)
    {
        // Arrange
        var postUrl = "https://reqres.in/api/users";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _batchPusher.PostResults(postUrl, jsonData, _mockLogger.Object));
        Assert.Equal("jsonData", exception.ParamName);
        _mockLogger.Verify(l => l.LogError("BatchPusher", "JSON data is null or empty",
            It.IsAny<ArgumentNullException>()), Times.Once());
    }

    [Fact]
    public async Task PostResults_SuccessfulPost_LogsSuccess()
    {
        // Arrange
        var postUrl = "https://reqres.in/api/users";
        var jsonData = "{\"name\": \"John Doe\", \"job\": \"Developer\"}";

        _mockAuthHeaderBuilder.Setup(a => a.BuildAuthHeaderAsync())
            .ReturnsAsync(new AuthenticationHeaderValue("Bearer", "test-token"));

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("{\"id\": \"123\", \"name\": \"John Doe\", \"job\": \"Developer\"}")
            });

        // Act
        await _batchPusher.PostResults(postUrl, jsonData, _mockLogger.Object);

        // Assert
        _mockLogger.Verify(l => l.LogInfo("BatchPusher", $"Streaming data to {postUrl}"), Times.Once());
        _mockLogger.Verify(l => l.LogInfo(
                "BatchPusher",
                It.Is<string>(s => s.Contains("Post successful") && s.Contains("Response Body Length:"))),
            Times.Once());
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()),
            Times.Never());
    }

    [Fact]
    public async Task PostResults_FailedPost_LogsErrorAndThrows()
    {
        // Arrange
        var postUrl = "https://reqres.in/api/users";
        var jsonData = "{\"name\": \"John Doe\", \"job\": \"Developer\"}";

        _mockAuthHeaderBuilder.Setup(a => a.BuildAuthHeaderAsync())
            .ReturnsAsync(new AuthenticationHeaderValue("Bearer", "test-token"));

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _batchPusher.PostResults(postUrl, jsonData, _mockLogger.Object));
        _mockLogger.Verify(l => l.LogInfo("BatchPusher", $"Streaming data to {postUrl}"), Times.Once());
        _mockLogger.Verify(l => l.LogError("BatchPusher",
            It.Is<string>(s => s.Contains("Failed to post results") && s.Contains(postUrl)),
            It.IsAny<HttpRequestException>()), Times.Once());
    }
}