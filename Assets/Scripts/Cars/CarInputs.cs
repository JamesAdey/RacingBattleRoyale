using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CarInputs : MonoBehaviour
{
    protected BaseCar car;

    public void SetCar(BaseCar c)
    {
        car = c;
    }

    public abstract XBoxCtrlInputs GetInputs();
}