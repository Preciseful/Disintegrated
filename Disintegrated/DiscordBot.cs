using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bot;

public class DiscordBot {
    private InteractionService Commands = null!;
    private IConfiguration Configuration;
    
    public static SocketGuild Server { get; private set; } = null!;
    public static DiscordSocketClient Client { get; private set; } = null!;

    public DiscordBot(string? path) {
        if (path == null) throw new ArgumentException("No JSON config file provided.", "path");
        
        IConfigurationBuilder _builder = new ConfigurationBuilder()
            .AddJsonFile(path: $"{(path.Last() == '\\' ? path : path + '\\')}config.json");
        
        Configuration = _builder.Build();
    }
    
    public static Task Main(string[] args) => new DiscordBot(args.ElementAtOrDefault(0)).MainAsync();

    public async Task MainAsync() {
        ServiceProvider serviceProvider = ConfigureServices();
        
        Client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        Commands = serviceProvider.GetRequiredService<InteractionService>();

        Client.Ready += ReadyAsync;
        Client.Log += LogAsync;
        Client.JoinedGuild += JoinGuild;
        Commands.Log += LogAsync;
        
        await Client.LoginAsync(TokenType.Bot, Configuration["Token"]);
        await Client.StartAsync();

        await serviceProvider.GetRequiredService<CommandHandler>().Start();
        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log) {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Discord: " + log.ToString());

        return Task.CompletedTask;
    }

    public static void Log(string log) {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(log);
    }
    
    public static void Error(string log) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(log);
    }

    private async Task JoinGuild(SocketGuild guild) {
        await Commands.RegisterCommandsToGuildAsync(guild.Id);
    }

    private async Task ReadyAsync() {
        foreach (SocketGuild guild in Client.Guilds) 
            await Commands.RegisterCommandsToGuildAsync(guild.Id);
    }
    
    private ServiceProvider ConfigureServices() => 
        new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandHandler>()
            .AddSingleton<DiscordSocketConfig>()
            .BuildServiceProvider();
}
