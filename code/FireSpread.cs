using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class FireSpread : TileMap
{
    private const string BgLayerName = "bg";
    private const string WheatLayerName = "stubbles";
    private const string FileLayerName = "fire";

    private int _bgLayerId;
    private int _wheatLayerId;
    private int _fireLayerId;

    private DateTime _lastSpread;

    private List<Vector2I> _fires = new();
    private System.Collections.Generic.Dictionary<Vector2I, WheatTileData> _wheats = new();
    private TimeSpan _spreadFreq = TimeSpan.FromSeconds(1.2);

    [Export]
    private Rect2I Size { get; set; }

    [Export]
    private double SpreadFreqSeconds
    {
        get => _spreadFreq.TotalSeconds;
        set => _spreadFreq = TimeSpan.FromSeconds(value);
    }

    [Export]
    private float SpreadChance { get; set; } = 0.1f;

    [Export]
    public float FuelConsumptionBySecond { get; set; } = 10f;

    public int WheatRemaining => _wheats.Count;

    public event Action? WheatsUpdated;

    public event Action<Vector2>? IgnitionAttempt;

    public override void _Ready()
    {
        _bgLayerId = Enumerable.Range(0, GetLayersCount()).Single(idx => GetLayerName(idx) == BgLayerName);
        _wheatLayerId = Enumerable.Range(0, GetLayersCount()).Single(idx => GetLayerName(idx) == WheatLayerName);
        _fireLayerId = Enumerable.Range(0, GetLayersCount()).Single(idx => GetLayerName(idx) == FileLayerName);

        FindFiresAndWheat();

        _lastSpread = DateTime.Now;
    }

    public override void _Process(double delta)
    {
        var now = DateTime.Now;
        if (now > _lastSpread + _spreadFreq)
        {
            delta = (now - _lastSpread).TotalSeconds;
            var spreadTimes = (int)(delta / _spreadFreq.TotalSeconds);
            var fireCopy = _fires.ToArray();
            foreach (var fire in fireCopy)
            {
                CalculateFireAt(fire, (float)delta, spreadTimes);
            }

            _lastSpread = now;
        }
    }

    public void Ignite(Vector2 position, float radius)
    {
        var xLeft = (int)Math.Ceiling(position.X - radius);
        var xRight = (int)Math.Floor(position.X + radius);
        var yTop = (int)Math.Ceiling(position.Y - radius);
        var yBottom = (int)Math.Floor(position.Y + radius);
        for (var x = xLeft; x <= xRight; x++)
        for (var y = yTop; y <= yBottom; y++)
        {
            var coords = new Vector2I(x, y);
            if ((coords - position).LengthSquared() > radius * radius) continue;

            AddFire(coords);
        }
    }

    public void Extinguish(Vector2 position, float radius)
    {
        var xLeft = (int)Math.Ceiling(position.X - radius);
        var xRight = (int)Math.Floor(position.X + radius);
        var yTop = (int)Math.Ceiling(position.Y - radius);
        var yBottom = (int)Math.Floor(position.Y + radius);
        for (var x = xLeft; x <= xRight; x++)
        for (var y = yTop; y <= yBottom; y++)
        {
            var coords = new Vector2I(x, y);
            if ((coords - position).LengthSquared() > radius * radius) continue;

            RemoveFire(coords);
        }
    }

    private void FindFiresAndWheat()
    {
        for (var x = Size.Position.X; x < Size.End.X; x++)
        for (var y = Size.Position.Y; y < Size.End.Y; y++)
        {
            var coords = new Vector2I(x, y);

            var wheatData = GetCellTileData(_wheatLayerId, coords);
            if (wheatData != null)
                _wheats[coords] = new WheatTileData
                {
                    Wetness = 0,
                    Fuel = 100f
                };

            var fireData = GetCellTileData(_fireLayerId, coords);
            if (fireData != null) _fires.Add(coords);
        }
    }

    private void CalculateFireAt(Vector2I coords, float delta, int spreadTimes)
    {
        var wheatData = GetCellTileData(_wheatLayerId, coords);
        if (wheatData == null)
        {
            RemoveWheat(coords);
            RemoveFire(coords);
            return;
        }

        var fuel = _wheats[coords].Fuel;
        fuel -= delta * FuelConsumptionBySecond;
        if (fuel <= 0)
        {
            RemoveWheat(coords);
            RemoveFire(coords);
            return;
        }

        _wheats[coords] = _wheats[coords] with { Fuel = fuel };

        if (spreadTimes <= 0) return;

        System.Collections.Generic.Dictionary<Vector2I, TileData> neighborWheats = new();
        for (var x = coords.X - 1; x <= coords.X + 1; x++)
        for (var y = coords.Y - 1; y <= coords.Y + 1; y++)
        {
            var spreadCoords = new Vector2I(x, y);
            if (spreadCoords == coords) continue;

            var spreadWheatData = GetCellTileData(_wheatLayerId, spreadCoords);
            if (spreadWheatData == null) continue;

            var spreadFireData = GetCellTileData(_fireLayerId, spreadCoords);
            if (spreadFireData != null) continue;

            neighborWheats[spreadCoords] = spreadWheatData;
        }

        while (spreadTimes > 0)
        {
            if (Random.Shared.NextSingle() <= SpreadChance)
            {
                var direction = Random.Shared.Next(8) switch
                {
                    0 => TileSet.CellNeighbor.LeftSide,
                    1 => TileSet.CellNeighbor.TopLeftCorner,
                    2 => TileSet.CellNeighbor.TopSide,
                    3 => TileSet.CellNeighbor.TopRightCorner,
                    4 => TileSet.CellNeighbor.RightSide,
                    5 => TileSet.CellNeighbor.BottomRightCorner,
                    6 => TileSet.CellNeighbor.BottomSide,
                    7 => TileSet.CellNeighbor.BottomLeftCorner,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var chosen = GetNeighborCell(coords, direction);
                if (neighborWheats.ContainsKey(chosen))
                {
                    var wetness = _wheats[coords].Wetness;
                    if (wetness > 0)
                    {
                        wetness--;
                        _wheats[coords] = _wheats[coords] with { Wetness = wetness };
                    }
                    else AddFire(chosen);
                }
                else IgnitionAttempt?.Invoke(chosen + new Vector2(0.5f, 0.5f));
            }

            spreadTimes--;
        }
    }

    private void AddFire(Vector2I coords)
    {
        GD.Print("Igniting at: ", coords);
        _fires.Add(coords);
        SetCell(_fireLayerId, coords, 29, new Vector2I(0, 7));
    }

    private void RemoveFire(Vector2I coords, int addWetness = 0)
    {
        _fires.Remove(coords);
        SetCell(_fireLayerId, coords);

        if (addWetness <= 0) return;

        var wheatTile = GetCellTileData(_wheatLayerId, coords);
        if (wheatTile == null) return;

        var wetness = _wheats[coords].Wetness;
        wetness += addWetness;
        _wheats[coords] = _wheats[coords] with { Wetness = wetness };
    }

    private void RemoveWheat(Vector2I coords)
    {
        GD.Print("Removing wheat: ", coords);
        _wheats.Remove(coords);
        SetCell(_wheatLayerId, coords);
        SetCellsTerrainConnect(_bgLayerId, new Array<Vector2I>(new[] { coords }), 0, 2);
        WheatsUpdated?.Invoke();
    }

    private struct WheatTileData
    {
        public int Wetness { get; set; }
        public float Fuel { get; set; }
    }
}