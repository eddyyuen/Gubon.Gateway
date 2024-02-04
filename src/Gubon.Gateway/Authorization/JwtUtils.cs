namespace Gubon.Gateway.Authorization
{
    using Gubon.Gateway.Store.FreeSql.Models;
    using Gubon.Gateway.Store.FreeSql.Models.Dto;
    using Gubon.Gateway.Utils.Config;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public interface IJwtUtils
    {
        public string GenerateJwtToken(User user);
        public string? ValidateJwtToken(string? token);
    }

    public class JwtUtils : IJwtUtils
    {
        private readonly GubonSettings _gubonSettings;

        public JwtUtils(GubonSettings gubonSettings)
        {
            _gubonSettings = gubonSettings;

            if (string.IsNullOrEmpty(_gubonSettings.JwtSettings.Secret))
                throw new Exception("JWT secret not configured");
        }

        public string GenerateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_gubonSettings.JwtSettings.Secret!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("Account", user.Account) }),
                Expires = DateTime.UtcNow.AddMinutes(_gubonSettings.JwtSettings.ExpiredTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string? ValidateJwtToken(string? token)
        {
            token.GetHashCode(StringComparison.OrdinalIgnoreCase);
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_gubonSettings.JwtSettings.Secret!);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userAccount = jwtToken.Claims.First(x => x.Type == "Account").Value;

                // return user id from JWT token if validation successful
                return userAccount;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
