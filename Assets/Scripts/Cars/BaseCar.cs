using Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct CarGameData
{
    public Powerup powerup;

    public void PackGameData(NetWriter writer)
    {
        writer.WriteByte(powerup.GetByteID());
    }
}

[System.Serializable]
public struct CarStats
{
    public float enginePower;
    public float boostPower;
    public float grip;
    public float driftGrip;
    public float boostGrip;

    public static CarStats Default = new CarStats(5f, 15f, 0.5f, 0.2f, 0.8f);

    public CarStats(float enginePower, float boostPower, float grip, float driftGrip, float boostGrip)
    {
        this.enginePower = enginePower;
        this.boostPower = boostPower;
        this.grip = grip;
        this.driftGrip = driftGrip;
        this.boostGrip = boostGrip;
    }
}

[System.Serializable]
public struct CarVisuals
{
    public Vector3 offset;
    public float steerAngle;
    public Text text;
    public int carNum;

    public CarVisuals(float steerAngle, int maxWheels)
    {
        this.steerAngle = steerAngle;
        offset = Vector3.zero;
        text = null;
        carNum = 0;
    }

    public static CarVisuals Default = new CarVisuals(30f, 4);

}

[System.Serializable]
public struct CarTracking
{
    public Gateway gate;
    public GatewayResult result;
    public float returnTime;
    public float gateBackTime;

    public float sqrDistToGate;
    public int gatesPassed;
    public float score;


    public void Init(Gateway gate)
    {
        this.gate = gate;
        gatesPassed = 0;
        sqrDistToGate = 0;
        score = 0;
    }
}

public class BaseCar : NetBehaviour, IDamageable
{

    internal Transform thisTransform;
    Rigidbody thisRigidbody;

    public float mul = 0f;
    public float currentSpeed = 0f;
    public float maxSpeed = 15;
    public float maxBoostSpeed = 30f;
    private float curMaxSpeed;
    public bool isGrounded;
    public bool canFlip;
    public int groundedCount;

    public GamePlayer pDriver;
    public RaceEntry entry = null;
    public CarStats stats = CarStats.Default;
    public CarVisuals visuals = CarVisuals.Default;
    public string debugMsg;
    public CarTracking tracking;

    public PlayerInput inputData;
    float steerInput;
    float driveInput;
    float accelerate;
    float boostSpeedPercentage;

    float groundedMod = 1;

    int vehicleNum;
    private int maxWheels;

    float lastGroundedTime;
    Quaternion flipRot;
    public float flipTime = 3f;
    readonly float mod = 10;
    private readonly float FALL_HEIGHT = -10;
    private float _score = 0f;


    public GhostControl pGhostControl;



    public float Score
    {
        get
        {
            return tracking.score;
        }
    }

    List<Wheel> frontWheels = new List<Wheel>();
    List<Wheel> rearWheels = new List<Wheel>();
    private float nextCheckTime;

    NetworkPose newPose = NetworkPose.identity;
    NetworkPose oldPose = NetworkPose.identity;

    Queue<NetworkPose> poseHistory = new Queue<NetworkPose>();
    private Vector3 startPos;
    private Quaternion startRot;

    public void AddFrontWheel(Wheel wh)
    {
        frontWheels.Add(wh);
        maxWheels++;
    }

    public void AddRearWheel(Wheel wh)
    {
        rearWheels.Add(wh);
        maxWheels++;
    }

    public void AddThrustForce(Transform thruster, float force)
    {
        if (boostSpeedPercentage < 1)
        {
            thisRigidbody.AddForce(thruster.forward * force, ForceMode.Acceleration);
        }
    }

    internal void ApplyVisuals()
    {
        visuals.text.text = visuals.carNum.ToString();
        gameObject.name = entry.racerName;
    }

    public void InitVehicle()
    {
        /*
        // if local?
        if (VehicleManager.vehicleData[vehicleNum].isLocal)
        {
            inputData = PlayerInputs.GetInputsForControllerNumber(VehicleManager.vehicleData[vehicleNum].localControllerNum);
            LocalPlayerManager.SetCarForController(VehicleManager.vehicleData[vehicleNum].localControllerNum, this);
        }
        else
        {
            // TODO
            // if server...
            // create an AI component
            AIBrain brain = gameObject.AddComponent<AIBrain>();
            brain.SetCar(this);
            inputData = brain.GetInputs();
        }
        */
    }

