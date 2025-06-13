using System.IO;

namespace SoundCloudClient;

public class Config
{
    public string ProxySettings { get; set; } = string.Empty;
    public bool EnableAdBlock { get; set; }
    public bool EnableDiscordRPC { get; set; }


    private static readonly string ConfigPath = "config.json";

    public static Config Load()
    {
        if (!File.Exists(ConfigPath))
            return new Config();

        try
        {
            string json = File.ReadAllText(ConfigPath);
            return System.Text.Json.JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }
        catch
        {
            return new Config();
        }
    }

    public void Save()
    {
        string json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(ConfigPath, json);
    }

    public override bool Equals(object? obj)
    {
        if(obj == null || (obj is Config) == false)
            return false;

        var config = obj as Config;

        return (config!.ProxySettings, config.EnableDiscordRPC, config.EnableAdBlock)
                == (ProxySettings, EnableDiscordRPC, EnableAdBlock);
    }
}
