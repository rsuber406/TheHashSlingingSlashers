using TMPro;
using UnityEngine;

public class PotentialRewardDisplay : MonoBehaviour
{
    public TextMeshProUGUI rewardText;
    private ScoreSys scoreSys;

    void Start()
    {
        scoreSys = FindFirstObjectByType<ScoreSys>();
        UpdateRewardUI();
    }

    void Update()
    {
        UpdateRewardUI();
    }

    private void UpdateRewardUI()
    {
        float potentialReward = scoreSys.CalculatePotentialReward();
        rewardText.text = "Potential Reward: " + potentialReward.ToString() + "X Multiplier";
    }
}

