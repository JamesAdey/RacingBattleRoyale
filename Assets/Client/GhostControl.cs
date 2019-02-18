using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostControl : MonoBehaviour {

    [SerializeField]
    private MeshRenderer[] renderers;

    [SerializeField]
    private Collider[] colliders;

    [SerializeField]
    private Canvas canvas;

    public void SetGhostMode(bool on)
    {
        if (on)
        {
            Material mat = Customisation.GetGhostMaterial();
            int ghostLayer = LayerMask.NameToLayer("Ghost");
            ApplyMaterialLayer(mat, ghostLayer);
        }
        else
        {
            Material mat = Customisation.GetRealMaterial();
            int realLayer = LayerMask.NameToLayer("Ignore Raycast");
            ApplyMaterialLayer(mat, realLayer);
        }
    }

    void ApplyMaterialLayer(Material mat, int layerNum)
    {
        foreach (MeshRenderer rend in renderers)
        {
            rend.sharedMaterial = mat;
            rend.gameObject.layer = layerNum;
        }
        foreach (Collider col in colliders)
        {
            col.gameObject.layer = layerNum;
        }
    }
}
