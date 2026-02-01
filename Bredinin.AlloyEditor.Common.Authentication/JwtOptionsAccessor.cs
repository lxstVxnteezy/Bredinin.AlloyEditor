using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Services.Common
{
    public class JwtOptionsAccessor
    {
        public readonly JwtConfiguration Value;

        public JwtOptionsAccessor(IOptions<JwtConfiguration> configOptions)
        {
            Value = configOptions.Value;

            if (string.IsNullOrWhiteSpace(Value.Key))
                throw new ArgumentException("JWT Key cannot be null or empty", nameof(Value.Key));

            if (string.IsNullOrWhiteSpace(Value.Issuer))
                throw new ArgumentException("JWT Issuer cannot be null or empty", nameof(Value.Issuer));

            if (string.IsNullOrWhiteSpace(Value.Audience))
                throw new ArgumentException("JWT Audience cannot be null or empty", nameof(Value.Audience));
        }
    }
}
