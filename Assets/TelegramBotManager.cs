using System;
using UnityEngine;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using System.Threading;
using System.Threading.Tasks;

public class TelegramBotManager : MonoBehaviour
{
    private static ITelegramBotClient botClient;
    private CancellationTokenSource cts;
    public string myToken;

    void Start()
    {
        botClient = new TelegramBotClient(myToken); // Replace with your bot token
        cts = new CancellationTokenSource();

        // Start receiving updates using Polling
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // Receive all update types
        };

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);
        ConsoleController.Log("Bot started!");
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update == null)
        {
            ConsoleController.LogError("Update is null.");
            return;
        }

        // Log received messages
        if (update.Message != null)
        {
            ConsoleController.Log("Received message: " + update.Message.Text);
        }
        else
        {
            ConsoleController.LogError("Message is null.");
        }

        try
        {
            // Check for /getgametaras command
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message != null)
            {
                var message = update.Message;

                bool result = !string.IsNullOrEmpty(message.Text);
                
                if (result && message.Text.ToLower() == "/getgametaras")
                {
                    ConsoleController.Log("/getgametaras received, sending game.");

                    await botClient.SendGameAsync(
                        chatId: message.Chat.Id, // Chat ID for the game
                        gameShortName: "coinclicker" // Game short name
                    );
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleController.LogError($"Error processing command: {ex.Message}");
        }

        try
        {
            // Handle callback queries
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callbackQuery = update.CallbackQuery;

                // Check if the request is outdated
                if (callbackQuery.Message != null && (DateTime.UtcNow - callbackQuery.Message.Date).TotalSeconds > 60)
                {
                    ConsoleController.Log("Request is outdated, notifying user.");

                    // Send a message to the user indicating the link is outdated
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id,
                        text: "The link is outdated. Please request a new link using the command /getgametaras."
                    );
                    return;
                }

                // Send game link
                await botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    url: "https://scorpioner2010.github.io/CoinsDemoBot/" // Game URL
                );

                ConsoleController.Log("Game link sent.");
            }
        }
        catch (Exception ex)
        {
            ConsoleController.LogError($"Error handling callback query: {ex.Message}");
        }
    }

    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        ConsoleController.LogError($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

    void OnApplicationQuit() => cts.Cancel();
}