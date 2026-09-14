using UnityEngine;

/// <summary>读四向输入，经 <see cref="GridActor"/> 走一步。不持有地图。</summary>
[RequireComponent(typeof(PlayerRenderer))]
public class PlayerController : ActorController
{
    [SerializeField] private TurnManager _manager;

    protected override void Awake()
    {
        base.Awake();
        if (_manager == null)
            _manager = FindObjectOfType<TurnManager>();
    }

    private void Update()
    {
        if (Busy) return;

        Dir dir;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0f)
            dir = h > 0f ? Dir.E : Dir.W;
        else if (v != 0f)
            dir = v > 0f ? Dir.N : Dir.S;
        else
            return;

        if (!Actor.TryStep(dir.ToDelta()))
        {
            Face(dir);
            return;
        }

        Busy = true;
        if (_manager != null)
            _manager.NotifyPlayerStepped(Actor);
        PlayRunAndStep(dir);
    }

    protected override void OnStepComplete()
    {
        Busy = false;
    }
}
