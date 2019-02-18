using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Navigation
{
    public class Gateway : MonoBehaviour
    {
        [SerializeField]
        private bool linkPrevious = true;
        public Gateway nextGate;

        public Gateway previousGate;

        internal Vector3 rightPost;
        internal Vector3 leftPost;

        public float halfWidth = 10;
        public float viewHeight = 1;
        public float sqrLength;
        float _sharpness;
        Transform thisTransform;
        Vector3 _centerViewPos;
        

        public Vector3 centerViewPos
        {
            get
            {
                return _centerViewPos;
            }
        }

        public Vector3 position
        {
            get
            {
                return thisTransform.position;
            }
        }

        public Vector3 Forward
        {
            get
            {
                return thisTransform.forward;
            }
        }

        public float sharpness
        {
            get
            {
                return _sharpness;
            }
        }



        // Use this for initialization
        void Awake()
        {
            thisTransform = this.transform;
            if (nextGate != null && linkPrevious)
            {
                nextGate.previousGate = this;
            }

            rightPost = thisTransform.position + thisTransform.right * halfWidth;
            leftPost = thisTransform.position - thisTransform.right * halfWidth;
            _centerViewPos = thisTransform.position + thisTransform.up * viewHeight;
            sqrLength = (leftPost - rightPost).sqrMagnitude;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                CalcSharpness();
            }
            Gizmos.color = Color.magenta;
            if (thisTransform == null)
            {
                thisTransform = this.transform;
            }
            rightPost = thisTransform.position + thisTransform.right * halfWidth;
            leftPost = thisTransform.position - thisTransform.right * halfWidth;

            Gizmos.DrawRay(thisTransform.position, thisTransform.forward * 3f);
            Gizmos.DrawLine(leftPost, rightPost);
            Gizmos.DrawWireSphere(leftPost, 1f);
            Gizmos.DrawWireSphere(rightPost, 1f);


            if (nextGate != null)
            {
                Gizmos.DrawLine(leftPost, nextGate.transform.position);
                Gizmos.DrawLine(rightPost, nextGate.transform.position);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(leftPost, nextGate.leftPost);
                Gizmos.DrawLine(rightPost, nextGate.rightPost);
            }

            Gizmos.color = Color.yellow;
            _centerViewPos = thisTransform.position + thisTransform.up * viewHeight;
            Gizmos.DrawLine(thisTransform.position, centerViewPos);
            Gizmos.DrawWireSphere(centerViewPos, 1);

        }

        private void CalcSharpness()
        {
            Gateway gate0 = this;
            Gateway gate1 = nextGate;
            Gateway gate2 = gate1.nextGate;

            if (gate0 == null || gate1 == null || gate0 == null)
            {
                return;
            }

            Vector3 first = gate1.transform.position - gate0.transform.position;
            Vector3 second = gate2.transform.position - gate1.transform.position;

            _sharpness = Vector3.Angle(first, second);
        }

        public Vector3 GetClosestPoint(Vector3 point)
        {
            Vector3 dirToGate = point - leftPost;
            Vector3 gateDir = rightPost - leftPost;

            float scalar = Vector3.Dot(dirToGate, gateDir) / gateDir.sqrMagnitude;
            // check bounds
            if (scalar < 0 || scalar > 1)
            {
                return thisTransform.position;
            }

            return leftPost + (gateDir * scalar);
        }

        public bool IsOnTarget(Transform trans)
        {
            Vector3 dirToGate = trans.position - leftPost;
            Vector3 gateDir = rightPost - leftPost;

            float scalar = Vector3.Dot(dirToGate, gateDir) / gateDir.sqrMagnitude;
            // check bounds
            return scalar > 0 && scalar < 1;
        }

        public GatewayResult IsWithinArea(Vector3 pos)
        {
            Vector3 leftLine = leftPost - previousGate.leftPost;
            Vector3 rightLine = rightPost - previousGate.rightPost;

            leftLine = Vector3.Cross(leftLine, Vector3.up);
            rightLine = Vector3.Cross(rightLine, Vector3.up);

            Vector3 dirToLeft = leftPost - pos;
            Vector3 dirToRight = rightPost - pos;
            // left dot
            float dotLeft = Vector3.Dot(dirToLeft, leftLine);
            // the cross product is in the wrong direction :(
            // so should be positive
            if (dotLeft < 0)
            {
                return GatewayResult.leftInvalid;
            }
            
            // right dot
            float dotRight = Vector3.Dot(dirToRight, rightLine);
            // should be negative
            if (dotRight > 0)
            {
                return GatewayResult.rightInvalid;
            }

            // check if we're between the two gates
            // check ahead of previous gate
            Vector3 dirFromPrevious = previousGate.position - pos;
            float dotPrev = Vector3.Dot(dirFromPrevious, previousGate.Forward);
            // this should be negative, as the directions are opposite
            if (dotPrev > 0)
            {
                return GatewayResult.backInvalid;
            }

            // check behind this gate

            Vector3 dirToGate = position - pos;
            float dotCurr = Vector3.Dot(dirToGate, Forward);
            // this should be positive
            if(dotCurr < 0)
            {
                return GatewayResult.fwdInvalid;
            }

            return GatewayResult.inArea;
        }
    }
}
