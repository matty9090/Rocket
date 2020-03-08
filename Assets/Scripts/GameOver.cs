using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField]
    private GameObject NewHighscore = null;

    [SerializeField]
    private GameObject NoHighscore = null;

    [SerializeField]
    private TMPro.TextMeshProUGUI NoHighscoreTXt;

    [SerializeField]
    private TMPro.TextMeshProUGUI TxtScore;

    public void ShowHighscore(int score)
    {
        TxtScore.text = "" + score;
        NewHighscore.SetActive(true);
        NoHighscore.SetActive(false);
    }

    public void ShowNoHighscore(int score, int hs)
    {
        NoHighscoreTXt.text = $"Highscore is {hs}, you scored";
        TxtScore.text = "" + score;
        NewHighscore.SetActive(false);
        NoHighscore.SetActive(true);
    }
}
