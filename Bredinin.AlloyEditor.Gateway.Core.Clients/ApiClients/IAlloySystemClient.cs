using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;

public interface IAlloySystemClient
{
    [Get("/api/alloys-systems")]
    Task<SearchAlloySystemResponse[]> GetAlloySystem(CancellationToken ctn = default);
}