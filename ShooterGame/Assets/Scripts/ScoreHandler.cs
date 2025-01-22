using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    public static ScoreHandler SHinstance;
    [SerializeField] TMP_Text LevelCompleteText;
    [SerializeField] TMP_Text topScore;
    [SerializeField] TMP_Text curScore;
    [SerializeField] TMP_Text timeToComplete;
    [SerializeField] TMP_Text enemiesKilled;
    public void CalculateScore(int completedlvl, int ScoreIn)
    {
        StartCoroutine(DisplayScores(completedlvl, ScoreIn));
        //TODO: Add time and Enemies Killed to the score screen as parameters
    }
    private IEnumerator DisplayScores(int completedlvl, int ScoreIn)
    {
        LevelCompleteText.text = "Level " + completedlvl + " Completed!";
        yield return new WaitForSeconds(1f);
        timeToComplete.text = "Time: To be Determined";
        timeToComplete.enabled = true;
        yield return new WaitForSeconds(1f);
        enemiesKilled.text = "Enemies Killed: To be Determined";
        enemiesKilled.enabled = true;
        yield return new WaitForSeconds(1f);
        curScore.text = "Total Score: " + curScore;
        curScore.enabled = true;
        yield return new WaitForSeconds(1f);
        topScore.text = "Top Score: " + curScore; //TODO: need code to read from a txt file.
        topScore.enabled = true;
        yield return new WaitForSeconds(1f);
    }
}
