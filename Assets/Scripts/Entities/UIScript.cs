using System;
using Systems;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Entities
{
    public class UIScript : SingletonBehaviour<UIScript> {

        public Text heightScore, centerText;
        private Vector3 lastPosition;

        public void OnScoreChanged()
        {
            heightScore.text = "Score: " + GameManager.Instance.Score;
        }

        public void SetGameOverGui()
        {
            centerText.text = String.Concat(
                "GAME OVER",
                //"\nHEIGHT POINTS: ", (GameManager.Instance.Score - GameManager.Instance.AdditionalScore),
                //"\nBEAN POINTS: ", GameManager.Instance.AdditionalScore,
                "\nSCORE: ", GameManager.Instance.Score,
                "\nBEST: ", PlayerPrefs.GetInt("bestScore", 0)
                );
            centerText.enabled = true;
        }

        public void SetPauseUi()
        {
            centerText.text = "PAUSED";
            centerText.enabled = true;
        }

        public void RemovePauseUi()
        {
            centerText.enabled = false;
        }
    }
}
