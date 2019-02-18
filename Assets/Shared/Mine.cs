using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : TempNetBehaviour
{

    [SerializeField]
    private Transform mineGfx;

    [SerializeField]
    private LayerMask groundLayers;

    

    [SerializeField]
    private LayerMask explosionLayers;
    [SerializeField]
    private float explosionRadius = 4;
    [SerializeField]
    private float explosionPower = 30;

    const float FALL_HEIGHT = 200;
    const float FALL_TIME = 2;
    const float ARM_TIME = 2;

    bool IsPrimed {
        get {
            return Time.time > primedTime;
        }
    }

    private float primedTime = 0;
    private float fallTime = 0;
    private Vector3 dropStart;

    // Use this for initialization
    void Start()
    {
        primedTime = Time.time + FALL_TIME + ARM_TIME;
        fallTime = Time.time + FALL_TIME;
        RaycastHit hit;
        if(Physics.Raycast(thisTransform.position, -thisTransform.up, out hit, Mathf.Infinity, groundLayers))
        {
            thisTransform.position = hit.point;
        }

        dropStart = thisTransform.position + Vector3.up * FALL_HEIGHT;
        Debug.DrawLine(thisTransform.position, dropStart, Color.red, 1);

    }

    // Update is called once per frame
    void Update()
    {
        // s = ut + 1/2 a t ^2
        // 1000 = 4u - 4.9 * 16

        float t = (fallTime - Time.time)/FALL_TIME;

        mineGfx.position = Vector3.Lerp(thisTransform.position, dropStart, t);


    }

    void OnTriggerEnter(Collider other)
    {
        if (NetworkCore.isServer && IsPrimed)
        {
            Debug.Log("TRIGGERED!!!");
            Explode();

            ItemManager.singleton.RemoveTempNetObject(this);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    void Explode()
    {
        Utils.ExplosionForce(thisTransform.position, explosionRadius, explosionPower, explosionLayers);
    }
}
