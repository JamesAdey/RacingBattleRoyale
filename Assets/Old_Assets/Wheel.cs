using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WheelType
{
    front,
    rear
}

public class Wheel : MonoBehaviour
{
    public WheelType wheelType;
    public Transform wheelGraphic;

    Transform thisTransform;
    BaseCar car;

    public Vector3 steerAxis = Vector3.up;
    public Vector3 turnAxis = Vector3.right;

    public float radius = 0.5f;
    float speed = 0f;
    //float wheelRot;

    public float angle = 0f;

    public LayerMask suspensionLayers;
    public float suspensionDist = 0.75f;
    float floorDist = 1;

    public bool isGrounded;

    // Use this for initialization
    void Awake()
    {
        if (!wheelGraphic)
        {
            Debug.LogError("WHEEL WITH NO GRAPHICS!!");
            return;
        }
        car = this.GetComponentInParent<BaseCar>();
        if (!car)
        {
            Debug.LogError("WHEEL WITH NO CAR!!");
            return;
        }
        thisTransform = this.transform;

        // auto fill the cars wheel arrays
        if (wheelType == WheelType.front)
        {
            car.AddFrontWheel(this);
        }
        else
        {
            car.AddRearWheel(this);
        }
    }

    // Update is called once per frame

    void FixedUpdate()
    {
        Vector3 up = thisTransform.up;
        Vector3 pos = thisTransform.position;
        RaycastHit hit;
        if (Physics.Raycast(pos, -up, out hit, suspensionDist, suspensionLayers))
        {
            floorDist = hit.distance - radius;
            isGrounded = true;
        }
        else
        {
            floorDist = suspensionDist - radius;
            isGrounded = false;
        }

        Vector3 wheelPos = pos - (up * floorDist);
        wheelGraphic.position = wheelPos;
        thisTransform.localEulerAngles = steerAxis * angle;

        float circumference = Mathf.PI * 2 * radius;
        float wheelRot = speed * circumference;

        Quaternion rot = wheelGraphic.rotation;
        // rotation from moving
        Quaternion movementRot = Quaternion.Euler(turnAxis * wheelRot);
        wheelGraphic.rotation = rot * movementRot;
    }

    internal void SetData(float currentSpeed, float ang)
    {
        this.speed = currentSpeed;
        this.angle = ang;
    }
}
