using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardBrain : CarInputs {

    private XBoxCtrlInputs inputs = null;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (inputs == null)
        {
            return;
        }
        inputs.leftStickY = Input.GetAxis("Vertical");
        inputs.leftStickX = Input.GetAxis("Horizontal");
        inputs.leftTrigger = Mathf.Abs(Input.GetAxis("Vertical"));
        inputs.yButton = Input.GetKey(KeyCode.R);
        //inputs.yButton = Input.GetKey(KeyCode.Joystick1Button4);
    }

    public override XBoxCtrlInputs GetInputs()
    {
        if (inputs == null)
        {
            inputs = new XBoxCtrlInputs(-1);
        }
        return inputs;
    }


    private void OnDrawGizmos()
    {
        if (car != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, car.tracking.gate.position);
            Gizmos.DrawWireSphere(car.tracking.gate.position, 0.5f);
        }
    }
}
