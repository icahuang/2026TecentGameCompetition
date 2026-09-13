using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField]private Transform _target;

    [Tooltip("角色落在屏幕上的位置。左下为 (0,0)，右上为 (1,1)。")]
    [SerializeField] private Vector2 _viewportAnchor = new Vector2(0.22f, 0.22f);

    [Tooltip("在视口锚点之上再平移的世界坐标微调。")]
    [SerializeField] private Vector2 _worldOffset;

    [Tooltip("跟随平滑时间，0 为贴死。")]
    [SerializeField] private float _smoothTime = 0.12f;

    private Camera _camera;
    private Vector3 _velocity;
    private bool _snapped;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogError($"{name}: 未指定跟随目标。", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = DesiredPosition();
        if (!_snapped || _smoothTime <= 0f)
        {
            transform.position = desired;
            _velocity = Vector3.zero;
            _snapped = true;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);
    }

    private Vector3 DesiredPosition()
    {
        float z = _target.position.z - transform.position.z;
        Vector3 atAnchor = _camera.ViewportToWorldPoint(new Vector3(_viewportAnchor.x, _viewportAnchor.y, z));
        Vector3 shift = _target.position - atAnchor;
        shift.z = 0f;
        return transform.position + shift + (Vector3)_worldOffset;
    }
}