    public void SetVehicleNumber(int num)
    {

    }



    void GetInput()
    {
        inputData = pDriver.ConsumeNextInput();

        steerInput = inputData.steerInput;
        driveInput = inputData.driveInput;
        accelerate = Mathf.Abs(driveInput);     // TODO change this
    }

    void FixedUpdate()
    {
        CheckGates();

        GetInput();
        if (NetworkCore.isServer)
        {
            SimulateServerPhysics();
        }
        else
        {
            SimulateClientPhysics();
        }

        // update the wheels
        UpdateWheels();

    }

    void SimulateClientPhysics()
    {
        // compute delta
        float delta = (Time.time - newPose.time) / NetworkCore.networkUpdateDelay;
        // lerp poses
        NetworkPose lerpPose;
        lerpPose.position = Vector3.Lerp(thisRigidbody.position, newPose.position, delta);
        lerpPose.rotation = Quaternion.Lerp(thisRigidbody.rotation, newPose.rotation, delta);
        lerpPose.velocity = Vector3.Lerp(oldPose.velocity, newPose.velocity, delta);
        lerpPose.angularVelocity = Vector3.Lerp(oldPose.angularVelocity, newPose.angularVelocity, delta);

        thisRigidbody.MovePosition(lerpPose.position);
        thisRigidbody.MoveRotation(lerpPose.rotation);
        thisRigidbody.velocity = lerpPose.velocity;
        thisRigidbody.angularVelocity = lerpPose.angularVelocity;
    }

    void SimulateServerPhysics()
    {
        currentSpeed = thisRigidbody.velocity.magnitude;
        // do the important calculations
        float speedPercentage = currentSpeed / maxSpeed;
        float boostSpeedPercentage = currentSpeed / maxBoostSpeed;
        //float speedRadius = Mathf.Lerp (minSpeedRadius, maxSpeedRadius, speedPercentage);
        //float turnAngle = Mathf.Lerp (maxTurnAngle, minTurnAngle, speedPercentage);

        Vector3 relativeVel = thisTransform.InverseTransformVector(thisRigidbody.velocity);
        Vector3 relativeAngularVel = thisTransform.InverseTransformVector(thisRigidbody.angularVelocity);

        float driveDir = 1;
        if (driveInput < -0.1f)
        {
            driveDir = -1;
        }

        float turnDir = 1;
        /*if (relativeVel.z < -0.1f)
        {
            turnDir = -1;
        }*/

        if (isGrounded)
        {
            mul = (1 - speedPercentage) * stats.enginePower * driveDir * groundedMod * accelerate;
            thisRigidbody.AddRelativeForce(Vector3.forward * mul, ForceMode.Acceleration);


            if (currentSpeed > 0.3f)
            {
                thisRigidbody.AddRelativeTorque(Vector3.up * steerInput * turnDir * mod, ForceMode.Acceleration);
            }
            thisRigidbody.AddRelativeForce(Vector3.right * -relativeVel.x * stats.grip * mod, ForceMode.Acceleration);
            thisRigidbody.AddRelativeTorque(Vector3.up * -relativeAngularVel.y * stats.grip * mod, ForceMode.Acceleration);
        }
        else
        {
            if (thisRigidbody.position.y < FALL_HEIGHT)
            {
                Fallen();
            }
        }
    }

