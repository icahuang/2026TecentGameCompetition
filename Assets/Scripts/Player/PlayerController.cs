using System.Collections;
using UnityEngine;

/// <summary>读四向输入，经 <see cref="GridActor"/> 走一步。不持有地图。</summary>
[RequireComponent(typeof(PlayerRenderer))]
[RequireComponent(typeof(GridActor))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _stepDuration = 1f;

    private PlayerRenderer _playerRenderer;
    private GridActor _actor;
    private AnimationType _animationType;
    private bool _busy;

    private void Awake()
    {
        _playerRenderer = GetComponent<PlayerRenderer>();
        _actor = GetComponent<GridActor>();
        _animationType = new AnimationType();
    }

    private void Update()
    {
        if (_busy) return;

        Dir dir;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0f)
            dir = h > 0f ? Dir.E : Dir.W;
        else if (v != 0f)
            dir = v > 0f ? Dir.N : Dir.S;
        else
            return;

        if (!_actor.TryStep(dir.ToDelta()))
        {
            _animationType.SetAnimationType(Act.idle, dir);
            _playerRenderer.Play(_animationType);
            return;
        }

        _busy = true;
        _animationType.SetAnimationType(Act.run, dir);
        StartCoroutine(Step());
    }

    private IEnumerator Step()
    {
        Vector3 from = transform.position;
        Vector3 to = _actor.WorldPosition;
        to.z = from.z;

        _playerRenderer.Play(_animationType);

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
        _animationType.SetAnimationType(Act.idle, _animationType.GetDir());
        _playerRenderer.Play(_animationType);
        _busy = false;
    }
}
