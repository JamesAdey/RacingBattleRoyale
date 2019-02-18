using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour
{
    public float speed = 10;
    public float runSpeed = 40;
    public float xSensitivity = 1;
    public float ySensitivity = 1;

    public float minPitch = -85;
    public float maxPitch = 85;

    float yaw;
    float pitch;
    Vector3 eulers = Vector3.zero;

    private Transform thisTransform;
    

    // Use this for initialization
    void Start()
    {
        thisTransform = this.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 move = Vector3.zero;
        move.z = Input.GetAxis("Vertical");
        move.x = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            move *= runSpeed;
        }
        else
        {
            move *= speed;
        }
        thisTransform.Translate(move * Time.deltaTime, Space.Self);

        yaw += Input.GetAxis("Mouse X") * xSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * ySensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (yaw > 360)
        {
            yaw -= 360;
        }
        if (yaw < 0)
        {
            yaw += 360;
        }

        eulers.x = pitch;
        eulers.y = yaw;
        thisTransform.eulerAngles = eulers;
    }
}
