namespace SoundCloudClient.Discord;

public record SoundCloudSongInfo
(
    string Name,
    string Avatar,
    string Artist,
    string Url,
    int TimePassed,
    int Duration,
    bool IsPaused    
);