    private void CheckGates()
    {
        if (Time.time < nextCheckTime)
        {
            return;
        }

        Vector3 pos = thisTransform.position;
        nextCheckTime = Time.time + 1;
        Vector3 gateDisp = tracking.gate.position - pos;
        tracking.sqrDistToGate = (gateDisp).sqrMagnitude;

        tracking.result = tracking.gate.IsWithinArea(pos);

        debugMsg = "normal";
        // not within the area!
        if (tracking.result != GatewayResult.inArea)
        {
            GatewayResult result = GatewayResult.inArea;
            int gatesToAdd = 1;
            if (tracking.gate != NodeGraph.StartGate)
            {
                bool didSkip = false;

                // check for skipping
                Gateway next = tracking.gate.nextGate;

                while (next != NodeGraph.StartGate)
                {
                    result = next.IsWithinArea(pos);
                    if (result == GatewayResult.inArea)
                    {
                        tracking.gate = next;
                        gateDisp = tracking.gate.position - pos;
                        tracking.sqrDistToGate = (gateDisp).sqrMagnitude;
                        tracking.gatesPassed += gatesToAdd;
                        didSkip = true;
                        break;
                    }
                    next = next.nextGate;
                    gatesToAdd++;
                }

                // did we skip?
                if (didSkip)
                {
                    return;
                }
            }

            debugMsg = "no shortcut";

            // now try looking back
            gatesToAdd = -1;
            Gateway prev = tracking.gate.previousGate;
            result = GatewayResult.inArea;
            while (prev != tracking.gate.nextGate)
            {
                result = prev.IsWithinArea(pos);
                if (result == GatewayResult.inArea)
                {
                    tracking.gate = prev;
                    gateDisp = tracking.gate.position - pos;
                    tracking.sqrDistToGate = (gateDisp).sqrMagnitude;
                    tracking.gatesPassed += gatesToAdd;
                    return;
                }
                prev = prev.previousGate;
                gatesToAdd--;
            }

            debugMsg = "didn't drop back";
        }

        // not near the gate
        if (tracking.sqrDistToGate > tracking.gate.sqrLength)
        {
            return;
        }



        float dot = Vector3.Dot(gateDisp, tracking.gate.Forward);

        // not yet past the gate
        if (dot >= 0)
        {
            return;
        }

        // near and past the gate, move onwards
        tracking.gatesPassed += 1;
        tracking.gate = tracking.gate.nextGate;
        gateDisp = tracking.gate.position - pos;
        tracking.sqrDistToGate = (gateDisp).sqrMagnitude;

    }

    void UpdateWheels()
    {
        groundedCount = 0;
        float ang = visuals.steerAngle * steerInput;
        for (int i = 0; i < frontWheels.Count; i++)
        {
            frontWheels[i].SetData(currentSpeed, ang);

            if (frontWheels[i].isGrounded)
            {
                groundedCount++;
            }
        }
        for (int i = 0; i < rearWheels.Count; i++)
        {
            rearWheels[i].SetData(currentSpeed, -ang);
            if (rearWheels[i].isGrounded)
            {
                groundedCount++;
            }
        }

        if (groundedCount <= 2)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            canFlip = true;
        }

        // calculate power percentage based on wheels on the ground

        groundedMod = groundedCount / maxWheels;

