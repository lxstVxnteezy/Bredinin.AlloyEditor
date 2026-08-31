using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;

public record RevokeTokenQuery(Guid UserId) : IRequest<Unit>;
