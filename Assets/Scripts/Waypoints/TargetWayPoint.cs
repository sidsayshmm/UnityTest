using UnityEngine;

namespace SpaceShipGame
{
    public class TargetWayPoint : Waypoint
    {
        private Vector3 worldTarget;

        public TargetWayPoint(Ship forShip, Arrow myArrow, Vector3 worldTarget) : base(forShip, myArrow)
        {
            this.worldTarget = worldTarget;
        }

        public override void Update()
        {
            forShip.transform.position = Vector3.MoveTowards(forShip.transform.position, worldTarget, forShip.moveSpeed * Time.deltaTime);
            if ((forShip.transform.position - worldTarget).sqrMagnitude < Mathf.Epsilon)
            {
                MarkCompleted();
            }
        }
    }
}