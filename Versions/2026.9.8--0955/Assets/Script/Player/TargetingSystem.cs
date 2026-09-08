using Script.Base.Interface.Battle;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem : MonoBehaviour
{
    [Header("索敌范围设置")]
    [Tooltip("自动索敌检测距离")]
    [SerializeField] private float autoDetectRange = 25f;

    [Tooltip("超出此距离丢失目标")]
    [SerializeField] private float loseTargetRange = 35f;

    [Tooltip("自动索敌检测间隔（秒）")]
    [SerializeField] private float autoDetectInterval = 0.3f;

    [Header("指示器设置")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0, 2f, 0);

    public Transform CurrentTarget { get; private set; }
    public bool HasTarget => CurrentTarget != null;

    private bool hasAutoLocked;
    private TargetIndicator activeIndicator;
    private float autoDetectTimer;

    private void Update()
    {
        if (HitstopManager.Instance != null && HitstopManager.Instance.IsFrozen) return;

        if (HasTarget)
        {
            if (!ValidateTarget())
            {
                ClearTarget();
                return;
            }
        }
        else if (!hasAutoLocked)
        {
            autoDetectTimer -= Time.deltaTime;
            if (autoDetectTimer <= 0f)
            {
                autoDetectTimer = autoDetectInterval;
                var nearest = FindNearestEnemy(autoDetectRange);
                if (nearest != null)
                    SetTarget(nearest, true);
            }
        }

        if (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame)
            ToggleManualLock();
    }

    public void HandleMiddleClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            ToggleManualLock();
    }

    private void ToggleManualLock()
    {
        if (HasTarget)
        {
            ClearTarget();
        }
        else
        {
            var nearest = FindNearestEnemy(autoDetectRange);
            if (nearest != null)
                SetTarget(nearest, true);
        }
    }

    private Transform FindNearestEnemy(float maxRange)
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        Transform nearest = null;
        float nearestSqrDist = maxRange * maxRange;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var damageable = enemy.GetComponentInChildren<IDamageable>();
            if (damageable == null || damageable.IsDead()) continue;

            float sqrDist = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private bool ValidateTarget()
    {
        if (CurrentTarget == null) return false;

        var damageable = CurrentTarget.GetComponentInChildren<IDamageable>();
        if (damageable == null || damageable.IsDead()) return false;

        float sqrDist = (CurrentTarget.position - transform.position).sqrMagnitude;
        if (sqrDist > loseTargetRange * loseTargetRange) return false;

        return true;
    }

    private void SetTarget(Transform target, bool manual)
    {
        CurrentTarget = target;
        hasAutoLocked = true;

        if (activeIndicator != null)
            Destroy(activeIndicator.gameObject);

        if (targetIndicatorPrefab != null)
        {
            var indicatorObj = Instantiate(targetIndicatorPrefab);
            activeIndicator = indicatorObj.GetComponent<TargetIndicator>();
            if (activeIndicator != null)
                activeIndicator.SetTarget(target, indicatorOffset);
        }
    }

    private void ClearTarget()
    {
        CurrentTarget = null;

        if (activeIndicator != null)
        {
            Destroy(activeIndicator.gameObject);
            activeIndicator = null;
        }

        autoDetectTimer = 0f;
    }
}
