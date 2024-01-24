using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class PlayerStats
{
    public const string SaveFileName = "save.json";
    
    public string? LastPlayedLevel { get; set; }
    public List<string> UnlockedLevels { get; init; } = new();

    public static async Task<PlayerStats> LoadAsync()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = Path.Join(appData, $"ta.{GameData.GameName}", SaveFileName);

        if (!File.Exists(path)) return new();

        await using var fr = File.OpenRead(path);
        var loaded = await JsonSerializer.DeserializeAsync<PlayerStats>(fr);
        return loaded ?? new();
    }

    public async Task SaveAsync()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var path = Path.Join(appData, $"ta.{GameData.GameName}", SaveFileName);

        await using var fw = File.Create(path);
        await JsonSerializer.SerializeAsync(fw, this);
    }
}