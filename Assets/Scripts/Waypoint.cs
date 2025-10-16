using UnityEngine;

namespace SpaceShipGame
{
    public abstract class Waypoint
    {
        public bool IsCompleted { get; private set; }
        protected Ship forShip;

        protected Arrow myArrow;

        protected Waypoint(Ship forShip, Arrow myArrow)
        {
            this.myArrow = myArrow;
            this.forShip = forShip;
        }

        public abstract void Update();

        protected virtual void MarkCompleted()
        {
            if (myArrow != null)
            {
                Object.Destroy(myArrow.gameObject);
            }
            IsCompleted = true;
        }

        public virtual void Cancel()
        {
            Object.Destroy(myArrow.gameObject);
        }
    }
}