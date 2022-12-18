using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace BirdBridge;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

#if DEBUG
    private static int _holdTime = 1000;
#else
    private static int _holdTime = 10000;
#endif

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if DEBUG
        var proxy = new HttpToSocks5Proxy("10.64.39.49", 61234);
        var httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });
        var Bot = new TelegramBotClient("", httpClient);
#else
        var Bot = new TelegramBotClient("");
#endif

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

        #region 机器人启动后忽略WaitTime时间的消息
        using (var x = new CancellationTokenSource())
        {
            Bot.StartReceiving(
                Handlers.HandleUpdateAsyncIgnore,
                Handlers.HandleErrorAsync,
                receiverOptions,
                x.Token);
            Thread.Sleep(_holdTime);
            x.Cancel();
        }
        #endregion

        Bot.StartReceiving(Handlers.HandleUpdateAsync,
                               Handlers.HandleErrorAsync,
                               receiverOptions,
                               stoppingToken);


        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //    await Task.Delay(1000, stoppingToken);
        //}
    }
}
