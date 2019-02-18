using Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : CarInputs
{

    public Gateway targetGate;
    public Vector3 targetPos;
	private XBoxCtrlInputs inputs = null;
    private Transform thisTransform;

    // Use this for initialization
    void Start ()
	{
        thisTransform = this.transform;
        targetGate = NodeGraph.GetNearestGateway(thisTransform);
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		if (inputs == null) {
			return;
		}
        targetGate = car.tracking.gate;
        inputs.leftTrigger = 1;
        targetPos = targetGate.GetClosestPoint(thisTransform.position);
        
        Vector3 dirToTarget = targetPos - thisTransform.position;
        float fwdDot = Vector3.Dot(dirToTarget, thisTransform.forward);
        float rightDot = Vector3.SignedAngle(thisTransform.forward, dirToTarget, thisTransform.up);

        inputs.leftStickX = DoClamp01(rightDot, -15, 15);
        inputs.leftStickY = DoClamp01(fwdDot, -1, 1);

        if (car.canFlip)
        {
            inputs.yButton = true;
        }
        else
        {
            inputs.yButton = false;
        }

    }

    private float DoClamp01(float val, float min, float max)
    {
        if(val < min)
        {
            return -1;
        }
        if(val > max)
        {
            return 1;
        }
        return 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPos);
        Gizmos.DrawWireSphere(targetPos, 0.5f);
    }

	public override XBoxCtrlInputs GetInputs ()
	{
		if (inputs == null) {
			inputs = new XBoxCtrlInputs (-1);
		}
		return inputs;
	}
}
