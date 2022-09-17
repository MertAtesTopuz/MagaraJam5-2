using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    #region MainVariables
    [Header("Main")]
    public float mainGravity;
    private Rigidbody2D rb;
    private Animator anim;

    #endregion
    
    #region OpenVariables
    [Header("Open")]
    public bool variJumpOpen;
    public bool dashOpen;

    #endregion

    #region LayerMaskVariables
    [Header("Layer Masks")]
    public LayerMask whatIsGround;
    public LayerMask whatIsCorner;

    #endregion

    #region SoundVariables
    [Header("Sounds")]
    public AudioSource audioSrc;
    public AudioClip walkSound;
    public AudioClip attackSound;
    public AudioClip jumpSound;

    #endregion

    #region WalkVariables
    [Header("Walk")]
    public float speed;
    [HideInInspector] public float moveInput;
    [HideInInspector] public float heightInput;
    private bool faceRight = true;
    private float direction = 1;

    #endregion

    #region JumpVariables
    [Header("Jump")]
    public int extraJumpsValue;
    private int extraJumps;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float movementForceInAir;
    public float jumpForce;
    public float airDragMultiplier = 0.9f;
    private float jumpPressedRemember = 0;
    public float jumpPressedRememberTime = 0.2f;
    [Range(0, 1)] public float cutJumpHeight;
    private bool canJump;
    public float normalJumpTime = 1f;
    private bool variJump = true;
    public float dJumpDustTime = 0.1f;
    public ParticleSystem dJumpDustParticle;
    private bool dJumpAnim;
    private bool jumpAnim;
    [HideInInspector] public float yVelocity;

    #endregion

    #region GroundVariables
    [Header("Ground Check")]
    public Transform groundCheck;
    private bool isGrounded;
    private float groundedRemember = 0;
    public float groundedRememberTime = 0.2f;
    public float groundCheckRadius;

    #endregion

    #region CornerCorrectionVariables
    [Header("Corner Correction")]
    [SerializeField] private float topRaycastLength;
    [SerializeField] private Vector3 edgeRaycastOffset;
    [SerializeField] private Vector3 innerRaycastOffset;
    private bool canCornerCorrect;

    #endregion

    #region DashVariables
    [Header("Dash")]
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;
    IEnumerator dashCoroutine;
    public ParticleSystem dashParticle;
    public float distanceBetweenImages;
    private float lastImageXPos;

    #endregion

    #region AttackVariables
    [Header("Attack")]
    private bool attackAnim;

    #endregion


    #region MainMethods
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        extraJumps = extraJumpsValue;
        rb.gravityScale = mainGravity;
    }

    private void Update()
    {
        CheckInput();
        CheckDirection();
        CheckIfCanJump();
        TimeManager();
        FastFall();
        Jump(Vector2.up);
        Attack();
        //AnimationControl();

        if (variJumpOpen && variJump)
        {
            VariableJump();
        }
    }

    private void FixedUpdate()
    {
        CheckSurroundings();
        DashForce();

        if (canCornerCorrect)
        {
            CornerCorrect(rb.velocity.y);
        }
    }
    #endregion

    #region CheckMethods
    private void CheckDirection()
    {
        if (faceRight && moveInput < 0)
        {
            Flip();
        }

        else if (!faceRight && moveInput > 0)
        {
            Flip();
        }
    }

    private void CheckInput()
    {
        //MoveInput
        moveInput = Input.GetAxisRaw("Horizontal");
        heightInput = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        //DirectionCheck
        if (moveInput != 0)
        {
            direction = moveInput;
        }

        //Jump Inputs
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressedRemember = jumpPressedRememberTime;
            jumpAnim = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * cutJumpHeight);
            }
        }

        //Double Jump
        if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 1)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            dJumpAnim = true;
            extraJumps--;
        }

        else if (Input.GetKeyDown(KeyCode.Space) && extraJumps == 0)
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }

        //Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash == true)
        {
            if (dashOpen)
            {
                if (dashCoroutine != null)
                {
                    StopCoroutine(dashCoroutine);
                }

                dashCoroutine = Dash(dashingTime, dashingCooldown);
                StartCoroutine(dashCoroutine);
            }
        }
    }
    #endregion

    #region MovementMethods

    void Flip()
    {
        faceRight = !faceRight;
        transform.Rotate(0f, 180f, 0f);

    }

    #endregion

    #region AttackMethods

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("isAttack");
        }
    }

    #endregion

    #region JumpMethods
    private void Jump(Vector2 direction)
    {
        if (canJump && extraJumps > 0)
        {
            groundedRemember = 0;
            jumpPressedRemember = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded)
        {
            groundedRemember = groundedRememberTime;
            extraJumps = extraJumpsValue;
            dJumpAnim = false;
        }

        if ((groundedRemember > 0) && rb.velocity.y <= 0 && (jumpPressedRemember > 0))
        {
            canJump = true;
        }

        else
        {
            canJump = false;
        }
    }

    private void VariableJump()
    {
        if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void FastFall()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private IEnumerator ReturneNormalJump()
    {
        variJump = false;
        yield return new WaitForSeconds(normalJumpTime);
        variJump = true;
    }
    #endregion

    #region DashMethods
    private void DashForce()
    {
        if (isDashing)
        {
            rb.AddForce(new Vector2(direction * dashingPower, 0), ForceMode2D.Impulse);

            if (Mathf.Abs(transform.position.x - lastImageXPos) > distanceBetweenImages)
            {
                AfterImagePool.instance.GetFromPool();
                lastImageXPos = transform.position.x;
            }
        }
    }

    private IEnumerator Dash(float dashTime, float dashCooldown)
    {
        Vector2 originalVelocity = rb.velocity;

        canDash = false;
        isDashing = true;

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        dashParticle.Play();

        AfterImagePool.instance.GetFromPool();
        lastImageXPos = transform.position.x;

        yield return new WaitForSeconds(dashTime);

        dashParticle.Stop();

        isDashing = false;

        rb.gravityScale = mainGravity;
        rb.velocity = originalVelocity;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    #endregion

    #region SoundMethods
    private void CharacterRunSound()
    {
        audioSrc.PlayOneShot(walkSound);
    }

    private void CharacterAttackSound()
    {
        audioSrc.PlayOneShot(attackSound);
    }

    #endregion

    #region OtherMethods
    private void AnimationControl()
    {
       /* anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (jumpAnim)
        {
            anim.SetBool("isJumping", true);
        }

        else
        {
            anim.SetBool("isJumping", false);
        }

        if (rb.velocity.y < 0f)
        {
            anim.SetFloat("heightInput", 0f);
        }

        if (dJumpAnim)
        {
            anim.SetBool("isDJumping", true);
            jumpAnim = false;
        }

        if (isGrounded)
        {
            anim.SetBool("isGrounded", true);
            anim.SetBool("isDJumping", false);
            anim.SetBool("isJumping", false);
        }

        else
        {
            anim.SetBool("isGrounded", false);
        }

        if (isDashing == true)
        {
            anim.SetBool("isDashing", true);
            anim.SetBool("isGrounded", false);
            anim.SetBool("isFalling", false);
            anim.SetBool("WallGrab", false);
            anim.SetBool("isJumping", false);
            anim.SetBool("isDJumping", false);
            anim.SetFloat("heightInput", 0f);
        }
        else
        {
            anim.SetBool("isDashing", false);
        }*/
    }

    private void TimeManager()
    {
        jumpPressedRemember -= Time.deltaTime;
        groundedRemember -= Time.deltaTime;
    }

    private void CheckSurroundings()
    {
        //Ground Collision
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        //Corner Collision
        canCornerCorrect = Physics2D.Raycast(transform.position + edgeRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) &&
                           !Physics2D.Raycast(transform.position + innerRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) ||
                           Physics2D.Raycast(transform.position - edgeRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner) &&
                           !Physics2D.Raycast(transform.position - innerRaycastOffset, Vector2.up, topRaycastLength, whatIsCorner);
    }

    private void OnDrawGizmos()
    {
        //Ground Check
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        //Corner Check
        Gizmos.DrawLine(transform.position + edgeRaycastOffset, transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position - edgeRaycastOffset, transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position + innerRaycastOffset, transform.position + innerRaycastOffset + Vector3.up * topRaycastLength);
        Gizmos.DrawLine(transform.position - innerRaycastOffset, transform.position - innerRaycastOffset + Vector3.up * topRaycastLength);

        //Corner Distance Check
        Gizmos.DrawLine(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength,
                        transform.position - innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.left * topRaycastLength);
        Gizmos.DrawLine(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength,
                        transform.position + innerRaycastOffset + Vector3.up * topRaycastLength + Vector3.right * topRaycastLength);
    }

    private void CornerCorrect(float Yvelocity)
    {
        //Push Right
        RaycastHit2D hit = Physics2D.Raycast(transform.position - innerRaycastOffset + Vector3.up * topRaycastLength,
            Vector3.left, topRaycastLength, whatIsCorner);

        if (hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength,
                transform.position - edgeRaycastOffset + Vector3.up * topRaycastLength);

            transform.position = new Vector3(transform.position.x + newPos, transform.position.y, transform.position.z);

            rb.velocity = new Vector2(rb.velocity.x, Yvelocity);

            return;
        }

        //Push Left
        hit = Physics2D.Raycast(transform.position + innerRaycastOffset + Vector3.up * topRaycastLength,
            Vector3.right, topRaycastLength, whatIsCorner);

        if (hit.collider != null)
        {
            float newPos = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * topRaycastLength,
                transform.position + edgeRaycastOffset + Vector3.up * topRaycastLength);

            transform.position = new Vector3(transform.position.x - newPos, transform.position.y, transform.position.z);

            rb.velocity = new Vector2(rb.velocity.x, Yvelocity);
        }
    }
    #endregion

}

/*
 */