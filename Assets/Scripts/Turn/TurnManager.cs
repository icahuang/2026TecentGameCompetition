using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 场景一份，管理回合的进行。场景里那一个可以用 <see cref="Instance"/> 拿到。
/// 玩家 <c>TryStep</c> 成功后通知这里，再让每个敌人走一步。
/// 撞墙只转身不通知。抓住玩家时显示场景里的 Reset 按钮，重载场景重启这一局。
/// </summary>
public class TurnManager : MonoBehaviour
{
    [SerializeField] private Button _resetButton;
    [SerializeField] private float _stepDuration = 0.25f;

    private readonly List<EnemyController> _enemies = new List<EnemyController>();
    private bool _caught;

    private static TurnManager _instance;

    public static TurnManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<TurnManager>();
            return _instance;
        }
    }

    /// <summary>走一格用几秒。所有角色统一读这一个值，别在 prefab 上各存一份。</summary>
    public float StepDuration => _stepDuration;

    private void Awake()
    {
        // 放在下面那个提前 return 之前：没配 Reset 按钮也照样算「场景里那一个」。
        _instance = this;

        if (_resetButton == null) return;
        _resetButton.gameObject.SetActive(false);
        _resetButton.onClick.RemoveListener(RestartRound);
        _resetButton.onClick.AddListener(RestartRound);
    }

    private void OnDestroy()
    {
        // 只有自己仍是当前实例时才清空。重载场景时新旧会交替，别把新的那个抹掉。
        if (_instance == this) _instance = null;

        if (_resetButton != null)
            _resetButton.onClick.RemoveListener(RestartRound);
    }

    // 用于EnemyController脚本自己把自己添加进list里
    public void Register(EnemyController enemy)
    {
        if (enemy != null && !_enemies.Contains(enemy))
            _enemies.Add(enemy);
    }
    // 用于EnemyController脚本自己把自己从list里移除
    public void Unregister(EnemyController enemy)
    {
        _enemies.Remove(enemy);
    }

    public void NotifyPlayerStepped(GridActor player)
    {
        if (player == null || _caught) return;
        for (int i = 0; i < _enemies.Count; i++)
        {
            EnemyController enemy = _enemies[i];
            if (enemy != null) enemy.TakeTurn(player);
        }
    }

    public void CatchPlayer(GridActor player)
    {
        if (_caught) return;
        _caught = true;
        Debug.Log("游戏失败：被敌人抓住了。");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
        }

        if (_resetButton != null)
            _resetButton.gameObject.SetActive(true);
    }

    public void RestartRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
