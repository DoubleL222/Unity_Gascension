using Entities;
using Player;
using Systems;
using UnityEngine;
using UnityEngine.UI;

public class GasController : MonoBehaviour
{
    //Allows for accention
    RaycastHit2D hit;
    public LayerMask mask;
    public float minTimeToHoldForGasJump = 0.2f;
    public float maxGasJumpChargeTime = 0.5f;
    public float airFartMultiplier = 2f;
    private Vector3 _velocity;
    //private Entities.Player player;
    private PlatformerMotor2D _platformerMotor2D;

    //Controls how much gas can be released
    public float minBuildUpForce;
    public float maxBuildUpForce;

    public float _timeJumpWasHeld = 0f;

    private bool _hasLandedSinceLastFartyJump = true;
    public bool MustLandBeforeNextFartyJump = true;
    public bool ResetOnGrounded, ResetOnNormalJump, ResetOnCornerJump, ResetOnWallJump, ResetOnWallStick, ResetOnWallSlide, ResetOnCornerGrab;

    public SoundScript soundScript;
    public PS_Fart particleFxScript;

    public Image cone;
    public Image cone_background;
    public Image cone_col;
    public Image gasBarFill;
    public float ChargeFillPerSec = 1.6f;
    private int currentFartyJumpCharges = 4;
    private int maxFartyJumpCharges = 4;

    public int beanScore;

    [Range(0.1f, 1.0f)]
    public float slowMoAmount = 0.3f;

    private float _currentSlowMoAmount;
    private float _slowMoTimer;
    public float _timeToFullSlowMo = 0.5f;

