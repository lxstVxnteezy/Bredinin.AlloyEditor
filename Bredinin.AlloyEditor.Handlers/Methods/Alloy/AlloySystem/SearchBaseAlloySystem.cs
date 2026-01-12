using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem
{
    public interface ISearchAlloySystem : IHandler
    {
        Task<SearchAlloySystemResponse[]> Handle(CancellationToken ctn);
    }
    internal class SearchAlloySystem(ServiceDbContext context)
        : ISearchAlloySystem
    {
        public async Task<SearchAlloySystemResponse[]> Handle(CancellationToken ctn)
        {
            return await context.AlloySystems
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SearchAlloySystemResponse(
                    x.Id,
                    x.BaseElementId,
                    x.Name,
                    x.Description))
                .ToArrayAsync(ctn);
        }
    }
}