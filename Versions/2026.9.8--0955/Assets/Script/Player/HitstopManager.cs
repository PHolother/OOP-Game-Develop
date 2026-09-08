using UnityEngine;

/// <summary>
/// 卡肉（Hitstop）管理器 — 命中时冻结玩家逻辑和动画，营造打击感
/// 方案3：状态锁 + 帧跳过
/// 其他脚本在 Update 开头检查 IsFrozen，为 true 则跳过本帧逻辑
/// </summary>
public class HitstopManager : MonoBehaviour
{
    public static HitstopManager Instance { get; private set; }

    [Header("卡肉参数")]
    [Tooltip("卡肉持续时间（秒），建议 0.05-0.12")]
    [SerializeField] private float duration = 0.08f;

    [Tooltip("需要冻结动画的 Animator 列表（留空则自动查找玩家 Animator）")]
    [SerializeField] private Animator[] frozenAnimators;

    private float timer;
    private float currentDuration;
    private bool isActive;

    private float[] originalSpeeds;

    private void Awake()
    {
        Instance = this;

        if (frozenAnimators == null || frozenAnimators.Length == 0)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var anim = player.GetComponent<Animator>();
                if (anim != null)
                    frozenAnimators = new[] { anim };
            }
        }

        if (frozenAnimators != null)
            originalSpeeds = new float[frozenAnimators.Length];
    }

    private void Update()
    {
        if (!isActive) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= currentDuration)
        {
            RestoreAnimators();
            isActive = false;
        }
    }

    /// <summary>
    /// 触发卡肉，使用 Inspector 配置的默认持续时间
    /// </summary>
    public void TriggerHitstop()
    {
        TriggerHitstop(duration);
    }

    /// <summary>
    /// 触发卡肉，指定自定义持续时间
    /// </summary>
    public void TriggerHitstop(float customDuration)
    {
        currentDuration = customDuration;
        FreezeAnimators();
        timer = 0f;
        isActive = true;
    }

    /// <summary>
    /// 当前是否处于卡肉冻结状态
    /// </summary>
    public bool IsFrozen => isActive;

    private void FreezeAnimators()
    {
        if (frozenAnimators == null) return;

        for (int i = 0; i < frozenAnimators.Length; i++)
        {
            if (frozenAnimators[i] != null)
            {
                originalSpeeds[i] = frozenAnimators[i].speed;
                frozenAnimators[i].speed = 0f;
            }
        }
    }

    private void RestoreAnimators()
    {
        if (frozenAnimators == null) return;

        for (int i = 0; i < frozenAnimators.Length; i++)
        {
            if (frozenAnimators[i] != null)
                frozenAnimators[i].speed = originalSpeeds[i];
        }
    }
}
