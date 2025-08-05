using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Services.Common
{
    public class JwtProvider 
    {
        public readonly JwtConfiguration JwtConfiguration;

        public JwtProvider(IOptions<JwtConfiguration> configOptions)
        {
            JwtConfiguration = configOptions.Value;

            if (string.IsNullOrWhiteSpace(JwtConfiguration.Key))
                throw new ArgumentException("JWT Key cannot be null or empty", nameof(JwtConfiguration.Key));

            if (string.IsNullOrWhiteSpace(JwtConfiguration.Issuer))
                throw new ArgumentException("JWT Issuer cannot be null or empty", nameof(JwtConfiguration.Issuer));

            if (string.IsNullOrWhiteSpace(JwtConfiguration.Audience))
                throw new ArgumentException("JWT Audience cannot be null or empty", nameof(JwtConfiguration.Audience));
        }

    }
}
