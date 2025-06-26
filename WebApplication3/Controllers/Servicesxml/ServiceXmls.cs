using System.Xml.Linq;

namespace WebApplication3.Controllers.Servicesxml
{
    public class ServiceXmls
    {
        private readonly HttpClient _httpClient;

        public ServiceXmls(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<XDocument?> ObtenerXmlDesdeUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                var xmlDoc = XDocument.Parse(content);
                return xmlDoc;
            }
            catch (Exception ex)
            {
                // Log error
                return null;
            }
        }
    }
}
