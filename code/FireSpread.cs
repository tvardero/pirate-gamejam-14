using System;
using System.Collections.Generic;
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

    [Export]
    public Rect2I CalculationArea { get; set; }

    public override void _Ready()
    {
        TemperatureMap = new float[CalculationArea.Size.X, CalculationArea.Size.Y];
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
            SpreadTemperature(fireCoords, 400);
        }
    }

    private void SpreadFireAndConsumeFuel(double delta)
    {
        IterateOnMap(CalculateFireSpread);
        return;

        void CalculateFireSpread(Vector2I coords)
        {
            var fuelData = GetCellTileData(BurnableLayer, coords);
            var fuel = fuelData?.GetCustomData(FuelCustomData).AsSingle() ?? 0;
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
            else if (!hasFire && temp >= 300 && fuel > 0)
            {
                // self-ignite if high temp on fueled area
                SetCellsTerrainConnect(FireLayer, new Array<Vector2I>(new[] { coords }), 0, FireTerrain);
                hasFire = true;
            }

            if (hasFire)
            {
                // consume fuel, spread and rise temperature
                fuel -= (float)delta * 5; // 5 points of fuel per second
                SpreadTemperature(coords, temp);
                temp += (float)delta * 100;

                fuelData!.SetCustomData(FuelCustomData, Variant.CreateFrom((double)fuel));
            }
            else if (temp != 0)
            {
                temp -= (float)delta * 100;
                if (temp < 0) temp = 0;
            }

            TemperatureMap[coords.X, coords.Y] = temp;
        }
    }

    private IEnumerable<Vector2I> FindTiles(int layer, Predicate<TileData?> predicate)
    {
        HashSet<Vector2I> found = new();
        IterateOnMap(coords =>
        {
            var tileData = GetCellTileData(layer, coords);
            if (predicate(tileData)) found.Add(coords);
        });

        return found;
    }

    private void IterateOnMap(Action<Vector2I> action)
    {
        for (var x = CalculationArea.Position.X; x < CalculationArea.End.X; x++)
        {
            for (var y = CalculationArea.Position.Y; y < CalculationArea.End.Y; y++) { action(new Vector2I(x, y)); }
        }
    }

    private void SpreadTemperature(Vector2I from, in float temp)
    {
        const float factor = 0.25f;
        for (var x = from.X - 1; x <= from.X + 1; x++)
        {
            for (var y = from.Y - 1; y <= from.Y + 1; y++)
            {
                if (x < 0 || x >= CalculationArea.Size.X) continue;
                if (y < 0 || y >= CalculationArea.Size.Y) continue;

                TemperatureMap[x, y] += factor * temp;
            }
        }

        TemperatureMap[from.X, from.Y] = temp;
    }
}