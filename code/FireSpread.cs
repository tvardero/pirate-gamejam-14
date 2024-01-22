using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class FireSpread : TileMap
{
    private const int GroundLayer = 0;
    private const int BurnableLayer = 1;
    private const int FireLayer = 2;
    private const string FuelCustomData = "fuel";
    private const string CanBurnCustomData = "can_burn";
    private const int FireTerrain = 2;

    private float[,] TemperatureMap = null!; // TemperatureMap[x][y], made because godot can not contain CustomData for empty tiles
    private float[,] TemperatureMapNextTick = null!;

    [Export]
    public Rect2I CalculationArea { get; set; }

    public override void _Ready()
    {
        TemperatureMap = new float[CalculationArea.Size.X, CalculationArea.Size.Y];
        TemperatureMapNextTick = new float[CalculationArea.Size.X, CalculationArea.Size.Y];
        InitializeFire();
    }

    public override void _PhysicsProcess(double delta)
    {
        SpreadFireAndConsumeFuel(delta);
    }

    private void InitializeFire()
    {
        var fires = FindTiles(FireLayer, td => td != null && td.Terrain == FireTerrain);
        foreach (var fireCoords in fires)
        {
            TemperatureMap[fireCoords.X, fireCoords.Y] += 400;
            SpreadTemperature(1f, fireCoords, 400);
        }
        SwapTemperatureMap();
    }

    private void SpreadFireAndConsumeFuel(double delta)
    {
        IterateOnMap(CalculateFireSpread);
        SwapTemperatureMap();
        return;

        void CalculateFireSpread(Vector2I coords)
        {
            var fuel = GetCellTileData(BurnableLayer, coords)?.GetCustomData(FuelCustomData).AsSingle();
            var hasFire = GetCellTileData(FireLayer, coords) != null;
            var temp = TemperatureMap[coords.X, coords.Y];

            if (hasFire && temp < 150)
            {
                // fire dies to low temp
                SetCell(FireLayer, coords);
                hasFire = false;
            }
            else if (hasFire && fuel <= 0)
            {
                // fire dies to no fuel
                SetCell(FireLayer, coords);
                hasFire = false;
            }
            else if (!hasFire && temp >= 300)
            {
                // self-ignite if high temp
                SetCellsTerrainConnect(FireLayer, new Array<Vector2I>(new[] {coords}), 0, FireTerrain);
                hasFire = true;
            }

            if (hasFire)
            {
                // consume fuel, spread and rise temperature
                fuel -= (float)delta * 5; // 5 points of fuel per second
                SpreadTemperature((float) delta, coords, temp);
                TemperatureMapNextTick[coords.X, coords.Y] += (float)delta * (temp < 300 ? 300 : temp < 600 ? 600 - temp : 0);
            }
        }
    }

    private IEnumerable<Vector2I> FindTiles(int layer, Predicate<TileData?> predicate)
    {
        ConcurrentBag<Vector2I> found = new();
        IterateOnMap(coords =>
        {
            var tileData = GetCellTileData(layer, coords);
            if (predicate(tileData)) found.Add(coords);
        });

        return found;
    }

    private void IterateOnMap(Action<Vector2I> action)
    {
        Parallel.For(CalculationArea.Position.X,
            CalculationArea.End.X,
            x => Parallel.For(CalculationArea.Position.Y, CalculationArea.End.Y, y => action(new Vector2I(x, y))));
    }

    private void SpreadTemperature(float delta, Vector2I from, float temp)
    {
        const float factor = 0.25f;
        for (var x = from.X - 1; x <= from.X + 1; x++)
        {
            for (var y = from.Y - 1; y <= from.Y + 1; y++)
            {
                if (x < 0 || x >= CalculationArea.Size.X) continue;
                if (y < 0 || y >= CalculationArea.Size.Y) continue;

                TemperatureMapNextTick[x, y] += factor * temp;
            }
        }

        if (temp != 0)
        {
            // drop temperature
            temp -= delta * (temp > 400 ? 0.125f * temp : 50);
            if (temp < 0) temp = 0;
        }

        TemperatureMapNextTick[from.X, from.Y] = temp;
    }

    private void SwapTemperatureMap()
    {
        (TemperatureMap, TemperatureMapNextTick) = (TemperatureMapNextTick, TemperatureMap);
    }
}