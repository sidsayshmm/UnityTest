using UnityEngine;

namespace SpaceShipGame
{
    public class TargetWayPointDirectional : Waypoint
    {
        private Vector3 worldTarget;
        private Vector3 worldOrientation;

        private bool initDone;
        private float adjustedMoveSpeed;
        private float adjustedRotateSpeed;
        private float upVector;

        public TargetWayPointDirectional(Ship forShip, Arrow myArrow, Vector3 worldTarget, Vector3 worldOrientation) : base(forShip, myArrow)
        {
            this.worldTarget = worldTarget;
            this.worldOrientation = worldOrientation;
        }

        private void Init()
        {
            initDone = true;
            var timeToMove = Vector3.Distance(forShip.transform.position, worldTarget) / forShip.moveSpeed;
            float angleToRotate = Vector3.Angle(forShip.transform.forward, worldOrientation.normalized);


            angleToRotate = Mathf.Abs(angleToRotate);
            var timeToRotate = angleToRotate / forShip.rotateSpeed;

            float longestTime = Mathf.Max(timeToRotate, timeToMove);
            adjustedMoveSpeed = Vector3.Distance(forShip.transform.position, worldTarget) / longestTime;
            adjustedRotateSpeed = angleToRotate / longestTime;
        }

        public override void Update()
        {
            if (!initDone)
            {
                Init();
                return;
            }

            forShip.transform.position = Vector3.MoveTowards(forShip.transform.position, worldTarget,
                adjustedMoveSpeed * Time.deltaTime);
            forShip.transform.rotation = Quaternion.RotateTowards(forShip.transform.rotation,
                Quaternion.LookRotation(worldOrientation, forShip.transform.up), adjustedRotateSpeed * Time.deltaTime);
            if ((forShip.transform.position - worldTarget).sqrMagnitude < Mathf.Epsilon)
            {
                MarkCompleted();
            }
        }
    }
}