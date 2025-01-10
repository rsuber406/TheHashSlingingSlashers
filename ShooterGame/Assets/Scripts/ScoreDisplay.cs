using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{

    public TextMeshProUGUI scoreText;
    private ScoreSys scoreSys;
    void Start()
    {
         scoreSys = FindFirstObjectByType<ScoreSys>();
        UpdateScoreUI();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreSys != null)
        {
            scoreText.text = "Score: " + scoreSys.GetScore().ToString();

        }
    }
}
