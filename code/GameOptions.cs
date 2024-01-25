using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class GameOptions
{
    public const string SaveFileName = "options.json";

    public double MusicVolume { get; set; } = 1;
    public double EffectVolume { get; set; } = 1;

    public static async Task<GameOptions> LoadAsync()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = Path.Join(appData, $"ta.{GameData.GameName}", SaveFileName);

        if (!File.Exists(path)) return new();

        await using var fr = File.OpenRead(path);
        var loaded = await JsonSerializer.DeserializeAsync<GameOptions>(fr);
        return loaded ?? new();
    }

    public async Task SaveAsync()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dir = Path.Join(appData, $"ta.{GameData.GameName}");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var path = Path.Join(dir, SaveFileName);
        await using var fw = File.Create(path);
        await JsonSerializer.SerializeAsync(fw, this);
    }
}