using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverDescriptionText;
    public Button restartButton;
    public Button menuButton;

    [Header("Game Over Messages")]
    public string playerDefeatedTitle = "Вас порешали!";
    public string playerDefeatedDescription = "Ваши математические навыки не смогли противостоять врагу...";
    public string playerVictoryTitle = "Победа!";
    public string playerVictoryDescription = "Вы успешно решили все примеры и победили врага!";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Скрываем панель при старте
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Настраиваем кнопки
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ReturnToMenu);
        }
    }

    // Показать экран поражения
    public void ShowGameOver(bool isVictory = false)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Устанавливаем текст в зависимости от результата
        if (isVictory)
        {
            if (gameOverTitleText != null)
                gameOverTitleText.text = playerVictoryTitle;
            if (gameOverDescriptionText != null)
                gameOverDescriptionText.text = playerVictoryDescription;
        }
        else
        {
            if (gameOverTitleText != null)
                gameOverTitleText.text = playerDefeatedTitle;
            if (gameOverDescriptionText != null)
                gameOverDescriptionText.text = playerDefeatedDescription;
        }

        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Останавливаем игру (пауза)
        Time.timeScale = 0f;
    }

    // Перезапуск текущей сцены
    public void RestartGame()
    {
        // Возвращаем нормальную скорость игры
        Time.timeScale = 1f;

        // Перезагружаем текущую сцену
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Вернуться в главное меню
    public void ReturnToMenu()
    {
        // Возвращаем нормальную скорость игры
        Time.timeScale = 1f;

        // Загружаем сцену главного меню
        SceneManager.LoadScene("MainMenu");
    }

    // Скрыть экран game over
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
}