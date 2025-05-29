using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebApplication3.Controllers
{
    public class FacturasController : Controller
    {
        public async Task<IActionResult> Facturacion()
        {
            const string FegoraTokenKey = "FegoraToken";

            // Verifica si ya hay un token guardado en sesión
            var tokenSesion = HttpContext.Session.GetString(FegoraTokenKey);
            if (!string.IsNullOrEmpty(tokenSesion))
            {
                Console.WriteLine($" devuelveme este token {tokenSesion}");
            }

            var token = await Sesiones();
            ViewBag.Token = token;
            return View();

        }

        public async Task<string> Sesiones()
        {
            const string FegoraTokenKey = "FegoraToken";

            // Intenta obtener el token de la cookie primero
            var tokenCookie = HttpContext.Request.Cookies["FegoraToken"];

            if (!string.IsNullOrEmpty(tokenCookie))
            {
                // Si la cookie existe, la devolvemos y también la guardamos en la sesión (opcional, para la sesión actual)
                HttpContext.Session.SetString(FegoraTokenKey, tokenCookie);
                return $"Token recuperado de la cookie: {tokenCookie}";
            }

            // Si no hay token en la cookie, o la cookie no existe, solicita uno nuevo
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.fegora.com/token");
            request.Headers.Add("Accept", "application/json");

            var collection = new List<KeyValuePair<string, string>>
    {
        new("grant_type", "password"),
        new("username", "Isaac7Guerra@gmail.com"),
        new("password", "Ara16081976"),
        new("client_id", "apiApp")
    };

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseBody);
            var newToken = json.RootElement.GetProperty("access_token").GetString();

            // Guarda el NUEVO token en la cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            };
            HttpContext.Response.Cookies.Append("FegoraToken", newToken, cookieOptions);

            // Guarda el NUEVO token en la sesión del servidor
            HttpContext.Session.SetString(FegoraTokenKey, newToken);

            return $"Este es el nuevo token: {newToken}";
        }

        [HttpGet]
        public async Task<string> ConsultaNit(string nit)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.fegora.com/contribuyente/{nit}");
            var accessToken = HttpContext.Request.Cookies["FegoraToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                return "Error: No se encontró el token de autorización.";
            }
            else
            {
                Console.WriteLine($"token disponible{accessToken}");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responsebody = await response.Content.ReadAsStringAsync();

                // var json = JsonDocument.Parse(responsebody);

                return responsebody;
            }
            catch (HttpRequestException ex)
            {
                return $"Error al consultar la API de Fegora: {ex.Message}";
            }
            catch (JsonException ex)
            {
                return $"Error al parsear la respuesta de la API: {ex.Message}";
            }


        }
    }
}
