using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 格子世界：地板、占据、格子↔世界坐标。不管输入和动画。
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _floorTilemap;

    private readonly Dictionary<Vector2Int, GridActor> _occupants = new Dictionary<Vector2Int, GridActor>();

    public Grid Grid => _grid;

    private void Awake()
    {
        if (_grid == null) {
            _grid = GetComponent<Grid>() ?? GetComponentInParent<Grid>();
        }
        if (_floorTilemap == null)
            Debug.LogError($"{name}: 未指定 Floor Tilemap。", this);
    }

    public Vector3 ToWorld(Vector2Int cell) => _grid.GetCellCenterWorld((Vector3Int)cell);

    public Vector2Int ToCell(Vector3 world) => (Vector2Int)_grid.WorldToCell(world);

    public GridActor OccupantAt(Vector2Int cell) =>
        _occupants.TryGetValue(cell, out GridActor actor) ? actor : null;

    /// <summary>地板上、且那块砖是能站的。视线穿过时忽略单位占用。</summary>
    public bool IsPassable(Vector2Int cell)
    {
        if (_floorTilemap == null) return false;
        TileBase tile = _floorTilemap.GetTile((Vector3Int)cell);
        if (tile == null) return false;
        return !(tile is AxisTile axisTile) || axisTile.Walkable;
    }

    public bool CanWalk(Vector2Int cell) =>
        IsPassable(cell)
        && !_occupants.ContainsKey(cell);

    public bool TryPlace(GridActor actor, Vector2Int cell)
    {
        if (actor == null || !CanWalk(cell)) return false;
        _occupants[cell] = actor;
        actor.SetCell(cell);
        return true;
    }

    public bool TryMove(GridActor actor, Vector2Int to)
    {
        if (actor == null) return false;
        if (to == actor.Cell) return true;
        if (!CanWalk(to)) return false;

        Unregister(actor);
        _occupants[to] = actor;
        actor.SetCell(to);
        return true;
    }

    public void Release(GridActor actor)
    {
        if (actor != null) Unregister(actor);
    }

    private void Unregister(GridActor actor)
    {
        if (_occupants.TryGetValue(actor.Cell, out GridActor current) && current == actor)
            _occupants.Remove(actor.Cell);
    }

    /// <summary>
    /// 编辑器里选中本物体时，在能站的格子上画线框立方体，方便看清可行走区域。
    /// 直接问 Tilemap，所以改了地图立刻就能看到。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_grid == null) _grid = GetComponent<Grid>() ?? GetComponentInParent<Grid>();
        if (_grid == null || _floorTilemap == null) return;

        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.75f);
        foreach (Vector3Int p in _floorTilemap.cellBounds.allPositionsWithin)
        {
            if (!IsPassable((Vector2Int)p)) continue;
            Gizmos.DrawWireCube(_grid.GetCellCenterWorld(p), _grid.cellSize);
        }
    }
}
