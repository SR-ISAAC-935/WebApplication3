using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication3.Custom;
using WebApplication3.Models;
using WebApplication3.Models.DTOs;

namespace WebApplication3.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly LumitecContext _context;
        private readonly Utilidades _utilidades;
        public AuthController(ILogger<AuthController> logger, LumitecContext lumitecContext, Utilidades Utilidades)
        {
            _logger = logger;
            _context = lumitecContext;
            _utilidades = Utilidades;
        }


        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            Console.WriteLine($"Username: {registerDTO.Username}, Password: {_utilidades.encriptarSHA256(registerDTO.Password)}, ConfirmPassword: {_utilidades.encriptarSHA256(registerDTO.ConfirmPassword)}");


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Información no válida. Verifique los datos ingresados.";
                return View(registerDTO);
            }

            // Verificar si el usuario ya existe
            var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Users == registerDTO.Username);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "El usuario ya está registrado.";
                return View(registerDTO);
            }

            // Crear un nuevo usuario
            var newUser = new Usuario
            {
                Users = registerDTO.Username,
                Passwords = _utilidades.encriptarSHA256(registerDTO.Password) // ¡Usa hashing para contraseñas en producción!
            };

            await _context.Usuarios.AddAsync(newUser);
            await _context.SaveChangesAsync();

            if (newUser.IdUser != 0)
            {
                TempData["SuccessMessage"] = "Registro exitoso. Ahora puede iniciar sesión.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo registrar";
                return RedirectToAction("Register");
            }

        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTOs loginDTOs)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Información del usuario no válida";
                return View();
            }

            try
            {
                // Busca el usuario en la base de datos
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Users == loginDTOs.Usuario && u.Passwords == _utilidades.encriptarSHA256(loginDTOs.Password));

                if (user != null)
                {
                    var role = _context.Roles.FirstOrDefault(u => u.Id.Equals(user.IdRole));

                    // Genera el token JWT
                    var token = _utilidades.generarJwT(user, role);

                    // Genera el refresh token
                    var refreshToken = _utilidades.GenerateRefreshToken();
                    var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Duración del refresh token

                    // Guarda el refresh token en la base de datos
                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = refreshTokenExpiry;
                    await _context.SaveChangesAsync();

                    // Guarda el access token en una cookie segura
                    Response.Cookies.Append("AppAuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(3),
                        Path = "/", // Asegúrate de que la cookie esté disponible en todo el sitio
                    });

                    // Guarda el refresh token en una cookie segura también (opcional)
                    Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });

                    TempData["SuccessMessage"] = "Inicio de sesión exitoso";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = "Credenciales incorrectas";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el inicio de sesión");
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la solicitud";
                return View();
            }
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequest tokenRequest)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == tokenRequest.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Refresh token inválido o expirado");

            var role = _context.Roles.FirstOrDefault(u => u.Id.Equals(user.IdRole));

            var newAccessToken = _utilidades.generarJwT(user, role);
            var newRefreshToken = _utilidades.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Recomendado si usas formularios
        public async Task<IActionResult> Logout()
        {
            // Obtener el usuario actual usando el token (si lo tienes en contexto)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Usuarios.FindAsync(int.Parse(userId));
                if (user != null)
                {
                    // Revocar el refresh token (lo invalidamos)
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            // Borrar cookies (access token y refresh token)
            Response.Cookies.Delete("AppAuthToken");
            Response.Cookies.Delete("RefreshToken");

            // (Opcional) cerrar la sesión por si estás usando Session también
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Auth");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
