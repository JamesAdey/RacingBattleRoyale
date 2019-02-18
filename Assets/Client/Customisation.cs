using System;
using UnityEngine;

internal class Customisation : MonoBehaviour
{
    [SerializeField]
    private Material ghostMaterial;
    [SerializeField]
    private Material carMaterial;

    private static Customisation singleton;

    void Awake()
    {
        singleton = this;
    }

    internal static Material GetGhostMaterial()
    {
        if(singleton == null)
        {
            return null;
        }
        return singleton.ghostMaterial;
    }

    internal static Material GetRealMaterial()
    {
        if (singleton == null)
        {
            return null;
        }
        return singleton.carMaterial;
    }
}