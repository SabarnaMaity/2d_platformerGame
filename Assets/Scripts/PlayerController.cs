
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Controls { mobile, pc }

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float doubleJumpForce = 8f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private bool isGroundedBool = false;
    private bool canDoubleJump = false;

    public Animator playeranim;

    public Controls controlmode;

    private float moveX = 0f;
    private float mobileMoveX = 0f;
    private bool isLeftHeld = false;
    private bool isRightHeld = false;

    public bool isPaused = false;

    public ParticleSystem footsteps;
    private ParticleSystem.EmissionModule footEmissions;

    public ParticleSystem ImpactEffect;
    private bool wasonGround;

    public float fireRate = 0.5f; // Time between each shot
    private float nextFireTime = 0f; // Time of the next allowed shot

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playeranim == null)
        {
            playeranim = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        ResetMovementState();
    }

    private void OnDisable()
    {
        ResetMovementState();
    }

    private void ResetMovementState()
    {
        isLeftHeld = false;
        isRightHeld = false;
        mobileMoveX = 0f;
        moveX = 0f;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (playeranim == null)
        {
            playeranim = GetComponentInChildren<Animator>();
        }
        if (footsteps != null)
        {
            footEmissions = footsteps.emission;
        }

        // Ensure groundLayer includes both "ground" layer and "Default" layer
        int groundLayerIndex = LayerMask.NameToLayer("ground");
        if (groundLayerIndex != -1)
        {
            groundLayer |= (1 << groundLayerIndex);
        }
        groundLayer |= (1 << 0); // Default layer

#if UNITY_ANDROID || UNITY_IOS
        controlmode = Controls.mobile;
#endif

        if (UIManager.instance != null)
        {
            UIManager.instance.EnableMobileControls();
        }

        ResetMovementState();
    }

    private void Update()
    {
        // Fail-safe: if running on a touch screen and no touches exist, ensure buttons are not stuck held down
        if (Application.isMobilePlatform && Input.touchCount == 0 && (isLeftHeld || isRightHeld))
        {
            isLeftHeld = false;
            isRightHeld = false;
            UpdateMobileMove();
        }

        isGroundedBool = IsGrounded();

        if (isGroundedBool)
        {
            canDoubleJump = true; // Reset double jump when grounded

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                Jump(jumpForce);
            }
        }
        else
        {
            if (canDoubleJump && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
            {
                Jump(doubleJumpForce);
                canDoubleJump = false; // Disable double jump until grounded again
            }
        }

        if (!isPaused)
        {
            // Handle shooting
            if (controlmode == Controls.pc && Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate; // Set the next allowed fire time
            }
        }

        SetAnimations();

        if (moveX != 0)
        {
            FlipSprite(moveX);
        }

        // ImpactEffect
        if (!wasonGround && isGroundedBool)
        {
            if (ImpactEffect != null)
            {
                ImpactEffect.gameObject.SetActive(true);
                ImpactEffect.Stop();
                if (footsteps != null)
                {
                    ImpactEffect.transform.position = new Vector2(footsteps.transform.position.x, footsteps.transform.position.y - 0.2f);
                }
                ImpactEffect.Play();
            }
        }

        wasonGround = isGroundedBool;
    }

    public void SetAnimations()
    {
        if (playeranim == null) return;

        if (Mathf.Abs(moveX) > 0.05f && isGroundedBool)
        {
            playeranim.SetBool("run", true);
            if (footsteps != null) footEmissions.rateOverTime = 35f;
        }
        else
        {
            playeranim.SetBool("run", false);
            if (footsteps != null) footEmissions.rateOverTime = 0f;
        }

        playeranim.SetBool("isGrounded", isGroundedBool);
    }

    private void FlipSprite(float direction)
    {
        if (direction > 0.05f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (direction < -0.05f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        float move = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Keyboard controls on PC / Unity Editor (avoids Android accelerometer/joystick drift)
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            move -= 1f;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            move += 1f;
        }
#endif

        // On mobile or if no keyboard input, use the on-screen button state
        if (move == 0f)
        {
            move = mobileMoveX;
        }

        moveX = move;
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Zero out vertical velocity
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        if (playeranim != null)
        {
            playeranim.SetTrigger("jump");
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        float rayLength = 0.25f;
        Vector2 rayOrigin = new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "killzone")
        {
            GameManager.instance.Death();
        }
    }

    // Mobile button controls
    public void SetLeftHeld(bool held)
    {
        isLeftHeld = held;
        UpdateMobileMove();
    }

    public void SetRightHeld(bool held)
    {
        isRightHeld = held;
        UpdateMobileMove();
    }

    private void UpdateMobileMove()
    {
        if (isLeftHeld && !isRightHeld)
        {
            mobileMoveX = -1f;
        }
        else if (isRightHeld && !isLeftHeld)
        {
            mobileMoveX = 1f;
        }
        else
        {
            mobileMoveX = 0f;
        }
        moveX = mobileMoveX;
    }

    public void MobileMove(float value)
    {
        mobileMoveX = value;
        moveX = value;
    }

    public void MobileJump()
    {
        if (isGroundedBool)
        {
            Jump(jumpForce);
        }
        else if (canDoubleJump)
        {
            Jump(doubleJumpForce);
            canDoubleJump = false; // Disable double jump until grounded again
        }
    }

    public void Shoot()
    {
        // GameObject fireBall = Instantiate(projectile, firePoint.position, Quaternion.identity);
        // fireBall.GetComponent<Rigidbody2D>().AddForce(firePoint.right * 500f);
    }

    public void MobileShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
}
