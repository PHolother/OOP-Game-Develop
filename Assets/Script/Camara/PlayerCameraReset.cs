using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraReset : MonoBehaviour
{
    private CinemachineFreeLook freeLook;
    private Transform playerTransform;
    private TargetingSystem targetingSystem;
    private Camera mainCam;

    private bool isResetting;
    private float resetTimer;
    private float startX, startY, targetX;

    [SerializeField] private float targetY = 0.5f;
    [SerializeField] private float resetSpeed = 8f;
    [SerializeField] private InputActionReference lookAction;

    [Header("索敌取景")]
    [Tooltip("开始修正的阈值（距屏幕中心的比例，0.3=距中心30%以外开始修正）")]
    [SerializeField] private float triggerRadius = 0.3f;
    [Tooltip("停止修正的阈值（进入此范围后停止修正，避免边界抖动）")]
    [SerializeField] private float deadRadius = 0.15f;
    [Tooltip("取景修正角速度（度/秒）")]
    [SerializeField] private float framingSpeed = 60f;
    [Tooltip("屏幕坐标平滑系数（越大越平滑）")]
    [SerializeField] private float screenSmooth = 12f;
    [Tooltip("此距离内不进行垂直修正（防止近身抖动）")]
    [SerializeField] private float verticalDisableDist = 4f;
    [Tooltip("垂直修正比例（相对水平修正的倍率）")]
    [SerializeField] private float verticalRatio = 0.3f;
    [Tooltip("Y轴钳制下限")]
    [SerializeField] private float yMin = 0.2f;
    [Tooltip("Y轴钳制上限")]
    [SerializeField] private float yMax = 0.8f;

    private Vector2 smoothedScreen;

    private void Start()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        targetingSystem = playerTransform.GetComponent<TargetingSystem>();
    }

    void Update()
    {
        if (targetingSystem == null)
            GetMiddleButton();
        HandleCameraControl();
    }

    private void LateUpdate()
    {
        if (isResetting) return;
        if (targetingSystem == null || !targetingSystem.HasTarget) return;
        ApplyFraming();
    }

    private void GetMiddleButton()
    {
        if (Mouse.current?.middleButton.wasPressedThisFrame == true)
        {
            StartResetCamera();
        }
    }

    private void StartResetCamera()
    {
        var proxy = freeLook.Follow != null ? freeLook.Follow.GetComponent<CameraFollowProxy>() : null;
        if (proxy != null)
            proxy.SnapToPlayer();

        targetX = CalculateTargetX();
        isResetting = true;
        resetTimer = 0f;
        startX = freeLook.m_XAxis.Value;
        startY = freeLook.m_YAxis.Value;
    }

    private void UpdateResetCamera()
    {
        resetTimer += Time.deltaTime * resetSpeed;

        freeLook.m_XAxis.Value = Mathf.Lerp(startX, targetX, resetTimer);
        freeLook.m_YAxis.Value = Mathf.Lerp(startY, targetY, resetTimer);

        if (resetTimer >= 1f)
        {
            isResetting = false;
            freeLook.m_XAxis.Value = targetX;
            freeLook.m_YAxis.Value = targetY;
        }
    }

    private void HandleCameraControl()
    {
        if (!isResetting)
        {
            var lookDelta = lookAction.action.ReadValue<Vector2>();
            freeLook.m_XAxis.m_InputAxisValue = lookDelta.x;
            freeLook.m_YAxis.m_InputAxisValue = lookDelta.y;
        }
        else
        {
            UpdateResetCamera();
        }
    }

    public void ResetCamera()
    {
        StartResetCamera();
    }

    /// <summary>
    /// 锁定索敌时，当敌人偏离屏幕中心超过阈值，微调 FreeLook 轨道角度。
    /// 带平滑、滞回区、距离感知，防止近身抖动。
    /// </summary>
    private void ApplyFraming()
    {
        if (mainCam == null)
            mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 enemyPos = targetingSystem.CurrentTarget.position;
        Vector3 screenPos = mainCam.WorldToScreenPoint(enemyPos);

        if (screenPos.z <= 0) return;

        // 归一化到 [-1, 1]，中心为 0
        float rawX = (screenPos.x / Screen.width) * 2f - 1f;
        float rawY = (screenPos.y / Screen.height) * 2f - 1f;
        Vector2 raw = new Vector2(rawX, rawY);

        // 指数平滑，消除帧间抖动
        float alpha = 1f - Mathf.Exp(-screenSmooth * Time.deltaTime);
        smoothedScreen = Vector2.Lerp(smoothedScreen, raw, alpha);

        float dist = smoothedScreen.magnitude;
        if (dist < deadRadius) return;

        // 滞回：只有超出 triggerRadius 才开始修正，进入 deadRadius 后停止
        // 在两者之间维持上一次的状态（不主动修正也不主动停止）
        float intensity = Mathf.InverseLerp(deadRadius, triggerRadius, dist);
        intensity = Mathf.Clamp01(intensity);

        if (intensity <= 0f) return;

        float maxCorrection = framingSpeed * Time.deltaTime * intensity;

        // 水平修正
        float xCorrection = smoothedScreen.x * maxCorrection;
        freeLook.m_XAxis.Value += xCorrection;

        // 垂直修正：距离过近时禁用，防止抖动
        float playerEnemyDist = Vector3.Distance(playerTransform.position, enemyPos);
        if (playerEnemyDist > verticalDisableDist)
        {
            float yCorrection = -smoothedScreen.y * maxCorrection * verticalRatio;
            freeLook.m_YAxis.Value = Mathf.Clamp(freeLook.m_YAxis.Value + yCorrection, yMin, yMax);
        }
    }

    private float CalculateTargetX()
    {
        var playerBackDirection = -playerTransform.forward;
        playerBackDirection.y = 0;
        playerBackDirection.Normalize();

        var cameraDirection = freeLook.transform.position - playerTransform.position;
        cameraDirection.y = 0;
        cameraDirection.Normalize();

        var deltaAngle = Vector3.SignedAngle(cameraDirection, playerBackDirection, Vector3.up);

        var tempAngle = freeLook.m_XAxis.Value + deltaAngle;

        while (tempAngle > 180) tempAngle -= 360;
        while (tempAngle < -180) tempAngle += 360;

        return tempAngle;
    }
}
