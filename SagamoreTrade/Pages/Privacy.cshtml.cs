using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TradingDataLibrary.Interfaces;

namespace SagamoreTrade.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly IRSITradeCandlesService tradeCandlesService;

        public PrivacyModel(IRSITradeCandlesService tradeCandlesService)
        {
            this.tradeCandlesService = tradeCandlesService;
        }

        public async void OnGet()
        {
            var s = await tradeCandlesService.GetStrategyInformation();
        }
    }
}
