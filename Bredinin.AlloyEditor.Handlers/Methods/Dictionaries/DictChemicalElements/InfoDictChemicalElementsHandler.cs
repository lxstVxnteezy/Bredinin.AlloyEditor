using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements
{
    public interface IInfoDictChemicalElementsHandler : IHandler
    { 
        Task<DictChemicalElementResponse[]?> Handle(CancellationToken ctn = default);
    }

    internal class InfoDictChemicalElementsHandler(
        IMemoryCache memoryCache,
        ServiceDbContext context)
        : IInfoDictChemicalElementsHandler
    {
        private const string CacheKey = nameof(InfoDictChemicalElementsHandler);

        public async Task<DictChemicalElementResponse[]?> Handle(CancellationToken ctn = default)
        {
            return await memoryCache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                entry.SetPriority(CacheItemPriority.High);

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

                return responses;
            });
        }
    }
}