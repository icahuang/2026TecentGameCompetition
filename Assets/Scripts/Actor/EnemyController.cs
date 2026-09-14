using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyRenderer))]
public class EnemyController : ActorController
{
    [SerializeField] private int _detectRange = 2;
    [SerializeField] private bool _requireLineOfSight = true;
    [SerializeField] private Dir _initDir = Dir.N;

    private TurnManager _turnManager;

    private bool _isSpottedPlayer = false;

    private void OnEnable()
    {
        _turnManager = FindObjectOfType<TurnManager>();
        if (_turnManager != null) _turnManager.Register(this);
    }

    private void OnDisable()
    {
        if (_turnManager != null) _turnManager.Unregister(this);
    }

    private void Start()
    {
        LastDir = _initDir;
        AnimationType animation = new AnimationType();
        animation.SetAnimationType(Act.idle, LastDir);
        Renderer.Play(animation);
    }

    public void TakeTurn(GridActor player)
    {
        if (!enabled || Actor == null || player == null) return;
        GridManager world = Actor.Manager;
        if (world == null) return;

        // SeesPlayer 只负责更新 _isSpottedPlayer（看见过一次就一直记着）。
        SeesPlayer(player.Cell);

        if (_isSpottedPlayer)
            Chase(player, world);
        else
            Patrol(world);
    }

    // 发现player后进行追踪的逻辑
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

    // 巡逻，逻辑是：朝LastDir一直走，撞墙或者被挡换成LastDir的反方向
    private void Patrol(GridManager world)
    {
        // 一直朝当前朝向直走，撞墙或被挡就掉头。
        if (!world.CanWalk(Actor.Cell + LastDir.ToDelta()))
            LastDir = LastDir.Opposite();

        TryWalk(LastDir);
    }

    private void SeesPlayer(Vector2Int playerCell)
    {
        Vector2Int enemyCell = Actor.Cell;

        Vector2Int delta = playerCell - enemyCell;
        // 如果敌人和玩家即不在同一行也不在同一列，则看不到玩家，直接返回false
        if (delta.x != 0 && delta.y != 0) return;

        switch (LastDir)
        {
            case Dir.N:
                if (delta.x > 0 && Mathf.Abs(delta.x) <= _detectRange) 
                {
                    Debug.Log("See u~");
                    _isSpottedPlayer = true;
                }
                break;
            case Dir.S:
                if (delta.x < 0 && Mathf.Abs(delta.x) <= _detectRange)
                {
                    Debug.Log("See u~");
                    _isSpottedPlayer = true;
                }
                break;
            case Dir.W:
                if (delta.y > 0 && Mathf.Abs(delta.y) <= _detectRange)
                {
                    Debug.Log("See u~");
                    _isSpottedPlayer = true;
                }
                break;
            case Dir.E:
                if (delta.y < 0 && Mathf.Abs(delta.y) <= _detectRange)
                {
                    Debug.Log("See u~");
                    _isSpottedPlayer = true;
                }
                break;
        }

        return;
    }

    private int Manhattan(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
