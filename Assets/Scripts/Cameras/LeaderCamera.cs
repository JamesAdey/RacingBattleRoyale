using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderCamera : MonoBehaviour {

    private Camera cam;
    public Transform target;
    public Vector3 viewOffset;
    private Transform thisTransform;

    public float distance = 5;
    public float height = 1.5f;
    public float speed = 5;
    private Vector3 targetPos;

    // Use this for initialization
    void Start()
    {
        thisTransform = this.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (BRGameController.gameOver)
        {
            target = null;
            return;
        }
        BaseCar c = BRGameController.GetCurrentLeader();
        
        if (!c)
        {
            return;
        }
        target = c.transform;


        targetPos = target.position - target.forward * distance + Vector3.up * height;

        thisTransform.position = Vector3.Lerp(thisTransform.position, targetPos, Time.deltaTime * speed);
        Vector3 targetDir = target.position - thisTransform.position;
        Quaternion desiredRot = Quaternion.LookRotation(targetDir, Vector3.up);
        thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, desiredRot, Time.deltaTime * speed);
    }
}
