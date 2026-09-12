using UnityEngine;

/// <summary>网格轴方向，不是屏幕方向。顺时针：N → E → S → W。</summary>
public enum Dir { N, E, S, W }

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
        _dir = dir;
        _act = act;
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
