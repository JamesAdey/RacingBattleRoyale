using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCamera : MonoBehaviour
{

    private Camera cam;
    public Transform target;
    public Vector3 viewOffset;
    private Transform thisTransform;

    public float distance = 7;
    public float height = 3;
    public float speed = 5;
    private Vector3 targetPos;

    [SerializeField]
    private LayerMask ghostMask;
    [SerializeField]
    private LayerMask realMask;

    // Use this for initialization
    void Start()
    {
        thisTransform = this.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!target)
        {
            return;
        }
        targetPos = target.position - target.forward * distance + Vector3.up * height;

        thisTransform.position = Vector3.Lerp(thisTransform.position, targetPos, Time.deltaTime * speed);
        thisTransform.LookAt(target.position + (target.rotation * viewOffset));

    }

    void LateUpdate()
    {
        if (!target)
        {
            return;
        }
    }


    public void SetGhostMode(bool on)
    {
        if (on)
        {
            cam.cullingMask = ghostMask;
        }
        else
        {
            cam.cullingMask = realMask;
        }
    }

    public void SetCameraTarget(Transform t)
    {
        target = t;
    }

    public void SetCameraViewRect(Rect rect)
    {
        // get camera and assign
        cam = this.GetComponent<Camera>();
        cam.rect = rect;
    }
}
