using UnityEngine;
using System.Collections;

/// <summary>读四向输入，交给 <see cref="GridManager"/>。不持有地图。</summary>

[RequireComponent(typeof(PlayerRenderer))]
public class PlayerController : MonoBehaviour
{
    // GridManager判断是否可以移动
    // private GridManager _gridManager;
    // PlayerRenderer播放动画
    private PlayerRenderer _playerRenderer;
    // AnimationType播放动画
    private AnimationType _animationType;

    // 判断是否可以移动
    private bool _nextCellCanGo = false;
    private bool _isRunning = false;
    public float _runDuration = 1f;
    private void Awake()
    {
        // if (_gridManager == null)
        // {
        //     Debug.LogError("PlayerController: GridManager 未赋值！");
        // }
        _playerRenderer = GetComponent<PlayerRenderer>();
        _animationType = new AnimationType();
    }

    private void Update()
    {
        if (_isRunning) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
    

        if (horizontalInput != 0)
        {
            _animationType.SetAnimationType(Act.run, horizontalInput > 0 ? Dir.E : Dir.W);
            _isRunning = true;
            StartCoroutine(Run());
        }
        else if (verticalInput != 0)
        {
            _animationType.SetAnimationType(Act.run, verticalInput > 0 ? Dir.N : Dir.S);
            _isRunning = true;
            StartCoroutine(Run());
            // 加入移动逻辑
        }
    }

    IEnumerator Run()
    {
        _playerRenderer.Play(_animationType);
        yield return new WaitForSeconds(_runDuration);
        _isRunning = false;
        _animationType.SetAnimationType(Act.idle, _animationType.GetDir());
        _playerRenderer.Play(_animationType);
    }
}
