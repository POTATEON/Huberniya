using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("UI References")]
    public GameObject battleUIPanel;
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI battleLogText;

    [Header("Number Pad")]
    public NumberPad numberPad;

    [Header("Battle Settings")]
    public float messageDisplayTime = 3f; // Время отображения сообщений

    private EnemyController currentEnemy;
    private PlayerMovement player;
    private int correctAnswer;
    private string equationString;
    private bool isEnemyTurn = false;
    private bool isGoidaAttack = false;
    private Coroutine currentMessageCoroutine;

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
        battleUIPanel.SetActive(false);
        player = FindObjectOfType<PlayerMovement>();

        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = player.maxHealth;
            playerHealthSlider.value = player.currentHealth;
        }

        if (battleLogText != null)
        {
            battleLogText.text = "";
        }
    }

    public void StartBattle(EnemyController enemy)
    {
        currentEnemy = enemy;

        if (player != null)
        {
            player.SetBattleState(true);
        }

        battleUIPanel.SetActive(true);

        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = currentEnemy.maxHealth;
            enemyHealthSlider.value = currentEnemy.currentHealth;
        }

        // Сообщение при начале боя отображается дольше
        StartCoroutine(ShowTemporaryMessage("Бой начался! Решайте примеры чтобы атаковать.", 4f));

        UpdateUI();
        GenerateEquation();

        if (numberPad != null)
        {
            numberPad.Show(equationString);
        }
    }

    public void EndBattle()
    {
        if (player != null)
        {
            player.SetBattleState(false);
        }

        battleUIPanel.SetActive(false);

        if (numberPad != null)
        {
            numberPad.Hide();
        }

        currentEnemy = null;
        isEnemyTurn = false;
        isGoidaAttack = false;

        // Останавливаем все корутины сообщений
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
    }

    public void CheckAnswer(int playerAnswer, bool isGoida)
    {
        if (isEnemyTurn || currentEnemy == null) return;

        isGoidaAttack = isGoida;

        if (playerAnswer == correctAnswer)
        {
            if (isGoidaAttack)
            {
                ShowBattleMessage($"С Богом! Игрок наносит сокрушительный удар - 40 урона!");
                currentEnemy.TakeDamage(40);
            }
            else
            {
                ShowBattleMessage($"Игрок решает пример правильно и наносит 20 урона!");
                currentEnemy.TakeDamage(20);
            }
        }
        else
        {
            if (isGoidaAttack)
            {
                ShowBattleMessage($"С Богом проваливается! Враг наносит ответный сокрушительный удар - 30 урона!");
                if (player != null)
                {
                    player.TakeDamage(30);
                }
            }
            else
            {
                ShowBattleMessage($"Игрок ошибается! Правильный ответ: {correctAnswer}");
                if (player != null)
                {
                    StartCoroutine(ShowEnemyCounterAttack());
                }
            }
        }

        UpdateUI();

        // Проверяем конец боя
        if (currentEnemy.currentHealth <= 0)
        {
            ShowBattleMessage("Враг побежден!", 4f);
            StartCoroutine(ShowVictoryScreenWithDelay(2f));
            return;
        }

        if (player != null && player.currentHealth <= 0)
        {
            ShowBattleMessage("Игрок побежден!", 4f);
            StartCoroutine(ShowGameOverScreenWithDelay(2f));
            return;
        }

        // Ход врага
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator ShowGameOverScreenWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndBattle();

        // Показываем экран поражения
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver(false);
        }
    }

    private IEnumerator ShowVictoryScreenWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndBattle();

        // Показываем экран победы
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver(true);
        }
    }

    private IEnumerator ShowEnemyCounterAttack()
    {
        yield return new WaitForSeconds(messageDisplayTime / 2f);
        ShowBattleMessage($"Враг контратакует и наносит 15 урона!");
        player.TakeDamage(15);
        UpdateUI();
    }

    private IEnumerator EnemyTurn()
    {
        isEnemyTurn = true;

        // Ждем пока сообщение игрока полностью отобразится
        yield return new WaitForSeconds(messageDisplayTime);

        if (currentEnemy != null && player != null && player.currentHealth > 0)
        {
            ShowBattleMessage($"Враг атакует и наносит {currentEnemy.attackDamage} урона!");
            currentEnemy.AttackPlayer();
            UpdateUI();
        }

        // Ждем пока сообщение атаки врага отобразится
        yield return new WaitForSeconds(messageDisplayTime);

        if (player != null && player.currentHealth <= 0)
        {
            ShowBattleMessage("Игрок побежден!", 4f);
            EndBattle();
            yield break;
        }

        // Новый раунд
        isEnemyTurn = false;
        isGoidaAttack = false;
        GenerateEquation();

        if (numberPad != null)
        {
            numberPad.Show(equationString);
        }
    }

    private void GenerateEquation()
    {
        int num1 = Random.Range(1, 11);
        int num2 = Random.Range(1, 11);
        int operation = Random.Range(0, 3);

        string opSymbol = "";
        switch (operation)
        {
            case 0:
                correctAnswer = num1 + num2;
                opSymbol = "+";
                break;
            case 1:
                if (num1 < num2) (num1, num2) = (num2, num1);
                correctAnswer = num1 - num2;
                opSymbol = "-";
                break;
            case 2:
                correctAnswer = num1 * num2;
                opSymbol = "×";
                break;
        }

        equationString = $"{num1} {opSymbol} {num2} =";
    }

    // Новый метод для показа сообщений с временем
    private void ShowBattleMessage(string message)
    {
        ShowBattleMessage(message, messageDisplayTime);
    }

    private void ShowBattleMessage(string message, float displayTime)
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        currentMessageCoroutine = StartCoroutine(ShowTemporaryMessage(message, displayTime));
    }

    private IEnumerator ShowTemporaryMessage(string message, float displayTime)
    {
        if (battleLogText != null)
        {
            battleLogText.text = message;
        }

        yield return new WaitForSeconds(displayTime);

        // Очищаем сообщение после указанного времени
        if (battleLogText != null && battleLogText.text == message)
        {
            battleLogText.text = "Введите ответ...";
        }
    }

    private IEnumerator EndBattleWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndBattle();
    }

    public void UpdateUI()
    {
        if (player != null)
        {
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = player.currentHealth;
            }
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{player.currentHealth}/{player.maxHealth}";
            }
        }

        if (currentEnemy != null)
        {
            if (enemyHealthSlider != null)
            {
                enemyHealthSlider.value = currentEnemy.currentHealth;
            }
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{currentEnemy.currentHealth}/{currentEnemy.maxHealth}";
            }
        }
    }

    // Старый метод для обратной совместимости
    public void AddBattleLog(string message)
    {
        ShowBattleMessage(message);
    }

    // Перегруженный метод для обычной проверки ответа
    public void CheckAnswer(int playerAnswer)
    {
        CheckAnswer(playerAnswer, false);
    }
}