using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject mainMenuPanel;
    public Button startButton;
    public Button exitButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI versionText;

    [Header("Scene Names")]
    public string gameSceneName = "GameScene";

    [Header("Menu Settings")]
    public string gameTitle = "Математический Бой";
    public string gameVersion = "Версия 1.0";

    private void Start()
    {
        InitializeMenu();
    }

    private void InitializeMenu()
    {
        // Устанавливаем заголовок и версию
        if (titleText != null)
            titleText.text = gameTitle;

        if (versionText != null)
            versionText.text = gameVersion;

        // Настраиваем кнопки
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Убеждаемся, что время течет нормально (на случай возврата из паузы)
        Time.timeScale = 1f;

        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        Debug.Log("Запуск игры...");

        // Загружаем игровую сцену
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Если имя сцены не указано, пытаемся загрузить следующую по порядку
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogError("Не найдена игровая сцена!");
            }
        }
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    // Метод для возврата в меню из игры
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
