using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SagamoreTrade.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration Configuration;
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public void OnGet()
        {
            ViewData["ChatId"] = Configuration["TelegramConfiguration:ChatId"];
            ViewData["Test"] = Configuration["Test"];
        }
    }
}
