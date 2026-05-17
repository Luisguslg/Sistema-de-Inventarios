using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace IITS.Pages
{
    // [SEC-AUDIT]: Mitigación para CWE-352 - Se eliminó el atributo [IgnoreAntiforgeryToken] que
    // deshabilitaba la validación de tokens antifalsificación en esta página. La página de error
    // solo expone un handler GET y no requiere la excepción; mantener el atributo ampliaba
    // innecesariamente la superficie de ataque CSRF al suprimir la protección del framework.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}