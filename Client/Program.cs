using Domain.Models;
using Microsoft.Ajax.Utilities.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;



namespace Client
{
    internal class Program
    {
       static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        HttpClient client = new HttpClient();
        var result = await client.GetAsync("https://localhost:7093/api/Good");

        Console.WriteLine(result);

        var test = await result.Content.ReadAsStringAsync();
        Console.WriteLine(test);

        Good[] good = JsonConvert.DeserializeObject<Good[]>(test);

        foreach (var product in good)
        {
            Console.WriteLine(good + " ");
        }

        var botClient = new TelegramBotClient("6222987510:AAHLPHV29pEXrB25cC5li2qfCxFHDN8VPXY");

        using CancellationTokenSource cts = new();

        //StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() //receive all update types
        };

        botClient.StartReceiving(
            updateHandler: HadleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        //Send canclellation request to stop bot
        cts.Cancel();
    }
    static async Task HadleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        //Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        //Only process text message
        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        //Echo received message text
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);

        if (message.Text == "Проверка")
        {
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Проверка: ОК!",
            cancellationToken: cancellationToken);
        }

        if (message.Text == "Привет")
        {
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Добро пожаловать в интернет-магазин одежды",
            cancellationToken: cancellationToken);
        }

        if (message.Text == "Картинка")
        {
            await botClient.SendPhotoAsync(
            chatId: chatId,
            photo : "https://mykaleidoscope.ru/uploads/posts/2021-11/1637185888_28-mykaleidoscope-ru-p-interer-magazina-zhenskoi-odezhdi-devushka-28.jpg"   ,
            cancellationToken: cancellationToken);
        }

        if (message.Text == "Видео")
        {
            await botClient.SendVideoAsync(
            chatId: chatId,
            video: ("https://github.com/UrmatbekMarina/-/raw/main/СALM-DOWN.mp3"),
            supportsStreaming: true,
            cancellationToken: cancellationToken);
        }

        if (message.Text == "Стикер")
        {
            await botClient.SendStickerAsync(
            chatId: chatId,
            sticker: ("https://basket-04.wb.ru/vol641/part64135/64135115/images/c246x328/1.jpg"),
            cancellationToken: cancellationToken);
        }
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
                KeyboardButton.WithRequestLocation("Share Location"),
                KeyboardButton.WithRequestContact("Share Contact"),
            });

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Who or Where are you?",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }
    static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
}