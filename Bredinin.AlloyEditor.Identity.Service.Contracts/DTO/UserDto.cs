namespace Bredinin.AlloyEditor.Identity.Service.Contracts.DTO
{
    public record UserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public List<RoleDto> Roles { get; set; } = null!;
    }
}
