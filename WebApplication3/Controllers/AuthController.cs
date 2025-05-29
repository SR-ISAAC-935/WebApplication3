using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
                    // Genera el token JWT
                    var token = _utilidades.generarJwT(user);

                    // Guarda el token en una cookie segura
                    Response.Cookies.Append("AppAuthToken", token, new CookieOptions
                    {
                        HttpOnly = true, // La cookie no estará disponible para JavaScript (previene XSS)
                        Secure = false,  // Requiere HTTPS (asegúrate de estar en producción con HTTPS)
                        SameSite = SameSiteMode.Strict, // Previene el envío en solicitudes cruzadas
                        Expires = DateTimeOffset.Now.AddHours(10), // Tiempo de expiración del token
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
        public IActionResult logout()
        {
            HttpContext.Session.Remove("AppAuthToken");
            return RedirectToAction("login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
