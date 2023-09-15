using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Bot;

public class CommandHandler {
    private readonly DiscordSocketClient Client;
    private readonly InteractionService Commands;
    private readonly IServiceProvider Services;

    public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services) {
        Client = client;
        Commands = commands;
        Services = services;
    }

    public async Task Start() {
        await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        Client.InteractionCreated += Interaction;
    }

    private async Task Interaction(SocketInteraction arg) {
        try {
            var context = new SocketInteractionContext(Client, arg);
            await Commands.ExecuteCommandAsync(context, Services);
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
            if (arg.Type == InteractionType.ApplicationCommand) {
                await arg.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}