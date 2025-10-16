using System;
using System.Collections.Generic;
using QuickOutline;
using UnityEngine;

namespace SpaceShipGame
{
    public class Ship : MonoBehaviour
    {
        [field: SerializeField] public float moveSpeed { get; private set; } = 4f;
        [field: SerializeField] public float rotateSpeed { get; private set; } = 4f;

        [SerializeField] private Outline outline;

        private List<Waypoint> currWayPoints;

        private void Awake()
        {
            currWayPoints = new();
        }

        public void AddWayPoints(Waypoint newWayPoint, bool isShiftPressed)
        {
            if (!isShiftPressed)
            {
                ClearWayPoints();
            }

            currWayPoints.Add(newWayPoint);
        }

        private void ClearWayPoints()
        {
            foreach (var currWayPoint in currWayPoints)
            {
                currWayPoint.Cancel();
            }

            currWayPoints.Clear();
        }

        private void Update()
        {
            if (currWayPoints.Count == 0)
            {
                return;
            }

            if (currWayPoints[0].IsCompleted)
            {
                currWayPoints.RemoveAt(0);
            }
            else
            {
                currWayPoints[0].Update();
            }
        }

        public void ToggleOutline(bool toSet)
        {
            outline.enabled = toSet;
        }
    }
}