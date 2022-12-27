using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Tweetinvi.Parameters.V2;

namespace BirdBridge
{
    internal class Handlers
    {
        #region 错误拦截
        /// <summary>拦截API错误并输出</summary>
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:{Environment.NewLine}[{apiRequestException.ErrorCode}]{Environment.NewLine}{apiRequestException.Message}{Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>未知api处理(可能是tg服务器的api更新了或者单纯没有去拦截而已)</summary>
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            //Console.WriteLine($"未知更新类型: {update.Type}, 可能是API变更");
            return Task.CompletedTask;
        }
        #endregion

        #region 消息处理
        public static async Task HandleUpdateAsyncIgnore(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
                Console.WriteLine($"消息处理时出错: {Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}");
            }
        }

        /// <summary>所有接收消息拦截处理</summary>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                #region 未被使用的消息拦截器
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                //UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                //UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                //UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                #endregion

                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
                Console.WriteLine($"消息拦截时出错: {Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}");
            }
        }

        /// <summary>接收消息分类</summary>
        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
#if DEBUG
            Console.WriteLine($"RecvMSG! MsgType: {message.Type} | MsgId: {message.MessageId}\r\nChatType: {message.Chat.Type} | ChatId: {message.Chat.Id} | ChatTitle: {message.Chat.Title}");
#endif
            if (message.From.IsBot == false && message.Type == MessageType.Text && message.Chat.Type == ChatType.Private)
            {
#pragma warning disable CS4014

                if (string.IsNullOrEmpty(message.Text) != true)
                {
                    if (message.Text.IndexOf("twitter.com") < 0)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "请输入正常的推特链接!");
                    }
                    else
                    {
                        var i1 = message.Text.IndexOf("/status/") + 8;
                        var i2 = message.Text.IndexOf('?');
                        var tid = string.Empty;
                        if (i2 > 0) tid = message.Text.Substring(i1, i2 - i1);
                        else tid = message.Text.Substring(i1);

                        var fields = new HashSet<string>(TweetResponseFields.Media.ALL) { "variants" };
                        var arg = new GetTweetV2Parameters(tid)
                        {
                            Expansions = { TweetResponseFields.Expansions.AttachmentsMediaKeys, },
                            MediaFields = fields,
                        };
                        var res = Program.Client.Raw.TweetsV2.GetTweetAsync(arg).Result;
                        var result = JsonConvert.DeserializeObject<JsonObj>(res.Content);
                        var mediaGroup = new List<IAlbumInputMedia>();

                        if (result.Errors != null)
                        {
                            botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: result.Errors[0].Detail);
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine("请求的原始内容尺寸超过限制(20MB)!");
                            sb.AppendLine("以下链接是您请求推文的源链接.");

                            var msg = string.Empty;
                            var index = result.Tweet.Text.IndexOf("https://t.co/");
                            if (index > -1) msg = $"<a href=\"https://twitter.com/{result.Includes.Users?[0].Username}/status/{tid}\">{result.Includes.Users?[0].Name}(@{result.Includes.Users?[0].Username})</a>{Environment.NewLine}{result.Tweet.Text.Substring(0, index)}{Environment.NewLine}💖{result.Tweet.PublicMetrics.LikeCount} 🔁{result.Tweet.PublicMetrics.RetweetCount} 💬{result.Tweet.PublicMetrics.ReplyCount}";
                            else msg = $"<a href=\"https://twitter.com/{result.Includes.Users?[0].Username}/status/{tid}\">{result.Includes.Users?[0].Name}(@{result.Includes.Users?[0].Username})</a>{Environment.NewLine}{result.Tweet.Text}{Environment.NewLine}💖{result.Tweet.PublicMetrics.LikeCount} 🔁{result.Tweet.PublicMetrics.RetweetCount} 💬{result.Tweet.PublicMetrics.ReplyCount}";

                            if (result.Includes.Media?.Length > 0)
                            {
                                for (int i = 0; i < result.Includes.Media.Length; i++)
                                {
                                    switch (result.Includes.Media[i].Type)
                                    {
                                        case "photo":
                                            if (mediaGroup.Count == 0) mediaGroup.Add(new InputMediaPhoto(new InputMedia(result.Includes.Media[i].Url)) { ParseMode = ParseMode.Html, Caption = msg });
                                            else mediaGroup.Add(new InputMediaPhoto(new InputMedia(result.Includes.Media[i].Url)));
                                            sb.AppendLine(result.Includes.Media[i].Url);
                                            break;
                                        case "video":
                                        case "animated_gif":
                                            var i4 = 0; // Bitrate
                                            var i5 = 0;
                                            for (int i3 = 0; i3 < result.Includes.Media[i].Variants.Length; i3++)
                                            {
                                                if (result.Includes.Media[i].Variants[i3].Bitrate > i4)
                                                {
                                                    i4 = result.Includes.Media[i].Variants[i3].Bitrate;
                                                    i5 = i3;
                                                }
                                            }
                                            if (mediaGroup.Count == 0) mediaGroup.Add(new InputMediaVideo(new InputMedia(result.Includes.Media[i].Variants[i5].URL)) { ParseMode = ParseMode.Html, Caption = msg });
                                            else mediaGroup.Add(new InputMediaVideo(new InputMedia(result.Includes.Media[i].Variants[i5].URL)));
                                            sb.AppendLine(result.Includes.Media[i].Variants[i5].URL);
                                            break;
                                        default: 
                                            break;
                                    }
                                }
                            }

                            try
                            {
                                _ = botClient.SendMediaGroupAsync(
                                    chatId: message.Chat.Id,
                                    replyToMessageId: message.MessageId,
                                    disableNotification: true,
                                    allowSendingWithoutReply: true,
                                    media: mediaGroup.ToArray()).Result;
                            }
                            catch (Exception ex)
                            {
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    replyToMessageId: message.MessageId,
                                    disableNotification: true,
                                    text: sb.ToString());
                            }
                        }
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "请输入要处理的推特链接!");
                }
#pragma warning restore CS4014
            }
        }
        #endregion
    }
}
