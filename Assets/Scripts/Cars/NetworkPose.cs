using UnityEngine;

public struct NetworkPose
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float time;

    public NetworkPose(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, float time)
    {
        this.position = pos;
        this.rotation = rot;
        this.velocity = vel;
        this.angularVelocity = angVel;
        this.time = time;
    }

    public static readonly NetworkPose identity = new NetworkPose(Vector3.zero, Quaternion.identity, Vector3.zero, Vector3.zero, 0f);
}