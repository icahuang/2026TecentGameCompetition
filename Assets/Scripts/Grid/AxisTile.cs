using UnityEngine;
using UnityEngine.Tilemaps;

public enum MovementAxis
{
    Free,
    NorthSouth,
    EastWest
}

/// <summary>
/// 画在地板 Tilemap 上即可：该格只允许沿指定轴走。普通 Tile 视为 <see cref="MovementAxis.Free"/>。
/// </summary>
[CreateAssetMenu(menuName = "Tiles/Axis Tile", fileName = "AxisTile")]
public class AxisTile : Tile
{
    [SerializeField] private MovementAxis _axis = MovementAxis.Free;

    public MovementAxis Axis => _axis;
}