    private LineRenderer line;
    private bool full;
    private bool _fillingUp;
    private bool _charging;
    private float _charge;
    private bool _lastJumpWasPressedOnTheGround = true;

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("collide " + collision.gameObject.name);
        if (collision.gameObject.tag == "Bean")
        {
            Destroy(collision.gameObject);
            GetComponent<PS_Fart>().CollectBeansEffect(collision.transform);
            soundScript.PlayEatSound();
            GameManager.Instance.AddToScore(beanScore);
            if (currentFartyJumpCharges < maxFartyJumpCharges)
            {
                currentFartyJumpCharges += 1;
                gasBarFill.fillAmount = (float)currentFartyJumpCharges / maxFartyJumpCharges;
            }
            //GameManager.Instance.changeStaminaCircleAmount(1.0f);
        }

    }

    // Use this for initialization
    void Start()
    {
        if (cone != null && cone_col != null && cone_background != null)
        {
            cone.fillAmount = 0;
            cone_background.fillAmount = 0;
            cone_col.fillAmount = 0;
            full = false;
            _fillingUp = true;
        }

        if (gasBarFill != null)
        {
            gasBarFill.fillAmount = 1.0f;
        }

        //player = this.gameObject.GetComponent<Entities.Player>();
        _platformerMotor2D = this.gameObject.GetComponent<PlatformerMotor2D>();

        //_velocity = player.GetVelocity();

        // Any of the following things reenable the ability to farty jump. Just about anything does, except an airjump and a farty-jump.

        _platformerMotor2D.onGrounded += () =>
        {
            //Debug.Log("onGrounded");
            if (ResetOnGrounded)
                _hasLandedSinceLastFartyJump = true;
            CancelGasCharge();
        };

        //_platformerMotor2D.onLanded += () =>
        //{
        //    //Debug.Log("onLanded");
        //    if (ResetOnGrounded)
        //        _hasLandedSinceLastFartyJump = true;
        //};

        _platformerMotor2D.onNormalJump += () =>
        {
            if (ResetOnNormalJump)
                //Debug.Log("onNormalJump");
                _hasLandedSinceLastFartyJump = true;
        };

        _platformerMotor2D.onCornerJump += () =>
        {
            //Debug.Log("onCornerJump");
            if (ResetOnCornerJump)
                _hasLandedSinceLastFartyJump = true;
        };

        _platformerMotor2D.onWallJump += (Vector2 wallNormal) =>
        {
            //Debug.Log("onWallJump");
            if (ResetOnWallJump)
                _hasLandedSinceLastFartyJump = true;
        };

        _platformerMotor2D.onWallSticking += () =>
        {
            //Debug.Log("onWallSticking");
            if (ResetOnWallStick)
                _hasLandedSinceLastFartyJump = true;
        };

        _platformerMotor2D.onWallSliding += () =>
        {
            //Debug.Log("onWallSliding");
            if (ResetOnWallSlide)
                _hasLandedSinceLastFartyJump = true;
        };

        _platformerMotor2D.onCornerGrabbed += () =>
        {
            //Debug.Log("onCornerGrabbed");
            if (ResetOnCornerJump)
                _hasLandedSinceLastFartyJump = true;
            CancelGasCharge();
        };

        line = GetComponent<LineRenderer>();
        line.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused())
            return;

        //drawLine() will draw a line between the mouse and the player when called. Uses mouse control;
        //may need to be changed based on how the actual mouse jump gets implemented!
        //drawLine();

        if (Input.GetButtonDown(PC2D.Input.JUMP))
            _lastJumpWasPressedOnTheGround = !_platformerMotor2D.IsInAir();

        if (currentFartyJumpCharges == 0 || _platformerMotor2D.IsOnGround() || _platformerMotor2D.IsOnCorner() || _lastJumpWasPressedOnTheGround)
            return;

        if ((!MustLandBeforeNextFartyJump ||  _hasLandedSinceLastFartyJump) && Input.GetButton(PC2D.Input.JUMP))
        {
            // Only count up _timeJumpWasHeld, if it hasn't yet reached minTimeToHoldForGasJump.
            // This is because we changed to the _charge up/down thing, so the jump force is no longer calculated directly from the time the button was held.
            if (_timeJumpWasHeld < minTimeToHoldForGasJump)
                _timeJumpWasHeld += Time.deltaTime;

            //if (_fillingUp)
            //{
            //    _timeJumpWasHeld += Time.deltaTime;
            //}
            //else
            //{
            //    _timeJumpWasHeld -= Time.deltaTime;
            //}

            // Only use gas-charge when holding for longer than the minimum time, or if we are already charging.
            if (_timeJumpWasHeld > minTimeToHoldForGasJump)
            {
                if (!_charging)
                {
                    //Debug.Log("Gas charge started");
                    _charging = true;
                    soundScript.StartPressingSound();

                    //Time.timeScale = slowMoAmount;

                    if(cone_background != null)
                        cone_background.fillAmount = 1.0f;
                    if (cone != null)
                        cone.fillAmount = 1.0f;
                }

                if (Time.timeScale > slowMoAmount)
                {
                    MusicManager.Instance.ReducePitch();
                    _slowMoTimer += Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Max(slowMoAmount, 1.0f - ((1.0f - slowMoAmount) * (_slowMoTimer/_timeToFullSlowMo)));
                }

                // The cone is always set to completely full?
                //if (cone != null && cone_col != null && cone_background != null)
                //{
                //    cone.fillAmount = 1;
                //    cone_background.fillAmount = 1.0f;
                //}

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.changeStaminaCircleAmount(0.0340f); //magic number used

                    if (cone != null && cone_col != null && cone_background != null)
                    {
                        _charge += (_fillingUp ? ChargeFillPerSec : -ChargeFillPerSec) * Time.unscaledDeltaTime;

                        if (_charge > 1f)
                        {
                            _charge = 1f;
                            _fillingUp = false;
                        }
                        else if (_charge < 0f)
                        {
                            _charge = 0f;
                            _fillingUp = true;
                        }

                        cone_col.fillAmount = _charge;
                    }
                }
            }

            /*
            if (_timeJumpWasHeld >= maxGasJumpChargeTime)
            {
                GameManager.Instance.changeStaminaCircleAmount(1.0f); //magic number used
                if (cone != null && cone_col != null)
                {
                    cone_col.fillAmount = 1;
                }
                
            }
            */

        }
        else if ((!MustLandBeforeNextFartyJump || _hasLandedSinceLastFartyJump) && _timeJumpWasHeld > minTimeToHoldForGasJump && Input.GetButtonUp(PC2D.Input.JUMP))
        {
            //if (_platformerMotor2D.jumpingHeld)
            //    return;

            //Debug.Log("Build up force released: "+buildUpForce);

            // Calculate the force with which to boost the ninja. Clamps between minBuildUpForce and maxBuildUpForce.
            fireGas(minBuildUpForce + (_charge * (maxBuildUpForce - minBuildUpForce))); //Can hold button down for greater effect
            CancelGasCharge();
        }
        else
        {
            CancelGasCharge();
        }
    }

    private void CancelGasCharge()
    {
        //Debug.Log("CancelGasCharge");
        _charging = false;
        _charge = 0f;
        _fillingUp = true;
        full = false;
        _slowMoTimer = 0f;
        Time.timeScale = 1;
        _timeJumpWasHeld = 0f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.changeStaminaCircleAmount(-1.00f); //magic number used
            if (cone != null && cone_col != null && cone_background != null)
            {
                cone.fillAmount = 0;
                cone_background.fillAmount = 0;
                cone_col.fillAmount = 0;
            }
        }
        soundScript.StopPressingSound();
        MusicManager.Instance.IncreasePitch();
    }

    void fireGas(float force)
    {
        //var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        //mousePosition.z = 0;

        //hit = Physics2D.Raycast(transform.position, mousePosition - transform.position, distance * 10, mask);

        //if (hit.collider != null){
        //var directionalVector = -(mousePosition - transform.position).normalized;
        var directionalVector = Vector3.up;

        //var oldJump = platformerMotor2D.jumpHeight; //Save old jump height

        //platformerMotor2D.dashDuration = 5.0f;

        if (_platformerMotor2D.facingLeft && directionalVector.x > 0.0f)
        {
            _platformerMotor2D.facingLeft = false;
        }

        _hasLandedSinceLastFartyJump = false;

        //move the player
        _platformerMotor2D.velocity = new Vector2(_platformerMotor2D.velocity.x, directionalVector.y * force * (_platformerMotor2D.IsGrounded() ? 1 : airFartMultiplier));

        _platformerMotor2D.ReportCustomJumpExecuted(true);

        soundScript.PlayFartSound();
        particleFxScript.LargeFartEffect.Play();
        
        // Never go below 0 charges (should never happen anyway)
        currentFartyJumpCharges = Mathf.Max(0, currentFartyJumpCharges - 1);
        gasBarFill.fillAmount = (float)currentFartyJumpCharges / maxFartyJumpCharges;

        //LastFartForce = force;
        //platformerMotor2D.jumpHeight = oldJump; //Put jump back
        //}
    }

    //Call this method when you need to draw a line between the mouse and the 
    private void drawLine()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, GetCurrentMousePosition().GetValueOrDefault());
        if (Input.GetKey(KeyCode.Mouse0))
        {
            line.enabled = true;
            //line.SetColors(new Color(_timeJumpWasHeld / maxGasJumpChargeTime, 0.0f, 0.0f, 1.0f), new Color(_timeJumpWasHeld / maxGasJumpChargeTime, 0.0f, 0.0f, 1.0f));
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            line.enabled = false;
        }
    }

    //Get mouse position
    private Vector3? GetCurrentMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        float rayDistance;
        if (plane.Raycast(ray, out rayDistance))
        {
            return ray.GetPoint(rayDistance);
        }
        return null;
    }

}
