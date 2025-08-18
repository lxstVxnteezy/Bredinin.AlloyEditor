namespace Bredinin.AlloyEditor.Identity.Service.Contracts.DTO
{
    public record RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
