using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    [RequireComponent(typeof(RaycastController))]
    public class PlatformController : MonoBehaviour
    {
        public enum PlatformState
        {
            Idling,
            Triggered,
            DoingActivity
        }

        private PlatformState _state;

        private RaycastController _rc;
        private SpriteRenderer _spriteRenderer;

        public LayerMask PassengerMask;
        public Vector3[] LocalWaypoints;
        private Vector3[] _globalWaypoints;
        private Vector3 _velocity = new Vector3(0, 0, 0);
        public float Speed;
        public float WaypointWaitTime;
        public bool Cyclic;

        public float ActivationDelay;
        private float _activationDelayTimer;
        private bool _destroyOnRouteFinished;

        [Range(0, 2)]
        public float EaseAmount;

        int _fromWaypointIndex;
        float _percentBetweenWaypoints;
        float _nextMoveTime;

        List<PassengerMovement> _passengerMovement;
        Dictionary<Transform, Player> _passengerDictionary;

        void Awake()
        {
            _rc = GetComponent<RaycastController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _passengerMovement = new List<PassengerMovement>();
            _passengerDictionary = new Dictionary<Transform, Player>();
        }

        void Start()
        {
            SetupPlatform(transform.position);
        }

        void Update()
        {
            _rc.UpdateRaycastOrigins();
            CalculateHit();

            switch (_state)
            {
                case PlatformState.Idling:
                    return;
                case PlatformState.Triggered:
                    _activationDelayTimer += Time.deltaTime;
                    if (_activationDelayTimer >= ActivationDelay)
                        _state = PlatformState.DoingActivity;
                    return;
                case PlatformState.DoingActivity:
                    // Do anything you want the platform to do, when its activity has been triggered.

                    // If there are no waypoints, then do not move the platform or do any movement-related calculations.
                    if (LocalWaypoints.Length < 1)
                        return;

                    // Move the platform
                    _velocity = CalculatePlatformMovement();

                    CalculatePassengerMovement(_velocity);

                    MovePassengers(true);
                    transform.Translate(_velocity);
                    MovePassengers(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CalculateHit()
        {
            float rayLength = RaycastController.skinWidth;
            for (int i = 0; i < _rc.verticalRayCount; i++)
            {
                Vector2 rayOrigin = _rc.raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_rc.verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, PassengerMask);

                Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);

                if (hit.collider != null)
                {
                    OnPlatformCollision(hit);
                }
            }
        }

        public void SetupPlatform(Vector3 position)
        {
            transform.position = position;

            _globalWaypoints = new Vector3[LocalWaypoints.Length];

            for (int i = 0; i < LocalWaypoints.Length; i++)
            {
                _globalWaypoints[i] = LocalWaypoints[i] + transform.position;
            }

            _rc.Setup();
        }

        public void SetupPlatform(Vector3 position, int tileSizeX, int tileSizeY)
        {
            _spriteRenderer.size = new Vector2(tileSizeX, tileSizeY);
            SetupPlatform(position);
        }

        public void ResetPlatform()
        {
            _state = PlatformState.Idling;
            _destroyOnRouteFinished = false;
            _spriteRenderer.color = Color.black;
            _activationDelayTimer = 0;
            _fromWaypointIndex = 0;
            _percentBetweenWaypoints = 0f;
            _nextMoveTime = 0f;
            _passengerDictionary.Clear();
            _passengerMovement.Clear();
            _globalWaypoints = null;
            LocalWaypoints = new Vector3[0];
        }

        float Ease(float x)
        {
            float a = EaseAmount + 1;
            return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
        }

        Vector3 CalculatePlatformMovement()
        {
            if (_globalWaypoints == null || _globalWaypoints.Length == 0)
                return Vector3.zero;

            if (Time.time < _nextMoveTime)
            {
                return Vector3.zero;
            }

            _fromWaypointIndex %= _globalWaypoints.Length;
            int toWaypointIndex = (_fromWaypointIndex + 1) % _globalWaypoints.Length;
            float distanceBetweenWaypoints = Vector3.Distance(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex]);
            _percentBetweenWaypoints += Time.deltaTime * Speed / distanceBetweenWaypoints;
            _percentBetweenWaypoints = Mathf.Clamp01(_percentBetweenWaypoints);
            float easedPercentBetweenWaypoints = Ease(_percentBetweenWaypoints);

            Vector3 newPos = Vector3.Lerp(_globalWaypoints[_fromWaypointIndex], _globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

            if (_percentBetweenWaypoints >= 1)
            {
                _percentBetweenWaypoints = 0;
                _fromWaypointIndex++;

                if (_fromWaypointIndex >= _globalWaypoints.Length - 1)
                {
                    if (_destroyOnRouteFinished)
                    {
                        if (PlatformSpawner.Instance != null)
                            PlatformSpawner.DestroyPlatform(gameObject);
                        else
                            Destroy(gameObject);

                        return Vector3.zero;
                    }
                    else if (!Cyclic)
                    {
                        _fromWaypointIndex = 0;
                        Array.Reverse(_globalWaypoints);
                    }
                }
                _nextMoveTime = Time.time + WaypointWaitTime;
            }

            return newPos - transform.position;
        }

        void MovePassengers(bool beforeMovePlatform)
        {
            foreach (PassengerMovement passenger in _passengerMovement)
            {
                if (!_passengerDictionary.ContainsKey(passenger.transform))
                {
                    _passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Player>());
                }

                if (passenger.moveBeforePlatform == beforeMovePlatform)
                {
                    _passengerDictionary[passenger.transform].Move(passenger.velocity);
                }
            }
        }

        void OnPlatformCollision(RaycastHit2D hit)
        {
            switch (_state)
            {
                case PlatformState.Idling:
                    _state = PlatformState.Triggered;
                    break;
                case PlatformState.Triggered:
                    // If you want to react to something after the platform has been triggered, but before it has started doing its activity, do it here.
                    _activationDelayTimer = 0;
                    break;
                case PlatformState.DoingActivity:
                    // If you want to react to something after the platform has started doing its activity, do it here.
                    // If the platform needs to be able to be triggered several times, make sure to set _state back to Idling.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void CalculatePassengerMovement(Vector3 velocity)
        {
            HashSet<Transform> movedPassengers = new HashSet<Transform>();
            _passengerMovement.Clear();

            float directionX = Mathf.Sign(velocity.x);
            float directionY = Mathf.Sign(velocity.y);

            //Static platform
            if (Mathf.Approximately(velocity.y, 0) && Mathf.Approximately(velocity.x, 0))
            {
                float rayLength = RaycastController.skinWidth;

                for (int i = 0; i < _rc.verticalRayCount; i++)
                {
                    Vector2 rayOrigin = Mathf.Approximately(directionY, -1) ? _rc.raycastOrigins.bottomLeft : _rc.raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (_rc.verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, PassengerMask);


                    if (hit.collider == null)
                        continue;

                    /*
            if (!movedPassengers.Contains(hit.transform))
            {
                movedPassengers.Add(hit.transform);
                float pushX = (directionY == 1) ? velocity.x : 0;
                float pushY = velocity.y - (hit.distance - RaycastController.skinWidth) * directionY;

                passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                OnPlatformCollision(hit);
            }
            */
                }
            }


            // Vertically moving platform
            if (!Mathf.Approximately(velocity.y, 0))
            {
                float rayLength = Mathf.Abs(velocity.y) + RaycastController.skinWidth;

                for (int i = 0; i < _rc.verticalRayCount; i++)
                {
                    Vector2 rayOrigin = Mathf.Approximately(directionY, -1) ? _rc.raycastOrigins.bottomLeft : _rc.raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (_rc.verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, PassengerMask);

                    if (hit.collider == null)
                        continue;

                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = Mathf.Approximately(directionY, 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - RaycastController.skinWidth) * directionY;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }

            // Horizontally moving platform
            if (!Mathf.Approximately(velocity.x, 0))
            {
                float rayLength = Mathf.Abs(velocity.x) + RaycastController.skinWidth;

                for (int i = 0; i < _rc.horizontalRayCount; i++)
                {
                    Vector2 rayOrigin = Mathf.Approximately(directionY, -1) ? _rc.raycastOrigins.bottomLeft : _rc.raycastOrigins.bottomRight;
                    rayOrigin += Vector2.up * (_rc.horizontalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, PassengerMask);

                    if (hit.collider == null)
                        continue;

                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - RaycastController.skinWidth) * directionX;
                        float pushY = -RaycastController.skinWidth;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }

            // Passenger on top of a horizontally or downward moving platform
            if (Mathf.Approximately(directionY, -1) || Mathf.Approximately(velocity.y, 0) && !Mathf.Approximately(velocity.x, 0))
            {
                float rayLength = RaycastController.skinWidth * 2;

                for (int i = 0; i < _rc.verticalRayCount; i++)
                {
                    Vector2 rayOrigin = _rc.raycastOrigins.topLeft + Vector2.right * (_rc.verticalRaySpacing * i);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, PassengerMask);

                    if (hit.collider == null)
                        continue;

                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }

        public struct PassengerMovement
        {
            public Transform transform;
            public Vector3 velocity;
            public bool standingOnPlatform;
            public bool moveBeforePlatform;

            public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
            {
                transform = _transform;
                velocity = _velocity;
                standingOnPlatform = _standingOnPlatform;
                moveBeforePlatform = _moveBeforePlatform;
            }
        }

        void OnDrawGizmos()
        {
            if (LocalWaypoints != null)
            {
                Gizmos.color = Color.red;
                float size = .3f;

                for (int i = 0; i < LocalWaypoints.Length; i++)
                {
                    Vector3 globalWaypointPos = (Application.isPlaying) ? _globalWaypoints[i] : LocalWaypoints[i] + transform.position;
                    Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                    Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
                }
            }
        }
    }
}
