using System.Text.Json;
using Bredinin.AlloyEditor.Common.Configurations;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;

public interface IGetMechanicalPropertyTypesHandler : IHandler
{
    Task<MechanicalPropertyTypeDto[]> Handle(CancellationToken ctn);
}

internal sealed class GetMechanicalPropertyTypesHandler(
    ServiceDbContext context,
    IDistributedCache cache,
    IOptions<CacheSettings> cacheSettings) : IGetMechanicalPropertyTypesHandler
{
    private const string CacheKey = nameof(GetMechanicalPropertyTypesHandler);

    public async Task<MechanicalPropertyTypeDto[]> Handle(CancellationToken ctn)
    {
        var cached = await cache.GetStringAsync(CacheKey, ctn);
            
        if (cached is not null)
            return JsonSerializer.Deserialize<MechanicalPropertyTypeDto[]>(cached)!;
        
        var response = await context.DictMechanicalPropertyTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new MechanicalPropertyTypeDto(
                x.Id,
                x.Name,
                x.Unit,
                x.Symbol,
                x.Description,
                x.ValueType,
                x.MinPossible,
                x.MaxPossible
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