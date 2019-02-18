using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField]
    private Text placeText;
    [SerializeField]
    private Text timerText;

    public void SetInfo(int place, int placesLeft, float timeLeft)
    {
        placeText.text = "Place:"+place.ToString();
        int timer = Mathf.CeilToInt(timeLeft);
        timerText.text = "Time Left:"+timer.ToString();

        float redAmt = 1 - Mathf.Clamp01((timer-5) / 30.0f);
        timerText.color = Color.Lerp(Color.black, Color.red, redAmt);
    }

    void Update()
    {
        if(BRGameController.CurrentRaceState() == RaceState.RACING)
        {
            placeText.enabled = true;
            timerText.enabled = true;
        }
        else
        {
            placeText.enabled = false;
            timerText.enabled = false;
        }
    }
}