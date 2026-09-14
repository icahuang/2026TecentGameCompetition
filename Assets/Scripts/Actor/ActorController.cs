using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GridActor))]
public abstract class ActorController : MonoBehaviour
{
    [SerializeField] private float _stepDuration = 1f;

    protected ActorRenderer Renderer { get; private set; }
    protected GridActor Actor { get; private set; }
    protected AnimationType Animation { get; private set; }
    protected Dir LastDir { get; set; } = Dir.N;
    protected bool Busy { get; set; }

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
        Vector3 from = transform.position;
        Vector3 to = Actor.WorldPosition;
        to.z = from.z;
        Renderer.Play(Animation);

        float elapsed = 0f;
        while (elapsed < _stepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _stepDuration);
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
