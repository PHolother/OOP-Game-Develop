using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerDodge : MonoBehaviour
{
    private Animator animator;
    private IMovementProvider movementProvider;

    [Header("闪避")]
    private bool isDodging;
    private bool isInputBuffered;
    private float inputBufferedStartTime;
    
    public float inputBufferedDuration = 0.35f;
    public int maxDodgeCount = 2;
    private int dodgeCount = 0;
    private float nextDodgeTime;
    public float dodgeCooldown = 1.5f;
    
    private int dodgeHash;
    void Start()
    {
        animator = GetComponent<Animator>();
        movementProvider = GetComponent<IMovementProvider>();
        dodgeHash = Animator.StringToHash("isDodging");
    }
    
    void Update()
    {
        CheckInputBufferTimeout();
    }
    
    public void GetShift(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        if (Time.time < nextDodgeTime) return;
        
        // 已经在闪避
        if (isDodging)
        {
            isInputBuffered = true; // 1. 闪避过程中又按了一次 shift
            inputBufferedStartTime = Time.time;
            return;
        }
        StartDodge();
    }
    private void CheckInputBufferTimeout()
    {
        if(isInputBuffered && Time.time >= inputBufferedStartTime + inputBufferedDuration) isInputBuffered = false;
    }

    private void StartDodge()
    {
        // 3. 平滑过渡动画, isInputBuffered 表示来自二次闪避
        if (isInputBuffered)
        {
            // CrossFadeInFixedTime 平滑过渡播放
            animator.CrossFadeInFixedTime("DodgeToSprinting", 13/41f);
        }
        
        isDodging = true;
        animator.SetBool(dodgeHash,isDodging);
        dodgeCount++;

        movementProvider.SetSprintState(true);
    }

    // 闪避动画结束调用
    public void OnDodgeEnd()
    {
        //预输入判断能否闪避
        if (isInputBuffered && dodgeCount < maxDodgeCount)
        {
            
            // 2. 注意此时先不把 isInputBuffered = false
            StartDodge();
            isInputBuffered = false;
            return;
        }

        isInputBuffered = false;
        isDodging = false;
        animator.SetBool(dodgeHash,isDodging); //没有预输入就直接回到移动混合树
        //次数用完，闪避进入冷却
        if (dodgeCount < maxDodgeCount) return;
        nextDodgeTime = Time.time + dodgeCooldown;
        dodgeCount = 0;
    }
}
