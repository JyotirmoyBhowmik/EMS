using Xunit;
using FluentAssertions;
using Moq;
using ScadaCore.Models;
using ScadaCore.Services;
using ScadaCore.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ScadaCore.Tests;

public class TagManagementServiceTests
{
    private readonly ScadaDbContext _context;
    private readonly Mock<TagCacheService> _mockCacheService;
    private readonly Mock<ILogger<TagManagementService>> _mockLogger;
    private readonly TagManagementService _service;

    public TagManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<ScadaDbContext>()
            .UseInMemoryDatabase(databaseName: "TestScadaDb")
            .Options;

        _context = new ScadaDbContext(options);
        _mockCacheService = new Mock<TagCacheService>(MockBehavior.Loose, null!);
        _mockLogger = new Mock<ILogger<TagManagementService>>();
        
        _service = new TagManagementService(_context, _mockCacheService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTag_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var tag = new Tag
        {
            Name = "TEST.TAG.001",
            Description = "Test Tag",
            DataType = "Float",
            Units = "°C",
            ScanRate = 1000
        };

        // Act
        var result = await _service.CreateTagAsync(tag);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("TEST.TAG.001");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateTag_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var tag1 = new Tag { Name = "DUPLICATE.TAG", DataType = "Float" };
        var tag2 = new Tag { Name = "DUPLICATE.TAG", DataType = "Float" };

        await _service.CreateTagAsync(tag1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateTagAsync(tag2));
    }

    [Fact]
    public async Task GetTagByName_WithExistingTag_ShouldReturnTag()
    {
        // Arrange
        var tag = new Tag { Name = "EXISTING.TAG", DataType = "Float" };
        await _service.CreateTagAsync(tag);

        // Act
        var result = await _service.GetTagByNameAsync("EXISTING.TAG");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("EXISTING.TAG");
    }

    [Fact]
    public async Task GetTags_WithSiteFilter_ShouldReturnFilteredTags()
    {
        // Arrange
        await _service.CreateTagAsync(new Tag { Name = "SITE01.TAG1", DataType = "Float", Site = "SITE01" });
        await _service.CreateTagAsync(new Tag { Name = "SITE02.TAG1", DataType = "Float", Site = "SITE02" });
        await _service.CreateTagAsync(new Tag { Name = "SITE01.TAG2", DataType = "Float", Site = "SITE01" });

        // Act
        var result = await _service.GetTagsAsync(site: "SITE01");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.Site.Should().Be("SITE01"));
    }

    [Fact]
    public async Task UpdateTag_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var tag = await _service.CreateTagAsync(new Tag { Name = "UPDATE.TAG", DataType = "Float", Units = "m/s" });
        var updatedTag = new Tag { Description = "Updated Description", DataType = "Float", Units = "km/h" };

        // Act
        var result = await _service.UpdateTagAsync(tag.Id, updatedTag);

        // Assert
        result.Description.Should().Be("Updated Description");
        result.Units.Should().Be("km/h");
        result.UpdatedAt.Should().BeAfter(result.CreatedAt);
    }

    [Fact]
    public async Task DeleteTag_WithExistingTag_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tag = await _service.CreateTagAsync(new Tag { Name = "DELETE.TAG", DataType = "Float" });

        // Act
        await _service.DeleteTagAsync(tag.Id);
        var result = await _service.GetTagByIdAsync(tag.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchTags_WithPattern_ShouldReturnMatchingTags()
    {
        // Arrange
        await _service.CreateTagAsync(new Tag { Name = "SOLAR.PANEL.VOLTAGE", DataType = "Float" });
        await _service.CreateTagAsync(new Tag { Name = "SOLAR.PANEL.CURRENT", DataType = "Float" });
        await _service.CreateTagAsync(new Tag { Name = "WIND.TURBINE.POWER", DataType = "Float" });

        // Act
        var result = await _service.SearchTagsAsync("SOLAR");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.Name.Should().Contain("SOLAR"));
    }
}
