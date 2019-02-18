using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTurret : MonoBehaviour
{

    public Vector3 lookAtPos;
    BaseCar attachedCar;
    Transform thisTransform;
    public Transform barrelHinge;

    public ParticleThruster[] thrusters;

    public float turnSpeed = 180;

    bool doThrust;

    [SerializeField]
    private float minBarrelRot = -50;
    [SerializeField]
    private float maxBarrelRot = 10;

    float yawInput;
    float pitchInput;

    float desiredYaw;
    float desiredPitch;

    Vector3 desiredTurretAngles = Vector3.zero;
    Vector3 desiredBarrelAngles = Vector3.zero;

    void Start()
    {
        attachedCar = GetComponentInParent<BaseCar>();
        thisTransform = this.transform;
    }



    void FixedUpdate()
    {
        GetInputs();
        UpdateThrusters();

        desiredTurretAngles.y += yawInput * turnSpeed * Time.deltaTime;
        if (desiredTurretAngles.y > 360)
        {
            desiredTurretAngles.y -= 360;
        }
        else if (desiredTurretAngles.y < 0)
        {
            desiredTurretAngles.y += 360;
        }

        Vector3 lookDir = (lookAtPos - barrelHinge.position);
        //lookDir = thisTransform.InverseTransformDirection(lookDir);
        Quaternion lookRot = Quaternion.LookRotation(lookDir,attachedCar.transform.up);

        Vector3 desiredEulers = lookRot.eulerAngles;

        desiredBarrelAngles.x = desiredEulers.x;
        desiredTurretAngles = thisTransform.localEulerAngles;
        desiredTurretAngles.y = desiredEulers.y;

        barrelHinge.localEulerAngles = desiredBarrelAngles;
        thisTransform.localEulerAngles = desiredTurretAngles;

    }

    void GetInputs()
    {

    }

    void UpdateThrusters()
    {
        for (int i = 0; i < thrusters.Length; i++)
        {
            thrusters[i].SetThruster(doThrust);
            if (doThrust)
            {
                attachedCar.AddThrustForce(thrusters[i].GetTransform(), thrusters[i].force);
            }
        }
    }

}
