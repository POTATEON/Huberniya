using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator _animator;

    [Header("Combat Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool inBattle = false;

    private Vector2 movement;
    private float xPosLastFrame;
    private Coroutine hitCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!inBattle && !IsPlayingHitAnimation())
        {
            HandleMovement();
            FlipCharacterX();
        }
    }

    private void FlipCharacterX()
    {
        if (transform.position.x > xPosLastFrame)
        {
            spriteRenderer.flipX = true;
        }
        else if (transform.position.x < xPosLastFrame)
        {
            spriteRenderer.flipX = false;
        }
        xPosLastFrame = transform.position.x;
    }

    private void HandleMovement()
    {
        float input = Input.GetAxis("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);

        if (input != 0)
        {
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    public void SetBattleState(bool battleState)
    {
        inBattle = battleState;
        if (inBattle)
        {
            _animator.SetBool("IsWalking", false);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Запускаем анимацию получения урона
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(HitAnimationRoutine());

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UpdateUI();
        }
    }

    private IEnumerator HitAnimationRoutine()
    {
        // Включаем анимацию получения урона
        _animator.SetBool("IsHit", true);

        // Ждем один кадр чтобы аниматор успел обработать изменение
        yield return null;

        // Ждем пока анимация не закончится
        yield return new WaitForSeconds(1f);

        // Выключаем анимацию получения урона
        _animator.SetBool("IsHit", false);
    }

    // Проверяем, проигрывается ли анимация получения урона
    private bool IsPlayingHitAnimation()
    {
        return _animator.GetBool("IsHit");
    }

    private void Die()
    {
        Debug.Log("Player died! Game Over.");

        // Останавливаем анимацию урона
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        _animator.SetBool("IsHit", false);

        // Показываем экран поражения
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver(false);
        }
    }

    // Метод для принудительной остановки анимации
    public void StopHitAnimation()
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        _animator.SetBool("IsHit", false);
    }
}