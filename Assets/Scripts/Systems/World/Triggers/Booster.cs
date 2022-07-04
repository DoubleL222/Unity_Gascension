using UnityEngine;

public class Booster : MonoBehaviour
{
    public float BoostAmount = 1f;
    private PlatformerMotor2D _motor;
    private Transform _player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //_player = other.transform;
            _motor = other.gameObject.GetComponent<PlatformerMotor2D>();
            //_motor.ForceDash(transform.up);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !_motor.IsGrounded() && !_motor.IsOnCorner())
        {
            _motor.velocity += (Vector2)transform.up * BoostAmount * Time.deltaTime;
            //_player.position += transform.up * BoostAmount * Time.deltaTime;
        }
    }
}
