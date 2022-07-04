using UnityEngine;
using Systems;

public class MoveUpwardsIfStationary : MonoBehaviour {
    private float _timer;
    private float _lastY;
    public float TimeBeforeMoving = 0.5f;
    public float MovementPerSecond = 300f;

    // Use this for initialization
    void Start ()
    {
        _lastY = transform.position.y;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        if (!Mathf.Approximately(_lastY, transform.position.y))
        {
            _timer = 0f;
            _lastY = transform.position.y;
        }
        else if (_timer < TimeBeforeMoving)
            _timer += Time.deltaTime;

        if (_timer >= TimeBeforeMoving)
        {
            transform.position += new Vector3(0f, MovementPerSecond * Time.deltaTime, 0f);
            _lastY = transform.position.y;
        }
    }
}
