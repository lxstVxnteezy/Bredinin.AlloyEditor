using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.DAL.Core;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem
{
    public interface ISearchAlloySystem : IHandler
    {
        Task<SearchAlloySystemResponse[]> Handle(CancellationToken ctn);
    }
    internal class SearchAlloySystem(IRepository<Domain.Alloys.AlloySystem> alloySystemRepository)
        : ISearchAlloySystem
    {
        public async Task<SearchAlloySystemResponse[]> Handle(CancellationToken ctn)
        {
            var data = await alloySystemRepository.Query.ToArrayAsync(ctn);

            return data.Select(MapToResponse).ToArray();
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
