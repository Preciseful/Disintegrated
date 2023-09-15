using Discord;
using Discord.Interactions;

namespace Bot.Commands; 

public class Ping : InteractionModuleBase<SocketInteractionContext>{
    private CommandHandler handler;

    public Ping(CommandHandler handler) => this.handler = handler;

    [SlashCommand("ping", "Get discord bot's latency.")]
    public async Task Invoke() {
        await DeferAsync(true);
        Embed embed = new EmbedBuilder()
            .WithTitle("Pong! :ping_pong:")
            .WithDescription("Latency: " + Context.Client.Latency.ToString() + "ms.")
            .WithCurrentTimestamp()
            .WithColor(Color.Blue)
            .Build();

        await ModifyOriginalResponseAsync(m => m.Embed = embed);
    }
}