        if (Time.time - lastGroundedTime > flipTime)
        {
            if (inputData.useInput > 0 && canFlip)
            {
                // flip the car
                flipRot = Quaternion.LookRotation(thisTransform.forward, Vector3.up);
                thisRigidbody.velocity = Vector3.zero;
                thisRigidbody.AddForce(Vector3.up * 10, ForceMode.VelocityChange);
                thisRigidbody.angularVelocity = Vector3.zero;
                canFlip = false;
            }
            else if (!canFlip)
            {
                thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, flipRot, Time.fixedDeltaTime);
            }
        }
    }

    internal void ChangeInputs(string v)
    {
        throw new NotImplementedException();
    }

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.AddNetFunctionListener("recv_car_data", thisNetworkID, ReceiveCarData);
        NetworkManager.AddNetFunctionListener("update_car_visuals", thisNetworkID, CL_UpdateCarVisuals);
        NetworkManager.NetworkUpdateEvent += NetworkUpdate;
    }

    public void OnDestroy()
    {
        NetworkManager.RemoveNetFunctionListener("recv_car_data", thisNetworkID);
        NetworkManager.RemoveNetFunctionListener("update_car_visuals", thisNetworkID);
        NetworkManager.NetworkUpdateEvent -= NetworkUpdate;
    }

    public void NetworkUpdate()
    {

        if (NetworkCore.isServer)
        {
            if (thisRigidbody == null)
            {
                return;
            }
            NetWriter writer = NetworkManager.StartNetworkMessage("recv_car_data", thisNetworkID);
            writer.WriteVector3(thisRigidbody.position);
            writer.WriteVector3(thisRigidbody.rotation.eulerAngles);
            writer.WriteVector3(thisRigidbody.velocity);
            writer.WriteVector3(thisRigidbody.angularVelocity);
            NetworkManager.SendMessageToAllClients(writer, NetworkCore.UnreliableSequencedMsg, false);
        }
    }

    public void ReceiveCarData(NetReader reader)
    {
        Vector3 pos = reader.ReadVector3();
        Vector3 ang = reader.ReadVector3();
        Quaternion rot = Quaternion.Euler(ang);
        Vector3 vel = reader.ReadVector3();
        Vector3 angVel = reader.ReadVector3();
        if (thisRigidbody == null)
        {
            return;
        }

        oldPose = newPose;
        newPose = new NetworkPose(pos, rot, vel, angVel, Time.time);

        poseHistory.Enqueue(newPose);
        if (poseHistory.Count > 10)
        {
            poseHistory.Dequeue();
        }

    }

    public override void Start()
    {
        base.Start();
        thisTransform = this.transform;
        thisRigidbody = this.GetComponent<Rigidbody>();
        thisRigidbody.WakeUp();
        tracking.Init(NodeGraph.SpawnGate);
    }

    public override void ServerStart()
    {

    }

    public override void ClientStart()
    {
    }

    public void OnDrawGizmosSelected()
    {
        if (poseHistory.Count == 0)
        {
            return;
        }
        Gizmos.color = Color.cyan;
        NetworkPose lastPose = poseHistory.Peek();
        foreach (NetworkPose pose in poseHistory)
        {
            Gizmos.DrawLine(lastPose.position, pose.position);
            Gizmos.DrawWireSphere(pose.position, 0.1f);
            lastPose = pose;
        }
    }

    void Fallen()
    {
        Gateway previousGate = tracking.gate.previousGate;
        Vector3 respawnPos;
        Quaternion respawnRot;
        if (previousGate == null)
        {
            respawnPos = startPos;
            respawnRot = startRot;
        }
        else
        {
            respawnPos = previousGate.position + tracking.gate.Forward + Vector3.up * 10;
            respawnRot = Quaternion.LookRotation(tracking.gate.position - previousGate.position);
        }
        DoRespawn(respawnPos, respawnRot);
    }

    internal void DoRespawn(Vector3 position, Quaternion rotation)
    {
        thisRigidbody.MovePosition(position);
        thisRigidbody.MoveRotation(rotation);
        thisRigidbody.velocity = Vector3.zero;
        thisRigidbody.angularVelocity = Vector3.zero;
        BroadcastVisuals();
    }

    internal void Restart(Vector3 position, Quaternion rotation)
    {
        // cache the starting positions
        startPos = position;
        startRot = rotation;
        DoRespawn(position, rotation);
        // TODO fix this
        tracking.Init(NodeGraph.SpawnGate);
        BroadcastVisuals();
    }

    public void BroadcastVisuals()
    {
        if (!NetworkCore.isServer)
        {
            return;
        }

        // pack the visuals
        NetWriter writer = PackVisuals();
        // broadcast this to everyone
        NetworkManager.SendMessageToAllClients(writer, NetworkCore.ReliableSequencedMsg, false);
    }

    public NetWriter PackVisuals()
    {
        NetWriter writer = NetworkManager.StartNetworkMessage("update_car_visuals", thisNetworkID);
        writer.WriteInt(visuals.carNum);    // write our car num
        if (pDriver != null)
        {
            writer.WriteBool(pDriver.isGhost);  // write our ghost state
        }
        else
        {
            writer.WriteBool(false);    // assume not a ghost
        }
        return writer;
    }

    public void CL_UpdateCarVisuals(NetReader reader)
    {

        int vehicleNum = reader.ReadInt();      // read number markings
        bool ghostState = reader.ReadBool();    // read ghost state

        // assign the vehicle number
        visuals.text.text = vehicleNum.ToString();
        // set ghost state
        pGhostControl.SetGhostMode(ghostState);

    }

    public void TakeDamage(DamageInfo info)
    {
        thisRigidbody.AddForce(info.force, info.forceMode);
    }

    public Vector3 GetPosition()
    {
        return thisRigidbody.position;
    }

    internal void RandomItem()
    {
        
    }
}
