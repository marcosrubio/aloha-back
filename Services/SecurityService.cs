using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Aloha.Model.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Aloha.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly IConfiguration configuration;

        private readonly AlohaContext dbContext;

        public SecurityService(
            IConfiguration configuration,
            AlohaContext dbContext)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
        }

        public static string SHA256HexHashString(string str)
        {
            string hashString;

            using (var sha256 = SHA256Managed.Create())
            {
                var hash = sha256.ComputeHash(Encoding.Default.GetBytes(str));
                hashString = string.Join(string.Empty, hash.Select(b => b.ToString("X2")));
            }

            return hashString;
        }

        public string GenerateJwtToken(string username, string password)
        {
            string passwordHash = SHA256HexHashString(password);
            var user = dbContext.Users.SingleOrDefault(x => x.UserName == username && x.PasswordHash == passwordHash);

            if (user == null)
            {
                return null;
            }

            var key = Encoding.ASCII.GetBytes(configuration["JwtKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, "admin")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return SHA256HexHashString(password);
        }
    }
}