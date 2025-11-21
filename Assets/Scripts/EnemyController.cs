using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float detectionRadius = 2f;
    public LayerMask playerLayer;
    public int maxHealth = 50;
    public int currentHealth;
    public int attackDamage = 10; // Теперь это поле используется

    private Transform player;
    private bool battleStarted = false;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (!battleStarted && !isDead && player != null &&
            Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer))
        {
            StartBattle();
        }
    }

    private void StartBattle()
    {
        if (battleStarted || isDead) return;

        battleStarted = true;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StartBattle(this);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
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

    private void Die()
    {
        isDead = true;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EndBattle();
        }

        Destroy(gameObject);
    }

    public void AttackPlayer()
    {
        if (player == null || isDead) return;

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}