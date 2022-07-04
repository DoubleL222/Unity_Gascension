using UnityEngine;
using UnityEngine.UI;


public class StaminaCircleScript : MonoBehaviour{
    Image fillImg;
    public float maxAmount;
    public float startAmount;
    float amount = 0.50f;
    float time;

    // Use this for initialization
    void Start(){
        fillImg = this.GetComponent<Image>();
        time = 0;
        fillImg.fillAmount = startAmount;
    }

    public void changeAmount(float addAmount){
        time += addAmount;
        if (0 > time){
            time = 0;
        }
        if (maxAmount < time){
            time = maxAmount;
        }
        fillImg.fillAmount = time;
    }
}
