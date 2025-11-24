using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public Text scoreText;
    public GameObject winPanel;
    public GameObject gameOverPanel;
    
    public bool isPlaying = true;
    private int score = 0;
    private int maxScore = 7; // Cambiado a 7 puntos para ganar

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        score = 0;
        UpdateScoreUI();
        
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();
        
        if (score >= maxScore)
        {
            Win();
        }
    }
    
    public int GetScore()
    {
        return score;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score + " / " + maxScore;
        }
    }

    void Win()
    {
        isPlaying = false;
        Time.timeScale = 0f;
        
        // renaudar el video cuando gana
        if (VideoController.instance != null)
        {
            VideoController.instance.PlayVideo();
            Debug.Log("Video renaudado - Ganaste!");
        }
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void GameOver()
    {
        isPlaying = false;
        Time.timeScale = 0f;
        
        // Pausar el video cuando pierde
        if (VideoController.instance != null)
        {
            VideoController.instance.PauseVideo();
            Debug.Log("Video pausado - Game Over!");
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        
        // Reanudar el video si existe
        if (VideoController.instance != null)
        {
            VideoController.instance.PlayVideo();
        }
        
        SceneManager.LoadScene("Windowsescritorio"); // ← Cambia este nombre si tu escena se llama diferente
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        // Reanudar el video si existe
        if (VideoController.instance != null)
        {
            VideoController.instance.PlayVideo();
        }
        
        SceneManager.LoadScene("flapibird"); // ← Cambia este nombre si tu escena se llama diferente
    }
}