using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 相机跟随代理 — 实现速度感知的水平位置滞后
/// Proxy 仅在 XZ 平面滞后，Y 轴直接同步玩家高度
/// FreeLook.Follow 和 LookAt 均设为此 GameObject
/// </summary>
public class CameraFollowProxy : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("玩家 Transform（自动通过 Tag 查找）")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("移动输入 Action（检测玩家是否在移动）")]
    [SerializeField] private InputActionReference moveAction;

    [Header("滞后参数")]
    [Tooltip("最大水平滞后距离（硬钳制，Proxy 永不超过此距离）")]
    [SerializeField] private float maxLagDistance = 1.1f;

    [Tooltip("慢速跟随系数（滞后阶段，值越小相机越慢）")]
    [SerializeField] private float slowFollowFactor = 1.5f;

    [Tooltip("快速跟随系数（阈值后加速追上，值越大追得越快）")]
    [SerializeField] private float fastFollowFactor = 10f;

    [Tooltip("回正系数（玩家停止后回到玩家位置的速度）")]
    [SerializeField] private float recenterFactor = 4f;

    [Tooltip("判定玩家停止移动的输入阈值")]
    [Range(0f, 0.5f)]
    [SerializeField] private float moveDeadzone = 0.1f;

    [Tooltip("单帧位移超过此值判定为瞬移/闪避，直接吸附")]
    [SerializeField] private float teleportThreshold = 3f;

    private Vector3 lastPlayerXZ;

    private void Start()
    {
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (moveAction != null && moveAction.action != null)
            moveAction.action.Enable();

        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
            lastPlayerXZ = Flatten(playerTransform.position);
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 currentPlayerXZ = Flatten(playerTransform.position);

        // 瞬移/闪避检测
        float frameMove = Vector3.Distance(currentPlayerXZ, lastPlayerXZ);
        if (frameMove > teleportThreshold)
        {
            SnapToPlayer();
            lastPlayerXZ = currentPlayerXZ;
            return;
        }

        Vector3 proxyXZ = Flatten(transform.position);
        Vector3 targetXZ = currentPlayerXZ;
        float horizontalDistance = Vector3.Distance(proxyXZ, targetXZ);

        bool isMoving = IsPlayerMoving();

        float factor;
        if (isMoving)
        {
            float t = Mathf.Clamp01(horizontalDistance / maxLagDistance);
            factor = Mathf.Lerp(slowFollowFactor, fastFollowFactor, t);
        }
        else
        {
            factor = recenterFactor;
        }

        // 平滑插值（帧率无关）
        Vector3 newXZ = Vector3.Lerp(proxyXZ, targetXZ, 1f - Mathf.Exp(-factor * Time.deltaTime));

        // 硬钳制：Proxy 与玩家的水平距离永不超过 maxLagDistance
        Vector3 toPlayer = currentPlayerXZ - newXZ;
        if (toPlayer.magnitude > maxLagDistance)
        {
            newXZ = currentPlayerXZ - toPlayer.normalized * maxLagDistance;
        }

        float newY = playerTransform.position.y;
        transform.position = new Vector3(newXZ.x, newY, newXZ.z);

        lastPlayerXZ = currentPlayerXZ;
    }

    public void SnapToPlayer()
    {
        if (playerTransform == null) return;
        transform.position = playerTransform.position;
    }

    private bool IsPlayerMoving()
    {
        if (moveAction == null || moveAction.action == null) return false;
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        return input.magnitude > moveDeadzone;
    }

    private static Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
