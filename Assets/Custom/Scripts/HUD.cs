using MText;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public GameObject[] Lives;
    public Modular3DText Score;
    public Modular3DText HighScoresText;
    public Modular3DText HighestScoreText;
    public GameObject MainMenu;
    public GameObject ScoreEntry;
    public Modular3DText Name;
    private int _currentScore = 0;    

    public void ReduceLives()
    {
        foreach (var item in Lives)
        {
            if (item.activeSelf)
            {
                item.SetActive(false);
                break;
            }
        }
    }

    public void ResetLives()
    {
        foreach (var item in Lives)
        {
            item.SetActive(true);            
        }
    }

    public void IncrementScore(int intScoreToAdd)
    {
        _currentScore += intScoreToAdd;

        Score.UpdateText(_currentScore);
    }

    public void ResetScore()
    {
        Score.UpdateText("00");
    }

    public void SetMainMenuVisible(bool visible)
    {
        MainMenu.SetActive(visible);
    }

    public void SetScoreEntryVisible(bool visible)
    {
        Game.Instance.IsScoreEntry = true;
        ScoreEntry.SetActive(visible);
        Game.Instance.Ship.SetForScoreEntry();
        Name.UpdateText("");
    }

    public IEnumerator ShowScores()
    {
        //Debug.Log("Getting Scores")
        Game.Instance.dl.GetScores();
        List<dreamloLeaderBoard.Score> scoreList = Game.Instance.dl.ToListHighToLow();

        float maxTimeToWait = 5f;
        float t = 0;
        while (t < maxTimeToWait)
        {
            t += Time.deltaTime;
            if (scoreList.Count == 0)
            {
                //GUILayout.Label("(loading...)");
                HighScoresText.UpdateText("Loading...");
                scoreList = Game.Instance.dl.ToListHighToLow();
            }
            else
            {
                int maxToDisplay = 10;
                int count = 0;
                string strScores = "";

                //var currentPlayerScore = scoreList.Where(s => s.playerName.Substring(0, 12) == SystemInfo.deviceUniqueIdentifier.Substring(0, 12) && s.score < Score).FirstOrDefault();

                //if (currentPlayerScore.score > 0)
                //{
                //    currentPlayerScore.score = Score;
                //    scoreList = scoreList.OrderByDescending(s => s.score).ToList();
                //}

                foreach (dreamloLeaderBoard.Score currentScore in scoreList)
                {
                    count++;

                    if (count == 1)
                    {
                        HighestScoreText.UpdateText(currentScore.score.ToString());
                    }

                    strScores += count.ToString("00") + " " + currentScore.playerName + " - " + currentScore.score.ToString();

                    //GUILayout.BeginHorizontal();
                    //GUILayout.Label(currentScore.playerName, width200);
                    //GUILayout.Label(currentScore.score.ToString(), width200);
                    //GUILayout.EndHorizontal();

                    if (count >= maxToDisplay) break;

                    strScores += Environment.NewLine;
                }

                HighScoresText.UpdateText(strScores);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SaveScore()
    {
        StartCoroutine(SaveScoreInternal());
    }

    private IEnumerator SaveScoreInternal()
    {
        Game.Instance.dl.AddScore(Name.text, Convert.ToInt32(Score.text));
        yield return new WaitForSeconds(0.5f);
        ScoreEntered();
    }

    private void ScoreEntered()
    {
        StartCoroutine(ShowScores());
        SetScoreEntryVisible(false);
        SetMainMenuVisible(true);
        Game.Instance.IsScoreEntry = false;
    }
}
