using System.Text.Json;
using Bredinin.AlloyEditor.Common.Configurations;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;

namespace Bredinin.AlloyEdtior.Handlers.Tests;

public class GetHeatTreatmentTypesHandlerCacheTests
{
    [Fact]
    public async Task Handle_WhenCacheIsFull_ShouldReturnFromDbAndPopulateCache()
    {
        //Arange
        var cacheKey = nameof(GetHeatTreatmentTypesHandler);

        var dbOptions = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ServiceDbContext(dbOptions);
        
        var cacheMock = new Mock<IDistributedCache>();

        var cacheSettingsMock = new Mock<IOptions<CacheSettings>>();
        
        var cachedData = new HeatTreatmentTypeDto[]
        {
            new(Guid.NewGuid(), "Закалка", "Описание", "HARD", 800, 900, "Вода")
        };
        cacheMock.Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.SerializeToUtf8Bytes(cachedData));

        context.DictTypesOfHeatTreatments.Add(new DictTypeOfHeatTreatment
        {
            Id = Guid.NewGuid(),
            Name = "Закалка",
            Description = "Тестовое описание",
            Code = "HARD",
            DefaultTemperatureMin = 800,
            DefaultTemperatureMax = 900,
            DefaultCoolingMedium = "Вода"
        });
        
        context.DictTypesOfHeatTreatments.Add(new DictTypeOfHeatTreatment
        {
            Id = Guid.NewGuid(),
            Name = "Отпуск",
            Description = "Тестовое описание",
            Code = "HARD",
            DefaultTemperatureMin = 200,
            DefaultTemperatureMax = 400,
            DefaultCoolingMedium = "Воздух"
        });
        
        await context.SaveChangesAsync();

        var handler = new GetHeatTreatmentTypesHandler(context, cacheMock.Object,cacheSettingsMock.Object);

        //Act
        var result = await handler.Handle(CancellationToken.None);

        //Assert
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("Закалка");
        
        cacheMock.Verify(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
   
    [Fact]
    public async Task Handle_WhenCacheIsEmpty_ShouldReturnFromDbAndPopulateCache()
    {
         //Arange
        var cacheKey = nameof(GetHeatTreatmentTypesHandler);

        var dbOptions = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ServiceDbContext(dbOptions);
        
        var cacheMock = new Mock<IDistributedCache>();

        var cacheSettingsMock = new Mock<IOptions<CacheSettings>>();
        cacheSettingsMock.Setup(c => c.Value).Returns(new CacheSettings { ExpirationMinutes = 10 });
        
        cacheMock.Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        context.DictTypesOfHeatTreatments.Add(new DictTypeOfHeatTreatment
        {
            Id = Guid.NewGuid(),
            Name = "Закалка",
            Description = "Тестовое описание",
            Code = "HARD",
            DefaultTemperatureMin = 800,
            DefaultTemperatureMax = 900,
            DefaultCoolingMedium = "Вода"
        });
        
        context.DictTypesOfHeatTreatments.Add(new DictTypeOfHeatTreatment
        {
            Id = Guid.NewGuid(),
            Name = "Отпуск",
            Description = "Тестовое описание",
            Code = "HARD",
            DefaultTemperatureMin = 200,
            DefaultTemperatureMax = 400,
            DefaultCoolingMedium = "Воздух"
        });
        
        await context.SaveChangesAsync();

        var handler = new GetHeatTreatmentTypesHandler(context, cacheMock.Object,cacheSettingsMock.Object);

        //Act
        var result = await handler.Handle(CancellationToken.None);

        //Assert
        result.Should().HaveCount(2);
        result!.Select(x => x.Name).Should().Equal("Закалка", "Отпуск"); 
        
        cacheMock.Verify(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}