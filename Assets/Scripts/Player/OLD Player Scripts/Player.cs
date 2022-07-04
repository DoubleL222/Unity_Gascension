using UnityEngine;

namespace Entities
{
    public class Player : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        public bool UseControllerOrStick;
        public string HorizontalAxis, JumpAxis;
        public bool CanJumpUpThroughPlatforms;
        public bool CanWallSlide;
        public bool NoGravity; //Jakob test

        public float MaxJumpHeight = 4.0f;
        public float MinJumpHeight = 0.5f;
        public float TimeToJumpApex = .4f;
        private float _accelerationTimeAirborne = .1f;
        private float _accelerationTimeGrounded = .1f;
        private float _moveSpeed = 6;

        public Vector2 WallJump = new Vector2(10, 15);
        public float WallSlideSpeedMax = 4;
        private bool _wallSliding;
        private int _wallDirX;

        public float _gravity;
        private float _maxJumpVelocity;
        private float _minJumpVelocity;
        private Vector3 _velocity;
        private float _velocityXSmoothing;
        [Range(1, 2)] public int PlayerNumber = 1;

        private bool _frozen;
        private bool _facingLeft = true;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            _gravity = -(2 * MaxJumpHeight) / Mathf.Pow(TimeToJumpApex, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * TimeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * MinJumpHeight);
        }

        void Update()
        {
            if (_frozen)
                return;

            // For using the built-in controller config.
            float horizInput = Input.GetAxis(HorizontalAxis);
            // For key-based controls which can be set in the inspector (uncomment the key-fields in this class)
            //float h = Input.GetKey(key_left) ? -1 : Input.GetKey(key_right) ? 1 : 0;

            Vector2 input = new Vector2(horizInput, 0);

            if (horizInput > 0 && _facingLeft)
                FlipSprite();
            else if (horizInput < 0 && !_facingLeft)
                FlipSprite();

            if (Input.GetButtonDown(JumpAxis)){
                OnJumpInputDown(input);
            }
            if (Input.GetButtonUp(JumpAxis)){
                OnJumpInputUp();
            }

            CalculateVelocity(input);

            if (CanWallSlide)
                HandleWallSliding();

            Move(_velocity * Time.deltaTime);

            // Stop the player in the y-axis, if he collides with something above or below him.
            // If he's standing on something that is sloping, make him slide down on it, using gravity and the normal-vector of the slope.
            //if (_controller.Collisions.Above || _controller.Collisions.Below) {
            //    if (_controller.Collisions.SlidingDownMaxSlope) {
            //        _velocity.y += _controller.Collisions.SlopeNormal.y * -_gravity * Time.deltaTime;
            //    } else {
            //        _velocity.y = 0;
            //    }
            //}
        }

        public void Move(Vector3 velocity)
        {
            transform.Translate(_velocity * Time.deltaTime);
        }

        void FlipSprite()
        {
            _facingLeft = !_facingLeft;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        public void ForceFacing(bool faceLeft)
        {
            if (faceLeft && !_facingLeft)
                FlipSprite();
            else if (!faceLeft && _facingLeft)
                FlipSprite();
        }

        public void OnJumpInputDown(Vector2 input)
        {
            if (_wallSliding)
            {
                _velocity.x = -_wallDirX * WallJump.x;
                _velocity.y = WallJump.y;
            }
            // If player is grounded, make him jump
            //if (_controller.Collisions.Below) {
            //    if (_controller.Collisions.SlidingDownMaxSlope) {
            //        if (!Mathf.Approximately(input.x, -Mathf.Sign (_controller.Collisions.SlopeNormal.x))) { // not jumping against max slope
            //            _velocity.y = _maxJumpVelocity * _controller.Collisions.SlopeNormal.y;
            //            _velocity.x = _maxJumpVelocity * _controller.Collisions.SlopeNormal.x;
            //        }
            //    } else {
            //        _velocity.y = _maxJumpVelocity;
            //    }
            //}
        }

        public void OnJumpInputUp()
        {
            if (_velocity.y > _minJumpVelocity)
            {
                _velocity.y = _minJumpVelocity;
            }
        }

        void HandleWallSliding()
        {
            //_wallDirX = (_controller.Collisions.Left) ? -1 : 1;
            //_wallSliding = false;
            //if ((_controller.Collisions.Left || _controller.Collisions.Right) && !_controller.Collisions.Below) { 
            //    _wallSliding = true;

            //    if (_velocity.y < -WallSlideSpeedMax) {
            //        _velocity.y = -WallSlideSpeedMax;
            //    }	
            //}
        }

        void CalculateVelocity(Vector2 input)
        {
            float targetVelocityX = input.x * _moveSpeed;

            // TODO Is player coliding with ground?
            bool collidingWithGround = true;

            _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing,
                collidingWithGround ? _accelerationTimeGrounded : _accelerationTimeAirborne);
            if (!NoGravity){
                _velocity.y += _gravity * Time.deltaTime;
            }
        }

        public void SetFrozen(bool freeze)
        {
            if (_frozen == freeze)
                return;

            _frozen = freeze;
            Debug.Log((freeze ? "Freezing" : "Unfreezing") + " Player " + PlayerNumber);
        }

        public void StopMovement()
        {
            _velocity = Vector3.zero;
            Debug.Log("Stopping Player " + PlayerNumber);
        }

        public void SetInvisible(bool hide)
        {
            // If trying to hide, and renderer is already disabled...
            if (hide && !_spriteRenderer.enabled || !hide && _spriteRenderer.enabled)
                return;

            _spriteRenderer.enabled = !hide;

            Debug.Log((hide ? "Hiding" : "Showing") + " Player " + PlayerNumber);
        }

        /// <summary>
        /// Negative values push down, positive values bounce up.
        /// </summary>
        /// <param name="amount"></param>
        public void Bounce(float amount)
        {
            _velocity = _velocity + new Vector3(0, amount, 0);
        }

        /// <summary>
        /// Negative values push left, positive push right.
        /// </summary>
        /// <param name="amount"></param>
        public void Push(float amount)
        {
            _velocity = _velocity + new Vector3(amount, 0, 0);
        }

        public Vector3 GetVelocity()
        {
            return _velocity;
        }

        public void SetVelocity(Vector3 vel)
        {
            _velocity = vel;
        }

        public bool IsWallSliding()
        {
            return _wallSliding;
        }

        public void SetWallSliding(bool isWallSliding)
        {
            _wallSliding = isWallSliding;
        }

        public void SetCollidersActive(bool enable)
        {
            GetComponent<BoxCollider2D>().enabled = enable;
        }
    }
}