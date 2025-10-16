using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceShipGame
{
    public class Enemy : Ship
    {
        [SerializeField] private GameObject circularOutline;

        private void Start()
        {
            RandomMovement();
        }
        
        private void RandomMovement()
        {
            for (int i = 0; i < 100; i++)
            {
                AddWayPoints(new TargetWayPointDirectional(this, null, 
                    new Vector3(Random.Range(-10,10), 0, Random.Range(-8, 8)), 
                    Random.rotation.eulerAngles), true);
            }
        }

        public void HoveringOverEnemy(bool toSet)
        {
            circularOutline.SetActive(toSet);
        }
    }
}