using SpaceShipGame.SpaceShipGame.Waypoints;
using UnityEngine;

namespace SpaceShipGame
{
    public class SelectionHandler : MonoBehaviour
    {
        [SerializeField] private Arrow arrowPrefab;
        
        private Camera mainCam;
        private Ship selectedShip;

        private bool isDragging = false;
        private Vector3 startMousePos;
        private Arrow currentArrow;
        private Enemy activeTrackingEnemy;
        
        private void Start()
        {
            if (mainCam == null)
                mainCam = Camera.main;
        }

        private void Update()
        {
            HandleMouseEvents();
        }

        private void HandleMouseEvents()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Input.mousePosition;
            }
            else if(Input.GetMouseButton(0))
            {
                if (Input.mousePosition != startMousePos)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    if (selectedShip != null)
                    {
                        if (!currentArrow)
                        {
                            currentArrow = CreateArrow();
                            currentArrow.Init(GetWorldCoordsFromMousePos(startMousePos), selectedShip.transform.forward, 0);
                        }
                        currentArrow.UpdateEndPoint(GetWorldCoordsFromMousePos(Input.mousePosition), false);

                        var enemy = CheckForEnemy();
                        if (enemy != activeTrackingEnemy)
                        {
                            if (activeTrackingEnemy)
                            {
                                activeTrackingEnemy.HoveringOverEnemy(false);
                            }
                            activeTrackingEnemy = enemy;
                            activeTrackingEnemy?.HoveringOverEnemy(true);
                        }
                    }
                }
            }
            else if(Input.GetMouseButtonUp(0))
            {
                if (selectedShip != null)
                {
                    if (isDragging)
                    {
                        if (activeTrackingEnemy != null)
                        {
                            selectedShip.AddWayPoints(new EnemyWayPoint(selectedShip, currentArrow, activeTrackingEnemy), IsShiftPressed());
                        }
                        else
                        {
                            var startWordCoords = GetWorldCoordsFromMousePos(startMousePos);
                            var targetWorldCoords = GetWorldCoordsFromMousePos(Input.mousePosition);
                            currentArrow.UpdateEndPoint(targetWorldCoords, true);
                            selectedShip.AddWayPoints(new TargetWayPointDirectional(selectedShip, currentArrow, startWordCoords,
                                (targetWorldCoords - startWordCoords).normalized), IsShiftPressed());
                        }
                        currentArrow = null;
                    }
                    else
                    {
                        //check for tap on other ships.
                        if (CheckForSelectionChange())
                        {
                            isDragging = false;
                            return;
                        }
                        var arrow = CreateArrow();
                        var startWorldPos = GetWorldCoordsFromMousePos(startMousePos);
                        arrow.Init(startWorldPos, selectedShip.transform.forward,0);
                        selectedShip.AddWayPoints(new TargetWayPoint(selectedShip, arrow, startWorldPos), IsShiftPressed() );
                    }
                }
                else
                {
                    CheckForSelectionChange();
                }
                
                isDragging = false;
            }
            else
            {
                isDragging = false;
            }
        }

        private bool CheckForSelectionChange()
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var ship = hit.collider.GetComponentInParent<Ship>();
                if (ship != selectedShip && ship.GetType() != typeof(Enemy))
                {
                    ChangeSelectedShip(ship);
                    return true;
                }
            }
            return false;
        }
        
        private Enemy CheckForEnemy()
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var enemy = hit.collider.GetComponentInParent<Enemy>();
                return enemy;
            }
            return null;
        }

        private bool IsShiftPressed()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
        private Vector3 GetWorldCoordsFromMousePos(Vector3 mousePos)
        {
            // var coords = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCam.transform.position.y));
            // return new Vector3(coords.x, 0, coords.z);
            Ray ray = mainCam.ScreenPointToRay(mousePos);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            plane.Raycast(ray, out float distance);
            Vector3 worldPos = ray.GetPoint(distance);
            return worldPos;
        }

        private void ChangeSelectedShip(Ship ship)
        {
            if (selectedShip != null)
            {
                selectedShip.ToggleOutline(false);
            }

            selectedShip = ship;
            selectedShip.ToggleOutline(true);
        }

        private Arrow CreateArrow()
        {
            var arrow =  Instantiate(arrowPrefab);
            arrow.transform.position = Vector3.zero;
            return arrow;
        }
    }
}