namespace Bredinin.AlloyEditor.Identity.Service.Contracts.DTO
{
    public record UserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}
