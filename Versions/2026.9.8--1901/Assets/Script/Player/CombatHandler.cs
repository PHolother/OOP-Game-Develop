using UnityEngine;
using Script.Base.Interface.Battle;

/// <summary>
/// 战斗事件处理器
/// 接收HitboxController的OnHit事件，处理伤害应用、音效、卡肉、镜头摇晃
/// 挂载在 Player 上
/// </summary>
public class CombatHandler : MonoBehaviour
{
    [Header("音效引用")]
    [Tooltip("命中音效")]
    public AudioClip hitSound;

    [Header("卡肉配置")]
    [Tooltip("卡肉持续时间（秒）")]
    public float hitStopDuration = 0.08f;

    [Header("镜头摇晃配置")]
    [Tooltip("命中时镜头摇晃强度倍率（1=默认强度）")]
    public float cameraShakeIntensity = 1f;

    [Header("引用")]
    [Tooltip("镜头摇晃控制器（留空则自动查找 Player Camara）")]
    [SerializeField] private CameraShakeController cameraShakeController;

    private void Start()
    {
        if (cameraShakeController == null)
        {
            var cameraObj = GameObject.Find("Player Camara");
            if (cameraObj != null)
                cameraShakeController = cameraObj.GetComponent<CameraShakeController>();
        }
    }

    /// <summary>
    /// HitboxController的OnHit事件绑定此方法
    /// 参数：attacker, battleAttribute, targetRoot, damage
    /// </summary>
    public void HandleHit(GameObject attacker, GameObject battleAttribute, GameObject target, int damage)
    {
        // 1. 应用伤害到目标
        ApplyDamage(target, damage);

        // 2. 播放命中音效
        PlayHitSound();

        // 3. 触发卡肉
        if (HitstopManager.Instance != null)
            HitstopManager.Instance.TriggerHitstop(hitStopDuration);

        // 4. 触发镜头摇晃
        if (cameraShakeController != null)
            cameraShakeController.TriggerShake(cameraShakeIntensity);
    }

    /// <summary>
    /// 对目标应用伤害
    /// </summary>
    private void ApplyDamage(GameObject target, int damage)
    {
        IDamageable damageable = target.GetComponentInChildren<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            return;
        }

        target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// 播放命中音效
    /// </summary>
    private void PlayHitSound()
    {
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}
