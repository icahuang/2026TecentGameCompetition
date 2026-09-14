using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 地板砖：能站 / 不能站。画在地板 Tilemap 上即可。
/// 不用 <see cref="AxisTile"/> 的普通 Tile 一律视为能站。
/// </summary>
[CreateAssetMenu(menuName = "Tiles/Axis Tile", fileName = "AxisTile")]
public class AxisTile : Tile
{
    [SerializeField] private bool _walkable = true;

    public bool Walkable => _walkable;
}
