using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication3.Models;
namespace WebApplication3.Custom
{
    public class Utilidades
    {
        private readonly IConfiguration _config;
        public Utilidades(IConfiguration configuration)
        {
            _config = configuration;
        }

        public String encriptarSHA256(String input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();

            }
        }

        public string generarJwT(Usuario user,Role role)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, user.Users), // Nombre de usuario
        new Claim(ClaimTypes.Name, user.Users),             // Nombre para User.Identity.Name
        new Claim(ClaimTypes.Role,role.Rol),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
