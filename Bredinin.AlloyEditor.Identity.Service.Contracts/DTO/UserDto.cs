namespace Bredinin.AlloyEditor.Identity.Service.Contracts.DTO
{
    public record UserDto
    {
        public Guid Id { get; set; }
        public required string Login { get; set; }
        public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
    }
}
