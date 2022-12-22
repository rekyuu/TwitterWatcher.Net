using Microsoft.AspNetCore.Mvc;

namespace TwitterWatcher.Controllers;

[ApiController]
[RequiresAuthHeader]
[Route("api/v1/telegram")]
public class TelegramController : ControllerBase
{
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(ILogger<TelegramController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public void SendMessage(string text)
    {
        TelegramUtility.SendMessage(text);
    }
}