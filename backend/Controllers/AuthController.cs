using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SplitMate.Api.Data;
using SplitMate.Api.Models;
using SplitMate.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SplitMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Szukamy użytkownika (ignorując wielkość liter)
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Login.ToLower() == request.Login.ToLower());

            if (user == null)
                return NotFound("Użytkownik nie istnieje");

            // 2. Weryfikujemy hasło używając AuthService
            if (!SplitMate.Api.Services.AuthService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Błędne hasło");
}

            // 3. Generujemy token JWT
            var token = CreateToken(user);

            return Ok(new { 
                token = token,
                username = user.Name,
                userId = user.Id
            });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var keyString = _configuration.GetSection("AppSettings:Token").Value;
            
            if (string.IsNullOrEmpty(keyString) || keyString.Length < 64)
            {
                // To jest bezpieczny, długi klucz zapasowy (ponad 64 znaki)
                keyString = "super_tajny_i_bardzo_dlugi_klucz_do_splitmate_1234567890_abcdef_ghijk_lmnop_qrstuv";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}