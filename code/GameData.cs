using System.Threading.Tasks;
using Gamejam.code;

public static class GameData
{
    public const string GameName = "ScorchedAcres";
    
    public static GameOptions GameOptions { get; private set; } = null!;
    public static PlayerStats PlayerStats { get; private set; } = null!;

    public static LevelData? LevelData { get; private set; }
    
    public static async Task LoadAsync()
    {
        GameOptions = await GameOptions.LoadAsync();
        PlayerStats = await PlayerStats.LoadAsync();
    }

    public static async Task SaveAsync()
    {
        await GameOptions.SaveAsync();
        await PlayerStats.SaveAsync();
    }

    public static void ResetPlayerStats()
    {
        PlayerStats = new();
    }

    public static void ResetLevelData() => LevelData = new();
}