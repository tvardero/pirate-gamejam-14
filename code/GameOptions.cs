using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class GameOptions
{
    private double _musicVolume = 1;
    private double _effectVolume = 1;
    private bool _showFps = true;
    public const string SaveFileName = "options.json";

    public double MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = value;
            Updated?.Invoke();
        }
    }

    public double EffectVolume
    {
        get => _effectVolume;
        set
        {
            _effectVolume = value;
            Updated?.Invoke();
        }
    }

    public bool ShowFps
    {
        get => _showFps;
        set
        {
            _showFps = value;
            Updated?.Invoke();
        }
    }

    public event Action? Updated;

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