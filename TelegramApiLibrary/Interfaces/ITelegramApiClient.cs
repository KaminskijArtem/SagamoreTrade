using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramApiLibrary.Interfaces
{
    public interface ITelegramApiClient
    {
        Task<HttpResponseMessage> SendMessage(TelegramApiBot bot, string text);
    }
}
