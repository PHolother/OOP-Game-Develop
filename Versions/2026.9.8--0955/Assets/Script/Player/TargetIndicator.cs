using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;
    private SpriteRenderer sr;
    private Camera mainCam;

    public void SetTarget(Transform t, Vector3 indicatorOffset)
    {
        target = t;
        offset = indicatorOffset;

        // Parent to target so it auto-follows
        if (target != null)
        {
            transform.SetParent(target, false);
            transform.localPosition = offset;
        }
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("TargetCircle");
        sr.color = new Color(1f, 0f, 0f, 0.85f);
        sr.sortingOrder = 10000;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam != null)
            transform.rotation = mainCam.transform.rotation;

        // Keep consistent screen size
        float dist = Vector3.Distance(mainCam != null ? mainCam.transform.position : Vector3.zero, transform.position);
        float scale = 0.15f * Mathf.Max(dist, 1f);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
