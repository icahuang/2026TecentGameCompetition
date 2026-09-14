using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyRenderer))]
public class EnemyController : ActorController
{
    [SerializeField] private int _detectRange = 6;
    [SerializeField] private bool _requireLineOfSight = true;

    private TurnManager _turnManager;

    private void OnEnable()
    {
        _turnManager = FindObjectOfType<TurnManager>();
        if (_turnManager != null) _turnManager.Register(this);
    }

    private void OnDisable()
    {
        if (_turnManager != null) _turnManager.Unregister(this);
    }

    public void TakeTurn(GridActor player)
    {
        if (!enabled || Actor == null || player == null) return;
        GridManager world = Actor.Manager;
        if (world == null) return;

        if (SeesPlayer(player.Cell))
            Chase(player, world);
        else
            Patrol(world);
    }

    private void Chase(GridActor player, GridManager world)
    {
        Vector2Int from = Actor.Cell;
        int currentDist = Manhattan(from, player.Cell);
        Dir? bestDir = null;
        int bestDist = currentDist;

        for (int i = 0; i < 4; i++)
        {
            Dir dir = (Dir)i;
            Vector2Int dest = from + dir.ToDelta();
            if (world.OccupantAt(dest) == player)
            {
                UpdateFace(dir);
                if (_turnManager != null) _turnManager.CatchPlayer(player);
                return;
            }

            if (!world.CanWalk(dest)) continue;

            int dist = Manhattan(dest, player.Cell);
            if (dist >= currentDist) continue;
            if (!bestDir.HasValue || dist < bestDist || (dist == bestDist && dir == LastDir))
            {
                bestDist = dist;
                bestDir = dir;
            }
        }

        if (bestDir.HasValue)
            TryWalk(bestDir.Value);
    }

    private void Patrol(GridManager world)
    {
        Vector2Int from = Actor.Cell;
        List<Dir> options = new List<Dir>(4);
        for (int i = 0; i < 4; i++)
        {
            Dir dir = (Dir)i;
            Vector2Int dest = from + dir.ToDelta();
            if (world.CanWalk(dest))
                options.Add(dir);
        }

        if (options.Count == 0) return;
        TryWalk(options[Random.Range(0, options.Count)]);
    }

    private bool SeesPlayer(Vector2Int playerCell)
    {
        Vector2Int from = Actor.Cell;
        int dist = Manhattan(from, playerCell);
        if (dist > _detectRange) return false;
        if (dist == 0) return true;
        if (!_requireLineOfSight) return true;
        if (from.x != playerCell.x && from.y != playerCell.y) return false;

        Vector2Int delta = playerCell - from;
        delta.x = (int)Mathf.Sign(delta.x);
        delta.y = (int)Mathf.Sign(delta.y);
        if (!DirExtensions.TryFromDelta(delta, out Dir along)) return false;

        GridManager world = Actor.Manager;
        Vector2Int p = from + along.ToDelta();
        while (p != playerCell)
        {
            if (!world.IsPassable(p)) return false;
            p += along.ToDelta();
        }

        return true;
    }

    private static int Manhattan(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
