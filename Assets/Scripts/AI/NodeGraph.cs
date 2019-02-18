using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Navigation
{
    public class NodeGraph : MonoBehaviour
    {
        private static NodeGraph singleton;

        public static Gateway GetNearestGateway(Transform trans)
        {
            Gateway nextGate = singleton.startGate;

            Gateway closest = singleton.startGate;
            float closestDist = Mathf.Infinity;

            while (nextGate)
            {
                float dist = (nextGate.position - trans.position).sqrMagnitude;
                if(dist < closestDist)
                {
                    closestDist = dist;
                    closest = nextGate;
                }

                nextGate = nextGate.nextGate;
                if(nextGate == singleton.startGate)
                {
                    break;
                }
            }

            return closest;
        }

        public Gateway startGate;
        public Gateway spawnGate;

        private void Awake()
        {
            singleton = this;
            // configure spawn gate
            spawnGate.previousGate = startGate;
            spawnGate.nextGate = startGate.nextGate;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        internal static Gateway StartGate
        {
            get
            {
                return singleton.startGate;
            }
        }

        internal static Gateway SpawnGate
        {
            get
            {
                return singleton.spawnGate;
            }
        }
    }
}
