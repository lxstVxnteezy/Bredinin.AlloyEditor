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
            var alloySystems = await context.AlloySystems
                .AsNoTracking()
                .ToArrayAsync(ctn);

            return alloySystems.Select(MapToResponse).ToArray();
        }

        private SearchAlloySystemResponse MapToResponse(Domain.Alloys.AlloySystem alloySystem)
        {
            return new SearchAlloySystemResponse(
                Id: alloySystem.Id,
                BaseElementId: alloySystem.BaseElementId,
                Name: alloySystem.Name,
                Description: alloySystem.Description);
        }
    }
}
