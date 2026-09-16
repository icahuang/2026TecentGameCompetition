using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridActor))]
[RequireComponent(typeof(ActorRenderer))]
public abstract class ActorController : MonoBehaviour
{
    // 场景里没放 TurnManager 时的兜底，正常情况走不到。
    private const float FallbackStepDuration = 0.25f;

    protected ActorRenderer Renderer { get; private set; }
    protected GridActor Actor { get; private set; }
    protected AnimationType Animation { get; private set; }
    protected Dir LastDir { get; set; } = Dir.N;
    protected bool Busy { get; set; }

    /// <summary>走一格用几秒。全角色统一取 <see cref="TurnManager"/> 上那一个值。</summary>
    protected static float StepDuration
    {
        get
        {
            TurnManager turn = TurnManager.Instance;
            return turn != null ? turn.StepDuration : FallbackStepDuration;
        }
    }

    protected virtual void Awake()
    {
        Renderer = GetComponent<ActorRenderer>();
        Actor = GetComponent<GridActor>();
        Animation = new AnimationType();
    }

    protected void UpdateFace(Dir dir)
    {
        LastDir = dir;
        Animation.SetAnimationType(Act.idle, dir);
        Renderer.Play(Animation);
    }

    protected bool TryWalk(Dir dir)
    {
        UpdateFace(dir);
        if (!Actor.TryStep(dir.ToDelta())) return false;
        PlayRunAndStep(dir);
        return true;
    }

    protected void PlayRunAndStep(Dir dir)
    {
        LastDir = dir;
        Animation.SetAnimationType(Act.run, dir);
        StartCoroutine(Step());
    }

    private IEnumerator Step()
    {
        // 开局读一次：这一格走到一半时改数值，也不会变速。
        float duration = StepDuration;

        Vector3 from = transform.position;
        Vector3 to = Actor.WorldPosition;
        to.z = from.z;
        Renderer.Play(Animation);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 p = Vector3.Lerp(from, to, t);
            p.z = from.z;
            transform.position = p;
            yield return null;
        }

        transform.position = to;
        Animation.SetAnimationType(Act.idle, Animation.GetDir());
        Renderer.Play(Animation);
        OnStepComplete();
    }

    protected virtual void OnStepComplete() { }
}
