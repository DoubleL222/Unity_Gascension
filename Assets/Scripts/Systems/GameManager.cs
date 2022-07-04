using System.Collections;
using Entities;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Systems
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public bool IsGameOver { get; private set; }
        public GameObject staminaCircle;
        public GameObject playerObject;
        public UIScript uiScript;
        private PlatformerMotor2D _playerMotor;
        private Transform _playerTransform;
        private float _lastPlayerY;
        private float _playerStartY;
        public int Score { get; private set; }
        public int AdditionalScore { get; private set; }

        private bool _isPaused = false;

        public void changeStaminaCircleAmount(float amount)
        {
            if (staminaCircle != null && staminaCircle.activeSelf)
            {
                staminaCircle.GetComponent<StaminaCircleScript>().changeAmount(amount);
            }
        }

        public void setGameOver()
        {
            IsGameOver = true;
            if (Score > PlayerPrefs.GetInt("bestScore", 0))
                PlayerPrefs.SetInt("bestScore", Score);

            StartCoroutine(deathEffect(1.2f));
            if(uiScript)
                uiScript.SetGameOverGui();

            if (_playerMotor)
            {
                // Make the motor think it's grounded, so the GasController and all the other stuff stops.
                _playerMotor.onGrounded();
                _playerMotor.frozen = true;
            }
        }

        private IEnumerator deathEffect(float delay)
        {
            playerObject.GetComponentInChildren<SoundScript>().PlayDeathFartSound();
            if(MusicManager.Instance)
                MusicManager.Instance.PlayGameOverJingle();
            playerObject.GetComponent<PS_Fart>().PlayDeathFartEffect();
            yield return new WaitForSeconds(delay);
            playerObject.GetComponent<PS_Fart>().PlayDeathBloodEffect();
            StopCoroutine(deathEffect(0));
        }

        public void AddToScore(int amount)
        {
            AdditionalScore += amount;
            UpdateScore();
        }

        private void UpdateScore()
        {
            // Add together the AdditionalScore and (units the player has moved / 0.25 units, floored to an int). So every 0.25 units the player moes upwards, he gets a point.
            Score = AdditionalScore + (int)((_lastPlayerY - _playerStartY)/0.25f);
            uiScript.OnScoreChanged();
        }

        private void Start()
        {
            StartCoroutine(PauseCoroutine());
            if (!playerObject)
                playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject)
            {
                _playerTransform = playerObject.transform;
                _playerMotor = playerObject.GetComponent<PlatformerMotor2D>();
            }

            _playerStartY = _playerTransform.position.y;
        }

        private void Update()
        {
            if (IsGameOver && Input.GetKeyDown("space"))
            {
                SceneManager.LoadScene(1);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(1);
            }

            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }

            if (_playerTransform.position.y > _lastPlayerY)
            {
                _lastPlayerY = _playerTransform.position.y;
                UpdateScore();
            } 
        }

        IEnumerator PauseCoroutine()
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.P) && !IsGameOver)
                {
                    if (_isPaused)
                    {
                        Debug.Log("Unpaused");
                        uiScript.RemovePauseUi();
                        Time.timeScale = 1;
                        _isPaused = false;
                    }
                    else
                    {
                        Debug.Log("Paused");
                        Time.timeScale = 0;
                        uiScript.SetPauseUi();
                        _isPaused = true;
                    }
                }
                yield return null;
            }
        }

        public bool IsPaused()
        {
            return _isPaused;
        }
    }
}