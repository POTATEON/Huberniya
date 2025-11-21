using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public Button exitButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI versionText;
    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    [Header("Video Intro")]
    public GameObject videoPanel;
    public RawImage videoRawImage; // Добавляем ссылку на RawImage
    public VideoPlayer videoPlayer;
    public Button skipButton;
    public TextMeshProUGUI skipText;
    public float skipButtonShowDelay = 3f;
    public KeyCode skipKey = KeyCode.Escape;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip menuMusic;

    [Header("Animation")]
    public Animator canvasAnimator;
    public float minLoadingTime = 2f;

    private bool isLoading = false;
    private bool isVideoPlaying = false;
    private Coroutine skipButtonCoroutine;
    private RenderTexture videoRenderTexture;

    void Start()
    {
        InitializeMenu();
        PlayMenuMusic();
    }

    void InitializeMenu()
    {
        Debug.Log("Инициализация главного меню");

        // Настраиваем тексты
        if (titleText != null)
            titleText.text = "МАТЕМАТИЧЕСКИЙ БОЙ";

        if (versionText != null)
            versionText.text = $"Версия {Application.version}";

        // Настраиваем кнопки
        SetupButtons();

        // Настраиваем видео систему
        SetupVideoSystem();

        // Скрываем панели
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (videoPanel != null)
            videoPanel.SetActive(false);

        // Проверяем EventSystem
        CheckEventSystem();

        // Убеждаемся, что время течет нормально
        Time.timeScale = 1f;

        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void SetupVideoSystem()
    {
        if (videoPlayer != null && videoRawImage != null)
        {
            // Создаем Render Texture если его нет
            if (videoPlayer.targetTexture == null)
            {
                CreateRenderTexture();
            }

            // Назначаем Render Texture в RawImage
            videoRawImage.texture = videoPlayer.targetTexture;

            // Устанавливаем цвет RawImage в белый
            videoRawImage.color = Color.white;

            // Настраиваем обработчики событий видео
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.errorReceived += OnVideoError;

            Debug.Log("Видео система настроена");
        }
        else
        {
            Debug.LogError("VideoPlayer или VideoRawImage не назначены!");
        }
    }

    void CreateRenderTexture()
    {
        // Создаем Render Texture с размерами 1920x1080
        videoRenderTexture = new RenderTexture(1920, 1080, 24);
        videoRenderTexture.name = "VideoRenderTexture";

        // Назначаем Render Texture видео плееру
        videoPlayer.targetTexture = videoRenderTexture;

        Debug.Log("Render Texture создан: 1920x1080");
    }

    void SetupButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                StartVideoIntro();
            });
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                ExitGame();
            });
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                SkipVideo();
            });
            skipButton.gameObject.SetActive(false);
        }

        Debug.Log("Кнопки настроены успешно");
    }

    void CheckEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem создан автоматически");
        }
    }

    void Update()
    {
        if (isVideoPlaying && Input.GetKeyDown(skipKey))
        {
            SkipVideo();
        }
    }

    void StartVideoIntro()
    {
        if (isLoading || isVideoPlaying) return;

        Debug.Log("Запуск видео-интро");

        // Скрываем основное меню
        if (startButton != null) startButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (versionText != null) versionText.gameObject.SetActive(false);

        // Убеждаемся, что RawImage настроен правильно
        if (videoRawImage != null)
        {
            videoRawImage.color = Color.white;
            videoRawImage.texture = videoPlayer.targetTexture;
        }

        // Показываем панель видео
        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
            isVideoPlaying = true;
        }

        // Скрываем кнопку пропуска
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        // Запускаем видео
        if (videoPlayer != null)
        {
            StartCoroutine(PlayVideoRoutine());
        }
        else
        {
            Debug.LogWarning("VideoPlayer не назначен, переходим сразу к игре");
            StartCoroutine(LoadGameAsync());
        }
    }

    IEnumerator PlayVideoRoutine()
    {
        Debug.Log("Подготовка видео...");

        // Убеждаемся, что RawImage виден
        if (videoRawImage != null)
        {
            videoRawImage.color = Color.white;
        }

        // Подготавливаем видео
        videoPlayer.Prepare();

        // Ждем пока видео подготовится
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("Видео подготовлено, начинаем воспроизведение");

        // Воспроизводим видео
        videoPlayer.Play();

        // Проверяем, что видео воспроизводится
        yield return new WaitForSeconds(0.1f);

        if (!videoPlayer.isPlaying)
        {
            Debug.LogWarning("Видео не воспроизводится, проверьте настройки");
            SkipVideo();
            yield break;
        }

        // Запускаем корутину для показа кнопки пропуска
        if (skipButtonCoroutine != null)
            StopCoroutine(skipButtonCoroutine);
        skipButtonCoroutine = StartCoroutine(ShowSkipButtonAfterDelay());

        Debug.Log("Видео воспроизводится успешно");
    }

    IEnumerator ShowSkipButtonAfterDelay()
    {
        yield return new WaitForSeconds(skipButtonShowDelay);

        if (skipButton != null && isVideoPlaying)
        {
            skipButton.gameObject.SetActive(true);

            if (skipText != null)
            {
                skipText.text = $"Пропустить ({skipKey})";
            }

            Debug.Log("Кнопка пропуска показана");
        }
    }

    void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("Видео завершено автоматически");
        StartCoroutine(LoadGameAsync());
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        Debug.Log("Видео подготовлено к воспроизведению");

        // Убеждаемся, что RawImage настроен
        if (videoRawImage != null && source.targetTexture != null)
        {
            videoRawImage.texture = source.targetTexture;
            videoRawImage.color = Color.white;
        }
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Ошибка видео: {message}");
        SkipVideo();
    }

    void SkipVideo()
    {
        if (!isVideoPlaying) return;

        Debug.Log("Пропуск видео");

        // Останавливаем видео
        if (videoPlayer != null)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Stop();
        }

        // Останавливаем корутину показа кнопки
        if (skipButtonCoroutine != null)
        {
            StopCoroutine(skipButtonCoroutine);
        }

        // Запускаем загрузку игры
        StartCoroutine(LoadGameAsync());
    }

    IEnumerator LoadGameAsync()
    {
        if (isLoading) yield break;

        isLoading = true;
        isVideoPlaying = false;

        Debug.Log("Начинаем загрузку игры...");

        // Скрываем видео панель
        if (videoPanel != null)
            videoPanel.SetActive(false);

        // Показываем панель загрузки
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        AsyncOperation asyncOperation;

        if (!string.IsNullOrEmpty(gameSceneName) && Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            asyncOperation = SceneManager.LoadSceneAsync(gameSceneName);
        }
        else
        {
            int gameSceneIndex = 1;
            if (gameSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                asyncOperation = SceneManager.LoadSceneAsync(gameSceneIndex);
            }
            else
            {
                Debug.LogError("Игровая сцена не найдена!");
                isLoading = false;
                if (loadingPanel != null)
                    loadingPanel.SetActive(false);
                yield break;
            }
        }

        asyncOperation.allowSceneActivation = false;

        float timer = 0f;

        while (timer < minLoadingTime || asyncOperation.progress < 0.9f)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (loadingText != null)
                loadingText.text = $"Загрузка... {Mathf.Round(progress * 100)}%";

            yield return null;
        }

        asyncOperation.allowSceneActivation = true;
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    void PlayMenuMusic()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры");
        StartCoroutine(ExitWithDelay(0.5f));
    }

    IEnumerator ExitWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }

        // Освобождаем Render Texture
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
        }
    }
}