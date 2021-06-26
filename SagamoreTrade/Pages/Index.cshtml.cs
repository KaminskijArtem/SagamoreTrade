using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TradingDataLibrary.Logging;

namespace SagamoreTrade.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Log"] = StaticLogger.Log;
        }
    }
}
