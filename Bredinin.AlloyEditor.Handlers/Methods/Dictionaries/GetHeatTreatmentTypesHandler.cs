using System.Text.Json;
using Bredinin.AlloyEditor.Common.Configurations;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;

public interface IGetHeatTreatmentTypesHandler : IHandler
{
    Task<HeatTreatmentTypeDto[]> Handle(CancellationToken ctn);
}

public sealed class GetHeatTreatmentTypesHandler(
    ServiceDbContext context,
    IDistributedCache cache,
    IOptions<CacheSettings> cacheSettings
    ) : IGetHeatTreatmentTypesHandler
{
    private const string CacheKey = nameof(GetHeatTreatmentTypesHandler);

    public async Task<HeatTreatmentTypeDto[]> Handle(CancellationToken ctn)
    {
        var cached = await cache.GetStringAsync(CacheKey, ctn);
            
        if (cached is not null)
            return JsonSerializer.Deserialize<HeatTreatmentTypeDto[]>(cached)!;
        
        var response = await context.DictTypesOfHeatTreatments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new HeatTreatmentTypeDto(
                x.Id,
                x.Name,
                x.Description,
                x.Code,
                x.DefaultTemperatureMin,
                x.DefaultTemperatureMax,
                x.DefaultCoolingMedium
            ))
            .ToArrayAsync(ctn);
        
        await cache.SetStringAsync(CacheKey, 
            JsonSerializer.Serialize(response), 
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow  = TimeSpan.FromMinutes(cacheSettings.Value.ExpirationMinutes)
            },
            ctn);
          
        return response;
    }
}