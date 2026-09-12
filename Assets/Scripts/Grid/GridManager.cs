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
    private readonly Dictionary<Vector2Int, MovementAxis> _axes = new Dictionary<Vector2Int, MovementAxis>();

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
        _axes.Clear();
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
            _axes[cell] = axisTile != null ? axisTile.Axis : MovementAxis.Free;
        }
    }

    public Vector3 ToWorld(Vector2Int cell) => _grid.GetCellCenterWorld((Vector3Int)cell);

    public Vector2Int ToCell(Vector3 world) => (Vector2Int)_grid.WorldToCell(world);

    public GridActor OccupantAt(Vector2Int cell) =>
        _occupants.TryGetValue(cell, out GridActor actor) ? actor : null;

    public bool CanWalk(Vector2Int cell) => _floor.Contains(cell) && !_occupants.ContainsKey(cell);

    public bool AllowsStep(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        return AllowsAxis(from, delta) && AllowsAxis(to, delta);
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

    private bool AllowsAxis(Vector2Int cell, Vector2Int delta)
    {
        if (!_axes.TryGetValue(cell, out MovementAxis axis) || axis == MovementAxis.Free)
            return true;
        if (axis == MovementAxis.NorthSouth)
            return delta == Dir.N.ToDelta() || delta == Dir.S.ToDelta();
        if (axis == MovementAxis.EastWest)
            return delta == Dir.E.ToDelta() || delta == Dir.W.ToDelta();
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (_grid == null) _grid = GetComponent<Grid>() ?? GetComponentInParent<Grid>();
        if (_grid == null || _floorTilemap == null) return;

        foreach (Vector3Int p in _floorTilemap.cellBounds.allPositionsWithin)
        {
            if (!_floorTilemap.HasTile(p)) continue;

            AxisTile axisTile = _floorTilemap.GetTile<AxisTile>(p);
            MovementAxis axis = axisTile != null ? axisTile.Axis : MovementAxis.Free;
            if (axis == MovementAxis.NorthSouth)
                Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.7f);
            else if (axis == MovementAxis.EastWest)
                Gizmos.color = new Color(1f, 0.45f, 0.2f, 0.7f);
            else
                Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.6f);

            Gizmos.DrawWireCube(_grid.GetCellCenterWorld(p), _grid.cellSize);
        }
    }
}
