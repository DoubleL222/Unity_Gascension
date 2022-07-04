using Systems;
using UnityEngine;

namespace Player
{
    public class GameOverChecker : MonoBehaviour {
        Camera _sceneMainCamera;
        // Use this for initialization
        void Start () {
            _sceneMainCamera = Camera.main;
            if (_sceneMainCamera == null)
            {
                _sceneMainCamera = FindObjectOfType<Camera>();
            }
        }
	
        // Update is called once per frame
        void Update () {
            if (transform.position.y < _sceneMainCamera.ScreenToWorldPoint(new Vector3(0, 0, _sceneMainCamera.nearClipPlane)).y - 0.5f && !GameManager.Instance.IsGameOver)
            {
                GameManager.Instance.setGameOver();
            }
        }
    }
}