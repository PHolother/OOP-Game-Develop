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
}