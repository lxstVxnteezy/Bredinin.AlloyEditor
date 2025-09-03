namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Entities
{
    public class RefreshTokenCacheEntry
    {
        public Guid UserId { get; set; }
        public DateTime Expires { get; set; }
    }
}
