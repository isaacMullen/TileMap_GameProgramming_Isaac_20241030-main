using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour
{
    public GameObject gamePanel;
    public GameObject combatPanel;
    public GameObject gameOverPanel;


    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;

    public FishTracker fishTracker;   
    
    // Start is called before the first frame update
    void Start()
    {
        gameOverPanel.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void EndGameDisplayUI()
    {
        gamePanel.SetActive(false);
        combatPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        

        gameOverText.SetText($"You Lost!");
        
        if(fishTracker.totalFish != 0)
        {
            scoreText.SetText($"Score: {fishTracker.totalFish}");
        }
        else
        {
            scoreText.SetText($"No Fish Caught...");
        }
        scoreText.enabled = true;
        gameOverText.enabled = true;

    }
}
