using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class ActorRenderer : MonoBehaviour
{
    private Animator _animator;
    private AnimationType _animationType;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _animationType = new AnimationType();
    }

    private void Start()
    {
        Play(_animationType);
    }

    public void Play(AnimationType animationType)
    {
        _animationType = animationType;
        _animator.Play(_animationType.GetAnimationType(), 0, 0f);
    }
}
