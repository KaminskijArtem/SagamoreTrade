using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigurationLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SagamoreTrade.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly TelegramConfiguration _myConfiguration;
        public IndexModel(ILogger<IndexModel> logger, IOptions<TelegramConfiguration> myConfiguration)
        {
            _logger = logger;
            _myConfiguration = myConfiguration.Value;
        }

        public void OnGet()
        {
            ViewData["Test"] = "ChatId: " + _myConfiguration.ChatId;
        }
    }
}
