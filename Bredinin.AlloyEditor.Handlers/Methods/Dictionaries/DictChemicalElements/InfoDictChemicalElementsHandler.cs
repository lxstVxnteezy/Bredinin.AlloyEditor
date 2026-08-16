using System.Text.Json;
using Bredinin.AlloyEditor.Common.Configurations;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements
{
    public interface IInfoDictChemicalElementsHandler : IHandler
    { 
        Task<DictChemicalElementResponse[]?> Handle(CancellationToken ctn = default);
    }

    internal class InfoDictChemicalElementsHandler(
        IDistributedCache cache,
        ServiceDbContext context,
        IOptions<CacheSettings> cacheSettings)
        : IInfoDictChemicalElementsHandler
    {
        private const string CacheKey = nameof(InfoDictChemicalElementsHandler);

        public async Task<DictChemicalElementResponse[]?> Handle(CancellationToken ctn = default)
        {
            var cached = await cache.GetStringAsync(CacheKey, ctn);
            
            if (cached is not null)
                return JsonSerializer.Deserialize<DictChemicalElementResponse[]>(cached);
            
            var responses = await context.DictChemicalElements
                .AsNoTracking()
                .Select(chemicalElement => new DictChemicalElementResponse(
                    chemicalElement.Id,
                    chemicalElement.Name,
                    chemicalElement.Symbol,
                    chemicalElement.Description,
                    chemicalElement.IsBaseForAlloySystem,
                    chemicalElement.AtomicNumber,
                    chemicalElement.AtomicWeight,
                    chemicalElement.Group,
                    chemicalElement.Period,
                    chemicalElement.Density
                ))
                .ToArrayAsync(ctn);
            
            await cache.SetStringAsync(CacheKey, 
                JsonSerializer.Serialize(responses), 
                new DistributedCacheEntryOptions()
                {
                  AbsoluteExpirationRelativeToNow  = TimeSpan.FromMinutes(cacheSettings.Value.ExpirationMinutes)
                },
                ctn);
          
            return responses;
        }
    }
}