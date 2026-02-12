using UnityEngine;

public class AuraFollowPlayer : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Init(Transform followTarget, Vector3 followOffset)
    {
        target = followTarget;
        offset = followOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
