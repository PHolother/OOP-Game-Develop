using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraReset : MonoBehaviour
{
    private CinemachineFreeLook freeLook;
    
    private bool isResetting;
    private float resetTimer;
    private float startX;
    private float startY;
    
    [Header("回正设置")]
    [SerializeField] private float targetX = 0f;
    [SerializeField] private float targetY = 0.5f;
    [SerializeField] private float resetSpeed = 8f;
    
    [SerializeField] private InputActionReference lookAction;
    
    private void Start()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        GetMiddleButton();
        IfReset();
    }
    
    public void GetMiddleButton()
    {
        if (Mouse.current?.middleButton.wasPressedThisFrame == true)
        {
            StartResetCamera();
        }
    }

    private void StartResetCamera()
    {
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

        if (resetTimer < 1f) return;
        
        isResetting = false;
        freeLook.m_XAxis.Value = targetX;
        freeLook.m_YAxis.Value = targetY;
    }

    private void IfReset()
    {
        // 正常视角控制
        if (!isResetting)
        {
            Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
            freeLook.m_XAxis.m_InputAxisValue = lookDelta.x;
            freeLook.m_YAxis.m_InputAxisValue = lookDelta.y;
        }
        
        // 回正动画
        if (isResetting)
        {
            UpdateResetCamera();
        }
    }
}
