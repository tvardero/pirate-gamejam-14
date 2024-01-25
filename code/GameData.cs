using System;
using System.Threading.Tasks;

public static class GameData
{
    public const string GameName = "Firespread";
    
    public static GameOptions GameOptions { get; private set; } = null!;
    public static PlayerStats PlayerStats { get; private set; } = null!;

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
}