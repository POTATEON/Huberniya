using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Animator _animator;

    private Vector2 movement;



    private float xPosLastFrame;


    private void Start()
    {

    }
    private void Update()
    {
        HandleMovement();
        FlipCharacterX();

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
}
