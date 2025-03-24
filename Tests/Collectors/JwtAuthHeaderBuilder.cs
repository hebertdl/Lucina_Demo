using System.Net;
using System.Text.Json;
using Core;
using Core.Interfaces;
using Moq;
using Moq.Protected;

namespace Lucina_Demo_Tests.Collectors;

public class JwtAuthHeaderBuilderTests
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthHeaderBuilder _jwtAuthHeaderBuilder;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger> _mockLogger;

    public JwtAuthHeaderBuilderTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockLogger = new Mock<ILogger>();
        _jwtAuthHeaderBuilder = new JwtAuthHeaderBuilder(
            _httpClient,
            _mockLogger.Object,
            "test-client-id",
            "test-client-secret",
            "https://example.com/test_token"
        );
    }

    [Fact]
    public async Task BuildAuthHeaderAsync_Success_ReturnsAuthHeader()
    {
        // Arrange
        var tokenResponse = new Dictionary<string, string> { { "access_token", "test-token" } };
        var jsonResponse = JsonSerializer.Serialize(tokenResponse);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var authHeader = await _jwtAuthHeaderBuilder.BuildAuthHeaderAsync();

        // Assert
        Assert.NotNull(authHeader);
        Assert.Equal("Bearer", authHeader.Scheme);
        Assert.Equal("test-token", authHeader.Parameter);
        _mockLogger.Verify(l => l.LogInfo("JwtAuthHeaderBuilder", "Building JWT auth header"), Times.Once());
        _mockLogger.Verify(l => l.LogInfo("JwtAuthHeaderBuilder", "JWT token retrieved successfully"), Times.Once());
    }

    [Fact]
    public async Task BuildAuthHeaderAsync_Failure_ThrowsException()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Bad Request")
            });

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<HttpRequestException>(() => _jwtAuthHeaderBuilder.BuildAuthHeaderAsync());
        Assert.Equal("Response status code does not indicate success: 400 (Bad Request).", exception.Message);
        _mockLogger.Verify(l => l.LogInfo("JwtAuthHeaderBuilder", "Building JWT auth header"), Times.Once());
        _mockLogger.Verify(l => l.LogError("JwtAuthHeaderBuilder", It.IsAny<string>(), It.IsAny<Exception>()),
            Times.Once());
    }

    [Fact]
    public async Task BuildAuthHeaderAsync_HttpRequestException_ThrowsException()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<HttpRequestException>(() => _jwtAuthHeaderBuilder.BuildAuthHeaderAsync());
        Assert.Equal("Network error", exception.Message);
        _mockLogger.Verify(l => l.LogInfo("JwtAuthHeaderBuilder", "Building JWT auth header"), Times.Once());
        _mockLogger.Verify(l => l.LogError("JwtAuthHeaderBuilder", It.IsAny<string>(), It.IsAny<Exception>()),
            Times.Once());
    }
}