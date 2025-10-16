using UnityEngine;

namespace SpaceShipGame
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private LineRenderer baseLine;
        [SerializeField] private LineRenderer leftPointer;
        [SerializeField] private LineRenderer rightPointer;

        private Vector3 startPoint;
        private Vector3 endPoint;

        public float baseLength = 1f;
        public float topArrowLength = 0.2f;
        private float yForLines;

        public void Init(Vector3 start, Vector3 dir, float yForLines)
        {
            startPoint = start;
            startPoint = new Vector3(startPoint.x, yForLines, startPoint.z);

            endPoint = start + dir * baseLength;
            endPoint = new Vector3(endPoint.x, yForLines, endPoint.z);
            this.yForLines = yForLines;
            ModifyLines();
        }

        public void UpdateEndPoint(Vector3 endPoint, bool final)
        {
            this.endPoint = endPoint;
            if (final)
            {
                this.endPoint = new Vector3(endPoint.x, yForLines, endPoint.z);
                this.endPoint = startPoint + (endPoint - startPoint).normalized * baseLength;
            }

            ModifyLines();
        }

        public void UpdateStartPoint(Vector3 startPoint)
        {
            this.startPoint = startPoint;
        }

        void ModifyLines()
        {
            baseLine.SetPosition(0, startPoint);
            baseLine.SetPosition(1, endPoint);

            leftPointer.SetPosition(1, endPoint);
            rightPointer.SetPosition(1, endPoint);

            var downOffset = endPoint + (startPoint - endPoint).normalized * topArrowLength;
            var sideOffset = Camera.main.transform.up * topArrowLength;
            leftPointer.SetPosition(0, downOffset + sideOffset);
            rightPointer.SetPosition(0, downOffset - sideOffset);
        }
    }
}