using DiscordRPC;
using DiscordRPC.Logging;

namespace SoundCloudClient.Discord;

public class DiscordRPCService
{
    private readonly DiscordRpcClient _client;
    private string _soundcloudImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTWEtov-j-5-6N2lx_Zm6l775IMz-KnPBLPkw&s";
    public DiscordRPCService()
    {
        _client = new DiscordRpcClient("1083078635986096199");

        _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
        _client.Initialize();
    }

    public void UpdatePresence(SoundCloudSongInfo song)
    {
        _client.SetPresence(new RichPresence()
        {
            Type = ActivityType.Listening,
            Details = song.Name,
            State = song.Artist,
            Assets = new Assets()
            {
                LargeImageKey = song.Avatar,
                SmallImageKey = _soundcloudImage,
                SmallImageText = "Soundcloud"
            },
            Timestamps = new Timestamps()
            {
                Start = DateTime.UtcNow.AddSeconds(-song.TimePassed),
                End = DateTime.UtcNow.AddSeconds(song.Duration - song.TimePassed)
            },
        });
    }

    public void Clear()
    {
        _client.ClearPresence();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}