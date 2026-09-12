using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerRenderer : MonoBehaviour
{
    // public static readonly string[] idleDirections = { "idle_N", "idle_E", "idle_S", "idle_W"};
    // public static readonly string[] runDirections = {"run_N", "run_E", "run_S", "run_W"};
    private Animator _animator;
    private Act _act = Act.idle;
    private Dir _dir = Dir.N;
    private AnimationType _animationType;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationType = new AnimationType();
    }

    public void Start()
    {
        Play(_animationType);
    }

    public void Play(AnimationType animationType)
    {
        _animationType = animationType;
        _animator.Play(_animationType.GetAnimationType(), 0, 0f);
    }


}
