using UnityEngine;

public class LookatSmooth : MonoBehaviour
{
    [Header("镜头跟随设置")]
    [SerializeField] private float smoothTime = 0.3f;
    private Vector3 targetWorldPosition;
    private float yOffset;
    private Vector3 velocity = Vector3.zero;
    void Start()
    {
        targetWorldPosition = transform.position;
        yOffset = transform.localPosition.y;
    }

    void Update()
    {
        targetWorldPosition = transform.parent.position + yOffset * Vector3.up;
    }

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position,targetWorldPosition,ref velocity,smoothTime);
    }
}
