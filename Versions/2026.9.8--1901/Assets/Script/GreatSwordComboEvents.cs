using UnityEngine;

/// <summary>
/// GreatSword Boss 连段动画事件接收器。
/// 每个攻击动画在“伤害帧刚结束”的位置都打了一个 RequestNextAttack 事件，
/// 本方法把事件传来的 index 写入 Animator 的 AttackIndex 参数，
/// 由状态机的 Equals 条件驱动过渡，实现“后摇取消 → 立刻接下一招前摇”。
/// </summary>
public class GreatSwordComboEvents : MonoBehaviour
{
    private Animator animator;
    private int attackIndexHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attackIndexHash = Animator.StringToHash("AttackIndex");
    }

    public void RequestNextAttack(int index)
    {
        if (animator != null)
        {
            animator.SetInteger(attackIndexHash, index);
        }
    }

    /// <summary>
    /// 伤害帧开始占位：由动画事件调用，后续在这里开启武器碰撞判定。
    /// </summary>
    public void OnDamageStart()
    {
        // TODO: 接入伤害系统（开启 hitbox / 进入可造成伤害状态）
    }

    /// <summary>
    /// 伤害帧结束占位：由动画事件调用，后续在这里关闭武器碰撞判定。
    /// </summary>
    public void OnDamageEnd()
    {
        // TODO: 接入伤害系统（关闭 hitbox / 退出可造成伤害状态）
    }
}