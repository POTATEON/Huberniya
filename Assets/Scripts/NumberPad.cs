using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NumberPad : MonoBehaviour
{
    [Header("Number Pad UI")]
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI equationText;
    public GameObject numberPadPanel;

    [Header("Buttons")]
    public Button[] numberButtons;
    public Button backspaceButton;
    public Button submitButton;
    public Button clearButton;
    public Button goidaButton;

    private string currentInput = "0";
    private const int MAX_DIGITS = 3;
    private bool canSubmit = true;

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < numberButtons.Length; i++)
        {
            if (numberButtons[i] != null)
            {
                int number = i;
                numberButtons[i].onClick.AddListener(() => AddNumber(number));
            }
        }

        if (backspaceButton != null)
            backspaceButton.onClick.AddListener(Backspace);

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearInput);

        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitNumber);

        if (goidaButton != null)
            goidaButton.onClick.AddListener(GoidaAttack);
    }

    public void AddNumber(int number)
    {
        if (!canSubmit) return;

        if (currentInput.Length < MAX_DIGITS)
        {
            if (currentInput == "0")
            {
                currentInput = number.ToString();
            }
            else
            {
                currentInput += number.ToString();
            }
            UpdateDisplay();
        }
    }

    public void Backspace()
    {
        if (!canSubmit) return;

        if (currentInput.Length > 1)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        else
        {
            currentInput = "0";
        }
        UpdateDisplay();
    }

    public void ClearInput()
    {
        if (!canSubmit) return;

        currentInput = "0";
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = currentInput;
        }
    }

    public void SubmitNumber()
    {
        if (!canSubmit || BattleManager.Instance == null) return;

        if (int.TryParse(currentInput, out int result) && currentInput != "0")
        {
            canSubmit = false;
            BattleManager.Instance.CheckAnswer(result, false); // Обычная атака
            ClearInput();
            canSubmit = true;
        }
        else
        {
            BattleManager.Instance.AddBattleLog("Введите число больше 0!");
        }
    }

    public void GoidaAttack()
    {
        if (!canSubmit || BattleManager.Instance == null) return;

        if (int.TryParse(currentInput, out int result) && currentInput != "0")
        {
            canSubmit = false;
            StartCoroutine(GoidaAttackCoroutine(result));
        }
        else
        {
            BattleManager.Instance.AddBattleLog("Введите число больше 0!");
        }
    }

    private IEnumerator GoidaAttackCoroutine(int result)
    {
        // Показываем текст Гойда
        BattleManager.Instance.AddBattleLog("ГОЙДА!!!");

        // Ждем 2 секунды чтобы увидеть сообщение ГОЙДА
        yield return new WaitForSeconds(2f);

        // Выполняем проверку ответа с флагом атаки Гойда
        BattleManager.Instance.CheckAnswer(result, true);
        ClearInput();
        canSubmit = true;
    }

    public void Show(string equation)
    {
        if (numberPadPanel != null)
        {
            numberPadPanel.SetActive(true);
        }

        if (equationText != null)
        {
            equationText.text = equation;
        }

        ClearInput();
        canSubmit = true;
    }

    public void Hide()
    {
        if (numberPadPanel != null)
        {
            numberPadPanel.SetActive(false);
        }
        canSubmit = false;
    }
}