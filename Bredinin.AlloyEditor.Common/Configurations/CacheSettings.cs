namespace Bredinin.AlloyEditor.Common.Configurations
{
    public class CacheSettings
    {
        public const string SectionName = "Cache";
        public int ExpirationMinutes { get; set; }
        public int MaxSize { get; set; }
    }
}
