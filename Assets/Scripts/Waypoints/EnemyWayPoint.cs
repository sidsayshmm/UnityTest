using UnityEngine;

namespace SpaceShipGame
{
    namespace SpaceShipGame.Waypoints
    {
        public class EnemyWayPoint : Waypoint
        {
            private Enemy enemy;
            private Vector3 startOffsetFromEnemy;
            public EnemyWayPoint(Ship forShip, Arrow myArrow, Enemy enemy) : base(forShip, myArrow)
            {
                this.enemy = enemy;
                startOffsetFromEnemy = (forShip.transform.position - enemy.transform.position);
            }

            public override void Update()
            {
                enemy.HoveringOverEnemy(true);
                myArrow.UpdateStartPoint(forShip.transform.position);
                myArrow.UpdateEndPoint(enemy.transform.position, false);
                forShip.transform.position = enemy.transform.position + startOffsetFromEnemy;
            }

            protected override void MarkCompleted()
            {
                base.MarkCompleted();
                enemy.HoveringOverEnemy(false);
            }

            public override void Cancel()
            {
                base.Cancel();
                enemy.HoveringOverEnemy(false);
            }
        }
    }
}