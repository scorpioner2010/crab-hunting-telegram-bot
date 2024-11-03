using System;
using UnityEngine;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using System.Threading;
using System.Threading.Tasks;

public class TelegramBotManager : MonoBehaviour
{
    private ITelegramBotClient botClient;
    private CancellationTokenSource cts;
    public string myToken;

    private const string GameCommand = "/getgame";
    private const string GameLink = "https://scorpioner2010.github.io/crab-hunting-build/";
    private const string GameShortName = "coinclicker";
    private const string HelpMessage = "Commands:\n/getgame - Get a link to the game.";

    void Start()
    {
        botClient = new TelegramBotClient(myToken);
        cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } }; // Receive all update types
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

        Debug.Log("Bot started!");
    }

    async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update == null)
        {
            Debug.LogError("Update is null.");
            return;
        }

        if (update.Message != null)
        {
            await HandleMessageAsync(update.Message);
        }
        else if (update.CallbackQuery != null)
        {
            await HandleCallbackQueryAsync(update.CallbackQuery);
        }
    }

    async Task HandleMessageAsync(Message message)
    {
        Debug.Log($"Received message: {message.Text}");

        if (string.IsNullOrEmpty(message.Text))
        {
            Debug.LogWarning("Received an empty message.");
            return;
        }

        switch (message.Text.ToLower())
        {
            case GameCommand:
                await SendGameAsync(message.Chat.Id);
                break;
            default:
                await SendHelpMessageAsync(message.Chat.Id);
                break;
        }
    }

    async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null)
        {
            Debug.LogWarning("CallbackQuery has no message.");
            return;
        }

        if ((DateTime.UtcNow - callbackQuery.Message.Date).TotalSeconds > 60)
        {
            Debug.Log("Request is outdated, notifying user.");
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "The link is outdated. Please request a new link using the command /getgame.");
        }
        else
        {
            Debug.Log("Sending game link.");
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, url: GameLink);
        }
    }

    async Task SendGameAsync(long chatId)
    {
        Debug.Log("Sending game link.");
        await botClient.SendGameAsync(chatId, GameShortName);
    }

    async Task SendHelpMessageAsync(long chatId)
    {
        Debug.Log("Sending help message.");
        await botClient.SendTextMessageAsync(chatId, HelpMessage);
    }

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Debug.LogError($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

    void OnApplicationQuit() => cts.Cancel();
}