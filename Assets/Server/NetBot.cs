using Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetBot {

    public static PlayerInput ThinkForCar(BaseCar car)
    {
        if(car == null)
        {
            return PlayerInput.None;
        }
        Vector3 carPos = car.thisTransform.position;
        Vector3 carFwd = car.thisTransform.forward;

        PlayerInput input = new PlayerInput();
        Gateway targetGate = car.tracking.gate;
        Vector3 targetPos = targetGate.GetClosestPoint(carPos);

        Vector3 dirToTarget = targetPos - carPos;
        float fwdDot = Vector3.Dot(dirToTarget, carFwd);
        float rightDot = Vector3.SignedAngle(car.thisTransform.forward, dirToTarget, car.thisTransform.up);

        input.steerInput = DoClamp01(rightDot, -15, 15);
        input.driveInput = DoClamp01(fwdDot, -1, 1);

        if (car.canFlip)
        {
            input.useInput = 1f;
        }
        else
        {
            input.useInput = 0f;
        }

        return input;

    }

    private static float DoClamp01(float val, float min, float max)
    {
        if (val < min)
        {
            return -1;
        }
        if (val > max)
        {
            return 1;
        }
        return 0;
    }
}
