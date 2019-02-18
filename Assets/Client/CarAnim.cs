using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAnim : MonoBehaviour
{

    [SerializeField]
    private Vector3 maxAngles = new Vector3(0,0,5);
    
    [SerializeField]
    private float rate = 7;
    [SerializeField]
    private float smooth = 5;
    [SerializeField]
    private Transform body = null;

    private Vector3 angles;
    private Vector3 smoothAngles;

    private Rigidbody thisRigidbody;
    private Transform thisTransform;
    

    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        thisTransform = GetComponent<Transform>();
    }

    void FixedUpdate()
    {

        Vector3 angVel = thisTransform.TransformVector(thisRigidbody.angularVelocity);

        // angular velocity around our Y-Axis causes roll
        float t = Mathf.Lerp(0, maxAngles.z, Mathf.Abs(angVel.y) * rate);
        angles.z = t * Mathf.Sign(angVel.y);

        // angular velocity on our Z-Axis causes roll
        t = Mathf.Lerp(0, maxAngles.z, Mathf.Abs(angVel.z) * rate);
        angles.z += t * Mathf.Sign(angVel.z);

        // angular velocity on our X-Axis causes pitch
        t = Mathf.Lerp(0, maxAngles.x, Mathf.Abs(angVel.x) * smooth);
        angles.x = t * Mathf.Sign(-angVel.x);

        smoothAngles = Vector3.MoveTowards(smoothAngles, angles, smooth);

        // assign the angles
        body.localEulerAngles = smoothAngles;
    }


}
