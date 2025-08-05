using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients
{
    public interface IDictionaryClient
    {
        [Headers("Authorization: Bearer")] // Явно указываем, что заголовок должен передаваться
        [Get("/api/dictionary/chemical-elements")]

        Task<DictChemicalElementResponse[]?> GetChemicalElements(CancellationToken cancellationToken = default);
    }

}
