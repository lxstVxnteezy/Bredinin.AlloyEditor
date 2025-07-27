using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients
{
    public interface IDictionaryClient
    {
        [Get("/api/dictionary/chemical-elements")]
        Task<DictChemicalElementResponse[]> GetChemicalElements(CancellationToken cancellationToken = default);
    }

}
