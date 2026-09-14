using UnityEngine;

/// <summary>网格轴方向，不是屏幕方向。顺时针：N → E → S → W。</summary>
public enum Dir { N, E, S, W }

public static class DirExtensions
{
    public static Vector2Int ToDelta(this Dir dir)
    {
        switch (dir)
        {
            case Dir.N: return new Vector2Int(1, 0);
            case Dir.E: return new Vector2Int(0, -1);
            case Dir.S: return new Vector2Int(-1, 0);
            case Dir.W: return new Vector2Int(0, 1);
            default: return Vector2Int.zero;
        }
    }

    public static Dir Opposite(this Dir dir)
    {
        switch (dir)
        {
            case Dir.N: return Dir.S;
            case Dir.E: return Dir.W;
            case Dir.S: return Dir.N;
            case Dir.W: return Dir.E;
            default: return dir;
        }
    }

    public static bool TryFromDelta(Vector2Int delta, out Dir dir)
    {
        for (int i = 0; i < 4; i++)
        {
            dir = (Dir)i;
            if (dir.ToDelta() == delta) return true;
        }

        dir = Dir.N;
        return false;
    }
}

/// <summary>Animator 状态前缀，实际播放 <c>{act}_{dir}</c>。</summary>
public enum Act { idle, run }

public class AnimationType
{
    private Dir _dir = Dir.N;
    private Act _act = Act.idle;
    public AnimationType()
    {
        // 默认动画为 idle_N
    }
    public void SetAnimationType(Act act, Dir dir)
    {
        _act = act;
        _dir = dir;
    }
    public Dir GetDir()
    {
        return _dir;
    }
    public Act GetAct()
    {
        return _act;
    }
    public string GetAnimationType()
    {
        return $"{_act}_{_dir}";
    }
}
