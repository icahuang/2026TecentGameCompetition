using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Flags]
public enum MovementLinks
{
    None = 0,
    N = 1 << 0,
    E = 1 << 1,
    S = 1 << 2,
    W = 1 << 3,
    NS = N | S,
    EW = E | W,
    NE = N | E,
    NW = N | W,
    SE = S | E,
    SW = S | W,
    NES = N | E | S,
    NEW = N | E | W,
    NSW = N | S | W,
    ESW = E | S | W,
    All = N | E | S | W
}

/// <summary>
/// 画在地板 Tilemap 上即可。普通 Tile 视为四向连通。
/// </summary>
[CreateAssetMenu(menuName = "Tiles/Axis Tile", fileName = "AxisTile")]
public class AxisTile : Tile
{
    [SerializeField] private MovementLinks _links = MovementLinks.All;

    public MovementLinks Links => _links;
}
