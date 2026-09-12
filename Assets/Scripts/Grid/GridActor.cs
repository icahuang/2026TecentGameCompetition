using UnityEngine;

/// <summary>
/// 占一格的单位。逻辑位置只有 <see cref="Cell"/>；画面插值由上层负责。
/// </summary>
public class GridActor : MonoBehaviour
{
    [SerializeField] private GridManager _manager;

    public Vector2Int Cell { get; private set; }

    public Vector3 WorldPosition =>
        _manager != null ? _manager.ToWorld(Cell) : transform.position;

    private void Start()
    {
        if (_manager == null)
            _manager = FindObjectOfType<GridManager>();

        if (_manager == null)
        {
            Debug.LogError($"{name}: 未指定 GridManager。", this);
            enabled = false;
            return;
        }

        Vector2Int cell = _manager.ToCell(transform.position);
        if (!_manager.TryPlace(this, cell))
        {
            Debug.LogError($"{name}: 起点 {cell} 放不下。", this);
            enabled = false;
            return;
        }

        Vector3 p = _manager.ToWorld(cell);
        transform.position = new Vector3(p.x, p.y, transform.position.z);
    }

    private void OnDestroy()
    {
        if (_manager != null) _manager.Release(this);
    }

    public bool TryStep(Vector2Int delta)
    {
        if (_manager == null) return false;
        return _manager.TryMove(this, Cell + delta);
    }

    internal void SetCell(Vector2Int cell) => Cell = cell;
}
