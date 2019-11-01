using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, Damagable
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float groundBaseMoveSpeed, jumpForce, fallMultiplier, lowJumpMultiplier, slideSpeed, bounceBackMagnitude, bounceBackTime, hitInvulnerableTime;
    [SerializeField] private int currentJumpCounter, maxJumpCounter;
    [SerializeField] private float collisionRadius;
    [SerializeField] private Vector2 bottomOffset, leftOffset, rightOffset, topOffset;
    [SerializeField] private float groundCheckWidth;
    [SerializeField] public GameObject surfaceStuckTo;
    [SerializeField] private GameObject jumpSmokePrefab;
    [SerializeField] private Animator anim;
    private float bobOffset;
    public static PlayerBehaviour instance;

    [SerializeField] private int maxHearts, currentHearts;

    [SerializeField] private Transform catchPos;
    public Transform CatchPos
    {
        get { return catchPos; }
    }
    private GameObject lastGround;

    public bool stuckToSurface;

    private float lAngle;

    [SerializeField] private float clampTimeElapsed, clampWalkDelay, clampWalkDistance, catchPosMoveSpeed;

    [SerializeField] private bool beingPulled, beingPushed, invulnerable;
    public bool Invulnerable
    {
        get { return invulnerable; }
    }
    private Vector2 target;
    [SerializeField] private float pulledMagnitude, pushedMagnitude;

    [SerializeField] private float pullMagnitude;

    [SerializeField] private float catchPosX;

    [SerializeField] private float heightChecks;

    public bool pullStartBonus;
    public float PullMagnitude
    {
        get { return pullMagnitude; }
    }

    [SerializeField] private float pushMagnitude;
    public float PushMagnitude
    {
        get { return pushMagnitude; }
    }

    [Space]
    [Header("Booleans")]
    public bool onGround, onWall, onLeftWall, onRightWall, wallJumped, wallSliding, canMove, onCeiling, vaulting, isBouncedBack;

    public int wallSide;
    public int side = 1;

    private Vector2 input;
    
    [Header("Layers")]
    public LayerMask groundLayer;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position + topOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset + Vector2.up * heightChecks, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset + Vector2.up * heightChecks, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset + Vector2.right * groundCheckWidth, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset + Vector2.left * groundCheckWidth, collisionRadius);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*
        MagneticObject objectCollider = collision.GetComponent<MagneticObject>();
        if (objectCollider != null)
        {
            objectCollider.beingPulled = false;
        }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if(collision.gameObject != lastGround)
            {
                if(currentJumpCounter < maxJumpCounter)
                {
                    currentJumpCounter++;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            lastGround = collision.gameObject;
        }
    }

    private void OnEnable()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        anim.SetFloat("Health", maxHearts);
    }

    private IEnumerator WallVault(bool aLeft)
    {
        anim.SetBool("onCorner", true);
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        this.GetComponent<Collider2D>().enabled = false;
        stuckToSurface = false;
        vaulting = true;
        Vector2 lStartPos = (Vector2)transform.position;
        float t = 0;
        while (t <= 1f)
        {
            if (aLeft)
            {
                transform.position = Vector2.Lerp(lStartPos, (lStartPos + Vector2.up * 2.02f), t);
                anim.transform.localScale = new Vector3(-1f, 1f, 1f);
                MoveCatchPos(new Vector3(-catchPosX, catchPos.localPosition.y, catchPos.localPosition.z),2f);
            }
            else if (!aLeft)
            {
                transform.position = Vector2.Lerp(lStartPos, (lStartPos + Vector2.up * 2.02f), t);
                anim.transform.localScale = Vector3.one;
                MoveCatchPos(new Vector3(catchPosX, catchPos.localPosition.y, catchPos.localPosition.z), 2f);

            }
            if(Physics2D.OverlapCircle(catchPos.transform.position,0.1f,1 >> LayerMask.NameToLayer("Ground")))
            {
                MoveCatchPos(new Vector3(0f, catchPos.localPosition.y, catchPos.localPosition.z), 2f);
            }
            yield return new WaitForSeconds(0.01f);
            t += 0.1f;
        }
        lStartPos = (Vector2)transform.position;
        t = 0;
        while (t <= 1f)
        {
            if (aLeft)
            {
                transform.position = Vector2.Lerp(lStartPos, (lStartPos + Vector2.left * 0.7f), t);
            }
            else if (!aLeft)
            {
                transform.position = Vector2.Lerp(lStartPos, (lStartPos + Vector2.right * 0.7f), t);
            }
            yield return new WaitForSeconds(0.01f);
            t += 0.1f;
        }
        vaulting = false;
        this.GetComponent<Collider2D>().enabled = true;
        rb.gravityScale = 1f;
        anim.SetBool("onCorner", false);
    }
    
    // Update is called once per frame
    void Update()
    {

        if (GameManager.instance.CurrentGameState == GameManager.GameState.paused) return;
        lAngle += Time.deltaTime*2f;
        bobOffset = Mathf.Sin(lAngle);
        catchPos.localPosition = new Vector3(catchPos.localPosition.x, 0.35f+(bobOffset * 0.1f), catchPos.localPosition.z);
        anim.SetFloat("Speed", rb.velocity.magnitude);
        if(rb.velocity.x > 0)
        {
            anim.transform.localScale = Vector3.one;
        } else if (rb.velocity.x < 0)
        {
            anim.transform.localScale = new Vector3(-1f,1f,1f);
        }
        HandleCatchPos();

        if (vaulting || isBouncedBack)
            return;

        groundCheck();
        wallCheck();
        ceilingCheck();


        if(!onWall && !onCeiling)
        {
            if (stuckToSurface && input.y > 0 && rb.velocity.y > 0 && !onGround)
            {
                //wall vault        
                bool lLeft = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset + Vector2.up*(heightChecks - 0.15f), collisionRadius, groundLayer);
                bool lRight = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset + Vector2.up * (heightChecks - 0.15f), collisionRadius, groundLayer);
                if(lLeft || lRight)
                    StartCoroutine(WallVault(lLeft));
            }
            stuckToSurface = false;
        }

        if (beingPulled == true)
        {
            Vector2 dir = (target - (Vector2)this.transform.position).normalized;
            rb.velocity = dir * pulledMagnitude;
            if (pullStartBonus) StartCoroutine(PullStart());
            return;
        }
        else if (beingPushed == true)
        {
            StartCoroutine(Pushed());
            return;
        }

        input.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Run();

        anim.SetBool("isSliding", (onWall && !onGround && !stuckToSurface));

        if (onWall && !onGround && stuckToSurface == false)
        {
            if (rb.velocity.y <= 0)
            {
                wallSliding = true;
                WallSlide();
            }
        }

        if (onGround)
        {
            wallSliding = false;
            wallJumped = false;
            currentJumpCounter = maxJumpCounter;
        }

        if(wallJumped == true && onWall == true && !onGround)
        {
            if(currentJumpCounter > 0)
            {
                wallJumped = false;
            }
        }

        rb.gravityScale = 3f;

        if (Input.GetButtonDown("Jump") && canMove)
        {
            //anim.SetTrigger("jump");

            if (onGround)
            {
                Jump(Vector2.up, false);
                Instantiate(jumpSmokePrefab, (Vector2)transform.position + bottomOffset + Vector2.up*0.01f ,Quaternion.identity);
                FindObjectOfType<AudioHandler>().Play("Jump Sound");
            }
            else if (onWall && !onGround)
            {
                WallJump();
                FindObjectOfType<AudioHandler>().Play("Jump Sound");
            }
            else if (onCeiling)
            {
                CeilingJump();
                FindObjectOfType<AudioHandler>().Play("Jump Sound");
            }

        }

        if(!onGround && rb.velocity.y > 0)
        {
            SetFalling(false);
        } else if (!onGround && rb.velocity.y < 0)
        {
            SetFalling(true);
        }

        DynamicAirTime();

    }

    private IEnumerator Pushed()
    {
        Vector2 dir = (target - (Vector2)this.transform.position).normalized;
        rb.velocity = (-dir * pushedMagnitude * 1.5f);
        yield return new WaitForSeconds(0.1f);
        beingPushed = false;
    }

    public void PullStartBonus(Vector2 aTarget)
    {
        beingPulled = true;
        target = aTarget;
    }

    private IEnumerator PullStart()
    {
        Vector2 dir = (target - (Vector2)this.transform.position).normalized;
        rb.velocity = (dir * pushedMagnitude * 1.5f);
        yield return new WaitForSeconds(0.1f);
        pullStartBonus = false;
        beingPulled = false;
    }

    public void channelPull()
    {
        canMove = false;
    }

    public void stopChannel()
    {
        canMove = true;
    }

    private void groundCheck()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset + (Vector2.right*groundCheckWidth), collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset + (Vector2.left*groundCheckWidth), collisionRadius, groundLayer);

        anim.SetBool("isGrounded", onGround);
        if(onGround)
        {
            SetFalling(false);
        }
    }
    private void ceilingCheck()
    {
        onCeiling = Physics2D.OverlapCircle((Vector2)transform.position + topOffset, collisionRadius, groundLayer);

        anim.SetBool("onCeiling", onCeiling);
        if(onCeiling)
        {
            SetFalling(false);
        }
    }

    private void wallCheck()
    {
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset + Vector2.up * heightChecks, collisionRadius, groundLayer);
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset + Vector2.up * heightChecks, collisionRadius, groundLayer);
        onWall = onLeftWall || onRightWall;

        wallSide = onRightWall ? -1 : 1;

        anim.SetBool("onWall", onWall);
        if(onWall && !wallSliding)
        {
            SetFalling(false);
        } else if (onWall && wallSliding)
        {
            SetFalling(true);
        }

        if(onWall)
        {
            anim.transform.localScale = new Vector3(-wallSide,1f,1f);
            if (stuckToSurface)
            {
                anim.transform.localScale = new Vector3(-1f * wallSide, 1f, 1f);
            }
        }
    }
    public void SetFalling(bool aState)
    {
        anim.SetBool("isFalling", aState);
    }
    public void SetHolding(bool aState)
    {
        anim.SetBool("holdingObj", aState);
    }
    public void SetPush(bool aState)
    {
        anim.SetTrigger("push");
    }
    public void SetAimForward(bool aState)
    {
        anim.SetBool("aimingForward", aState);
        anim.SetBool("aimingAbove", false);
        anim.SetBool("aimingBelow", false);
    }
    public void SetAimAbove(bool aState)
    {
        anim.SetBool("aimingForward", false);
        anim.SetBool("aimingBelow", false);
        anim.SetBool("aimingAbove", aState);
    }
    public void SetAimBelow(bool aState)
    {
        anim.SetBool("aimingForward", false);
        anim.SetBool("aimingAbove", false);
        anim.SetBool("aimingBelow", aState);
    }

    private void MoveCatchPos(Vector3 aTarget, float aLerpSpeed)
    {
        catchPos.localPosition = Vector3.Lerp(catchPos.localPosition,aTarget,Time.deltaTime * aLerpSpeed);
    }
    public void SetAimNothing()
    {
        anim.SetBool("aimingForward", false);
        anim.SetBool("aimingBelow", false);
        anim.SetBool("aimingAbove", false);
    }
    public void pullPlayerTowards(Vector2 aTarget)
    {
        beingPulled = true;
        target = aTarget;
    }

    private void HandleCatchPos()
    {
        if (anim.transform.localScale.x > 0 && !onWall)
        {
            MoveCatchPos(new Vector3(catchPosX, catchPos.localPosition.y, catchPos.localPosition.z), catchPosMoveSpeed);
        }
        else if (anim.transform.localScale.x < 0 && !onWall)
        {
            MoveCatchPos(new Vector3(-catchPosX, catchPos.localPosition.y, catchPos.localPosition.z), catchPosMoveSpeed);
        }
        if (onLeftWall)
        {
            MoveCatchPos(new Vector3(catchPosX, catchPos.localPosition.y, catchPos.localPosition.z), catchPosMoveSpeed);
        }
        else if (onRightWall)
        {
            MoveCatchPos(new Vector3(-catchPosX, catchPos.localPosition.y, catchPos.localPosition.z), catchPosMoveSpeed);
        }
        if (Physics2D.OverlapCircle(catchPos.transform.position, 0.05f, 1 << LayerMask.NameToLayer("Ground")) && !onWall)
        {
            MoveCatchPos(new Vector3(0f, catchPos.localPosition.y, catchPos.localPosition.z), catchPosMoveSpeed);
        }
    }

    public void pushPlayerFrom(Vector2 aTarget)
    {
        FindObjectOfType<AudioHandler>().Play("Magnet Push");
        beingPushed = true;
        target = aTarget;
    }

    public void realseExternalForce()
    {
        beingPulled = false;
        beingPushed = false;
        target = Vector2.zero;
        groundCheck();
        wallCheck();
        ceilingCheck();
        DynamicAirTime();
    }

    public void StartBounceBack(Vector2 aForceDir)
    {
        StartCoroutine(BounceBack(aForceDir));
    }
    private IEnumerator BounceBack(Vector2 aForceDir)
    {
        if (isBouncedBack) yield break;
        isBouncedBack = true;
        rb.gravityScale = 1f;
        rb.AddForce(aForceDir * bounceBackMagnitude, ForceMode2D.Impulse);
        yield return new WaitForSeconds(bounceBackTime);
        rb.gravityScale = 0f;
        isBouncedBack = false;
    }
    private IEnumerator SetInvulnerable()
    {
        invulnerable = true;
        yield return new WaitForSeconds(hitInvulnerableTime);
        invulnerable = false;
    }

    public void MovePlayerTowards(Vector2 aPoint)
    {
        rb.MovePosition(aPoint);
    }

    public void Damage(int amount)
    {
        if(!invulnerable)
        {
            CameraControl.instance.Shake(0.15f, 8, 7f);
            currentHearts -= amount;
            anim.SetFloat("Health", currentHearts);
            StartCoroutine(PlayHitAnim());
            if (currentHearts <= 0)
            {
                FindObjectOfType<AudioHandler>().Stop("Game Music");
                FindObjectOfType<AudioHandler>().DelayedPlay("Loss Music");
                Die();
                return;
            }
            StartCoroutine(SetInvulnerable());
        }
    }

    private IEnumerator PlayHitAnim()
    {
        anim.SetBool("isHit", true);
        yield return new WaitForSeconds(0.6f);
        anim.SetBool("isHit", false);
    }

    public void AddHealth(int amount)
    {
        currentHearts += amount;
        if(currentHearts > maxHearts)
        {
            currentHearts = maxHearts;
        }
    }

    public void Die()
    {
        rb.velocity = Vector2.zero;
        GameManager.instance.LoseLevel();
    }

    private void Run()
    {
        if (canMove == false) return;

        if(Input.GetMouseButton(0) && stuckToSurface) // Being Pulled
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if(!stuckToSurface)
        {
            anim.SetBool("isClimbingDown", false);
        } else if (stuckToSurface && !onGround && onWall && rb.velocity.y > 0)
        {
            anim.SetBool("isClimbingDown", false);
        }

        if (stuckToSurface)
        {
            if(onGround && onWall)
            {
                if(input.y < 0)
                {
                    rb.velocity = Vector2.zero;
                    return;
                } else if (input.x > 0 && onRightWall)
                {
                    rb.velocity = Vector2.zero;
                    return;
                } else if (input.x < 0 && onLeftWall)
                {
                    rb.velocity = Vector2.zero;
                    return;
                }
            }

            if(!onGround)
            {
                if (onCeiling)
                {
                    if(input.y > 0)
                    {
                        rb.velocity = Vector2.zero;
                        return;
                    }
                    rb.velocity = new Vector2(input.x * groundBaseMoveSpeed / 3f, 0f);
                    if (input.y < 0) CeilingJump();
                }
                else if (onWall)
                {
                    rb.velocity = new Vector2(0f, input.y * groundBaseMoveSpeed / 3f);
                    if (rb.velocity.y < 0)
                    {
                        anim.SetBool("isClimbingDown", true);
                    }
                    if ((input.x > 0 && onLeftWall) || (input.x < 0 && onRightWall))
                    {
                        WallJump();
                    }
                }

                if (onCeiling && onWall)
                {
                    if (input.y > 0)
                    {
                        rb.velocity = Vector2.zero;
                        return;
                    }
                    else if (input.x < 0 && onLeftWall)
                    {
                        rb.velocity = Vector2.zero;
                        return;
                    }
                    else if (input.x > 0 && onRightWall)
                    {
                        rb.velocity = Vector2.zero;
                        return;
                    }
                }
                return;
            }

        }

        if (stuckToSurface && onGround)
        {
            if(onWall)
            {
                rb.velocity = new Vector2(input.x*groundBaseMoveSpeed, input.y * groundBaseMoveSpeed / 3f);
            }
        }

        if (wallJumped == false)
        {
            if (onWall && currentJumpCounter > 0 && !onGround)
            {
                if ((input.x > 0 && onLeftWall) || (input.x < 0 && onRightWall))
                {
                    WallJump();
                    return;
                }
            }

            if (onGround && onWall && !stuckToSurface)
            {
                if (onLeftWall && input.x < 0)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
                else if(onRightWall && input.x > 0)
                {
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }else if ((onRightWall && input.x < 0) || (onLeftWall && input.x > 0) )
                {
                    rb.velocity = new Vector2(input.x * groundBaseMoveSpeed, rb.velocity.y);
                }
            }
            else
            {
                if (onGround)
                {
                    rb.velocity = new Vector2(input.x * groundBaseMoveSpeed, rb.velocity.y);
                } else if (input.x != 0 && !onGround)
                {
                    rb.velocity = new Vector2(input.x * groundBaseMoveSpeed, rb.velocity.y);
                }
                else if (input.x == 0 && !onGround)
                {
                    rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x,-groundBaseMoveSpeed/2,groundBaseMoveSpeed/2), rb.velocity.y);
                }
            }

        } else
        {
            if(input.x != 0 && !onGround)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(input.x * groundBaseMoveSpeed, rb.velocity.y), 5f * Time.deltaTime);
            }
        }
    }

    private void WallSlide()
    {
        /*
        if (wallSide != side)
            anim.Flip(side * -1);

        
        if (!canMove)
            return;
        */
        bool pushingWall = false;
        if ((rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);

    }

    private void Jump(Vector2 dir, bool wall, bool aCeiling = false)
    {
        //slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        //ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        if (currentJumpCounter > 0 && !stuckToSurface)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
            currentJumpCounter--;
            anim.SetBool("isFalling", true);
        }else if(stuckToSurface)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
            stuckToSurface = false;
            anim.SetBool("isFalling", true);
        }
    }

    private void WallJump()
    {
        if ((side == 1 && onRightWall) || side == -1 && !onRightWall)
        {
            side *= -1;
            //anim.Flip(side);
        }

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up * 1f + wallDir * 1f), true);

        wallJumped = true;

        if ((currentJumpCounter > 0 && !stuckToSurface) || stuckToSurface) Instantiate(jumpSmokePrefab, (Vector2)transform.position + ((side == -1) ? rightOffset : leftOffset) + Vector2.down * 0.8f, Quaternion.Euler(0f,0f, ((side == -1) ? 90f : 270f)));

    }

    private void CeilingJump()
    {
        Jump((Vector2.down * .5f), true);
    }

    private void DynamicAirTime()
    {
        if(stuckToSurface)
        {
            rb.gravityScale = 0f;
            return;
        }
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

}
