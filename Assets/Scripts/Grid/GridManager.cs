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

    private readonly HashSet<Vector2Int> _floor = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, GridActor> _occupants = new Dictionary<Vector2Int, GridActor>();
    private readonly Dictionary<Vector2Int, MovementLinks> _links = new Dictionary<Vector2Int, MovementLinks>();

    public Grid Grid => _grid;

    private void Awake()
    {
        if (_grid == null) {
            _grid = GetComponent<Grid>() ?? GetComponentInParent<Grid>();
        }
        Bake();
    }

    public void Bake()
    {
        _floor.Clear();
        _links.Clear();
        if (_floorTilemap == null)
        {
            Debug.LogError($"{name}: 未指定 Floor Tilemap。", this);
            return;
        }

        foreach (Vector3Int p in _floorTilemap.cellBounds.allPositionsWithin)
        {
            if (!_floorTilemap.HasTile(p)) continue;

            Vector2Int cell = (Vector2Int)p;
            _floor.Add(cell);

            AxisTile axisTile = _floorTilemap.GetTile<AxisTile>(p);
            _links[cell] = axisTile != null ? axisTile.Links : MovementLinks.All;
        }
    }

    public Vector3 ToWorld(Vector2Int cell) => _grid.GetCellCenterWorld((Vector3Int)cell);

    public Vector2Int ToCell(Vector3 world) => (Vector2Int)_grid.WorldToCell(world);

    public GridActor OccupantAt(Vector2Int cell) =>
        _occupants.TryGetValue(cell, out GridActor actor) ? actor : null;

    public bool CanWalk(Vector2Int cell) =>
        _floor.Contains(cell)
        && !_occupants.ContainsKey(cell)
        && IsPassable(cell);

    /// <summary>地板上且非 Blocked。视线穿过时忽略单位占用。</summary>
    public bool IsPassable(Vector2Int cell)
    {
        if (!_floor.Contains(cell)) return false;
        return !_links.TryGetValue(cell, out MovementLinks links) || links != MovementLinks.None;
    }

    public bool AllowsStep(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        if (!DirExtensions.TryFromDelta(delta, out Dir dir)) return false;
        return AllowsLeave(from, dir) && AllowsEnter(to, dir);
    }

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
        if (!CanWalk(to) || !AllowsStep(actor.Cell, to)) return false;

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

    private bool AllowsLeave(Vector2Int cell, Dir dir)
    {
        if (!_links.TryGetValue(cell, out MovementLinks links) || links == MovementLinks.All)
            return true;
        return (links & dir.ToLink()) != 0;
    }

    private bool AllowsEnter(Vector2Int cell, Dir dir)
    {
        if (!_links.TryGetValue(cell, out MovementLinks links) || links == MovementLinks.All)
            return true;
        return (links & dir.Opposite().ToLink()) != 0;
    }

    /// <summary>
    /// 编辑器里选中本物体时，在地板 Tilemap 每个有 Tile 的格子上画线框立方体，
    /// 方便看清可行走区域；有 AxisTile 时用其颜色，否则用默认绿色。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_grid == null) _grid = GetComponent<Grid>() ?? GetComponentInParent<Grid>();
        if (_grid == null || _floorTilemap == null) return;

        foreach (Vector3Int p in _floorTilemap.cellBounds.allPositionsWithin)
        {
            if (!_floorTilemap.HasTile(p)) continue;

            AxisTile axisTile = _floorTilemap.GetTile<AxisTile>(p);
            Color c = axisTile != null ? axisTile.color : new Color(0.2f, 0.9f, 0.4f, 1f);
            c.a = 0.75f;
            Gizmos.color = c;
            Gizmos.DrawWireCube(_grid.GetCellCenterWorld(p), _grid.cellSize);
        }
    }
}
