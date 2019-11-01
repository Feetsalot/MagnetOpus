using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, Damagable
{
    [SerializeField] private float speed, dropSpeed, alertTime, explosionRadius, currentHealth, maxHealth, patrolSpeed, chaseSpeed, startExplodeRadius;
    [SerializeField] private float nextWaypointDist;
    [SerializeField] LayerMask visionBlockers;
    [SerializeField] LayerMask Damagables;
    [SerializeField] private Transform target;
    [SerializeField] private Transform leftSightRange, rightSightRange;
    [SerializeField] private List<Transform> targets;
    [SerializeField] private int targetIndex;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MagneticObject mag;
    private Transform followTarget;
    private bool hitLevel, attacking, onSurface, explodeOnContact, followingTarget;
    [SerializeField] private float collisionRadius;
    [SerializeField] private Collider2D collider;
    [SerializeField] private Animator anim;
    [SerializeField] private int explosionDamage;
    [SerializeField] private PathManager.Path patrolPath;
    [SerializeField] private GameObject alertPrefab;
    [SerializeField] private float countdownTime;
    [SerializeField] private GameObject metalFragPrefab;
    private bool countingDown;
    [SerializeField] private bool exploding, targetPlayerOnRelease;

    private bool backPath;
    private void Start()
    {
        targetIndex = 0;
        anim.SetBool("isDetectingPlayer", false);
        anim.SetBool("isChasingPlayer", false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position, collisionRadius);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void SetPath(PathManager.Path lPath, int lIndex = 0)
    {
        patrolPath = lPath;
        targets = patrolPath.wayPoints;
        targetIndex = lIndex;
    }

    private void Update()
    {
        if (GameManager.instance.CurrentGameState == GameManager.GameState.paused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (exploding) return;

        if(explodeOnContact && !mag.isAttachedPlayer)
        {
            LayerMask lEnemyMask = LayerMask.GetMask("Enemy");
            ContactFilter2D lFilter = new ContactFilter2D();
            lFilter.useLayerMask = true;
            lFilter.layerMask = (targetPlayerOnRelease) ? Damagables : lEnemyMask;
            Collider2D[] contactResults = new Collider2D[10];
            int lNumCol =  Physics2D.OverlapCircle(rb.position, 0.5f, lFilter, contactResults);
            if(lNumCol > 0)
            {
                foreach (Collider2D lColl in contactResults)
                {
                    if (lColl != null && lColl != collider)
                    {
                        StartExplosion();
                        break;
                    }
                }
            }
            contactResults = new Collider2D[10];
        }

        bool lNewSurf = onSurface;
        onSurface = Physics2D.OverlapCircle(rb.position, collisionRadius, 1 << LayerMask.NameToLayer("Ground"));
        if(lNewSurf != onSurface)
        {
            SetClosestPath();
        }
        if(mag != null)
        {
            if(mag.isAttachedPlayer)
            {
                if (countingDown == false) StartCoroutine(ExplodeCountdown());
                attacking = false;
                followingTarget = false;
                followTarget = null;
                onSurface = false;
                explodeOnContact = true;
                targetPlayerOnRelease = false;
            }
            if(mag.beingPulled || mag.beingPushed || mag.isAttachedPlayer)
            {
                attacking = false;
                followingTarget = false;
                followTarget = null;
                onSurface = false;
                return;
            }
        }

        if(explodeOnContact)
        {
            return;
        }

        if (!onSurface)
        {
            rb.gravityScale = 1f;
            //return;
        }

        if(followingTarget)
        {
            FollowTarget();
            return;
        }

        if (attacking)
        {
            return;
        }

        SeekPlayer();
        if (onSurface && !attacking)
        {
            Patrol();
        }
    }

    private void SetClosestPath()
    {
        PathManager.Path currentPath = PathManager.instance.GetClosestActivePath(rb.position);
        SetPath(currentPath);
        SetClosestWaypoint();
    }

    private void SetClosestWaypoint(bool notCurrent = false)
    {
        int lClosest = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            float lDistToI = Vector2.Distance(rb.position, targets[i].position);
            float lDistToClosest = Vector2.Distance(rb.position, targets[lClosest].position);
            if (lDistToI < lDistToClosest)
            {
                RaycastHit2D hit = Physics2D.Raycast(rb.position, (Vector2)targets[i].position - rb.position, lDistToClosest, visionBlockers);
                if(notCurrent)
                {
                    if (targetIndex == i)
                    {

                    } else
                    {
                        if(!hit) lClosest = i;
                    }
                } else
                {
                    if(!hit) lClosest = i;
                }
            }
        }
        targetIndex = lClosest;
    }

    private void FollowTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)rb.position, (Vector2)followTarget.position - (Vector2)rb.position, 100f, visionBlockers);
        if(hit)
        {
            if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                attacking = false;
                rb.velocity = Vector2.zero;
                followingTarget = false;
                followTarget = null;
                return;
            }
        }
        if(rb.position.x > followTarget.position.x - startExplodeRadius && rb.position.x < followTarget.position.x + startExplodeRadius )
        {
            if(followTarget.position.y > rb.position.y + startExplodeRadius*1.5 || followTarget.position.y < rb.position.y - startExplodeRadius*1.5)
            {
                attacking = false;
                rb.velocity = Vector2.zero;
                followingTarget = false;
                followTarget = null;
                SetClosestPath();
                return;
            }
            rb.velocity = Vector2.zero;
            followingTarget = false;
            StartExplosion();
            return;
        }
        Vector2 dir = new Vector2(followTarget.position.x - rb.position.x, 0f).normalized;
        rb.velocity = new Vector2(dir.x * chaseSpeed, rb.velocity.y);
    }

    private void SeekPlayer()
    {
        RaycastHit2D hitBelow = Physics2D.Raycast(rb.position,Vector2.down,100f, visionBlockers);
        if(hitBelow)
        {
            if (hitBelow.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                FindObjectOfType<AudioHandler>().DelayedPlay("Self Destruct");
                FindObjectOfType<AudioHandler>().Play("Alerted");
                StartCoroutine(DropAttack());
                return;
            }
        }
        RaycastHit2D hitRight = Physics2D.Linecast(rb.position, rightSightRange.position, visionBlockers);
        bool rightHit = false;
        if (hitRight) rightHit = (hitRight.collider.gameObject.layer == LayerMask.NameToLayer("Player"));
        RaycastHit2D hitLeft = Physics2D.Linecast(rb.position,leftSightRange.position, visionBlockers);
        bool leftHit = false;
        if (hitLeft) leftHit = (hitLeft.collider.gameObject.layer == LayerMask.NameToLayer("Player"));
        hitLevel = rightHit || leftHit;
        if(hitLevel)
        {
            if (rightHit)
            {
                FindObjectOfType<AudioHandler>().Play("Alerted");
                FindObjectOfType<AudioHandler>().Play("IntruderV1");
                StartCoroutine(Attack(hitRight.collider.transform));
                return;
            }
            if(leftHit)
            {
                FindObjectOfType<AudioHandler>().Play("Alerted");
                FindObjectOfType<AudioHandler>().Play("IntruderV2");
                StartCoroutine(Attack(hitLeft.collider.transform));
                return;
            }
        }
        anim.SetBool("isChasingPlayer", false);
        anim.SetBool("isDetectingPlayer", false);
    }

    private IEnumerator Attack(Transform aTarget)
    {
        Instantiate(alertPrefab, transform.position, Quaternion.identity);
        anim.SetBool("isDetectingPlayer", true);
        attacking = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(alertTime);
        anim.SetBool("isChasingPlayer", true);
        rb.gravityScale = 1f;
        followTarget = aTarget;
        followingTarget = true;
    }

    private IEnumerator DropAttack()
    {
        Instantiate(alertPrefab, transform.position, Quaternion.identity);
        anim.SetBool("isDetectingPlayer", true);
        attacking = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(alertTime);
        anim.SetBool("isChasingPlayer", true);
        anim.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        anim.transform.localPosition = new Vector2(0f, -0.5f);
        rb.gravityScale = 1f;
        explodeOnContact = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (   collision.gameObject.layer == LayerMask.NameToLayer("Ground")
            || collision.gameObject.layer == LayerMask.NameToLayer("Player")
            || collision.gameObject.layer == LayerMask.NameToLayer("Enemy" ) )
        {
            if (explodeOnContact)
            {
                StartExplosion();
            }
        }

    }

    private IEnumerator ExplodeCountdown()
    {
        countingDown = true;
        Instantiate(alertPrefab, transform.position, Quaternion.identity);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        explodeOnContact = true;
        anim.SetBool("isDetectingPlayer", true);
        anim.SetBool("Countdown", true);
        yield return new WaitForSeconds(countdownTime);
        mag.releaseFromPlayer();
        if(!exploding) StartExplosion();
    }

    private void StartExplosion()
    {
        if (exploding) return;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        anim.SetBool("Primed", true);
        anim.SetBool("isDetectingPlayer", true);
        StartCoroutine(Explode());
        FindObjectOfType<AudioHandler>().Play("Boom Bot Explosion");
    }

    public IEnumerator Explode()
    {
        exploding = true;
        rb.velocity = Vector2.zero;
        mag.SetClickBuffer(false);
        yield return new WaitForSeconds(0.01f * anim.speed);
        CameraControl.instance.Shake(0.15f, 8, 7f);
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = Damagables;
        Collider2D[] collResults = new Collider2D[20];
        Physics2D.OverlapCircle(transform.position, explosionRadius, filter, collResults);
        foreach (Collider2D coll in collResults)
        {
            if (coll != null)
            {
                Damagable dmg = coll.gameObject.GetComponent<Damagable>();
                if (coll != collider && dmg != null)
                {
                    if (coll.GetComponent<PlayerBehaviour>() != null)
                    {
                        Vector2 lBounceDir = (coll.transform.position - transform.position).normalized;
                        if(!PlayerBehaviour.instance.Invulnerable) PlayerBehaviour.instance.StartBounceBack(lBounceDir*2f);
                    }
                    dmg.Damage(explosionDamage);
                }
            }
        }

        int lCount = 0;
        while(lCount < 2)
        {
            MetalFrag frag = GameManager.instance.SpawnMetalFrag(rb.position, Quaternion.identity);
            frag.spawnParent = gameObject;
            int randF_X = Random.Range(-10, 10);
            int randF_Y = Random.Range(-10, 10);
            frag.GetComponent<Rigidbody2D>().velocity = new Vector2(randF_X / 2f, randF_Y / 2f);
            lCount++;
        }

        yield return new WaitForSeconds(1.123f * anim.speed);
        Die();
    }

    private void Die()
    {
        EnemyWaveManager.instance.RemoveEnemy(this.gameObject);
        Destroy(this.gameObject);
    }

    private void Patrol()
    {
        rb.gravityScale = 0f;
        if (targetIndex >= targets.Count)
        {
            backPath = true;
            targetIndex = targets.Count - 1;
        }
        else if(targetIndex <= 0)
        {
            backPath = false;
            targetIndex = 0;
        }

        Vector2 dir = ((Vector2)targets[targetIndex].position - rb.position).normalized;

        if (!attacking)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (dir * patrolSpeed), 10 * Time.deltaTime);
            anim.SetFloat("Speed", rb.velocity.magnitude);
        }

        bool lOnRightWall = Physics2D.OverlapCircle(rb.position + Vector2.right*0.5f,0.1f,1 << LayerMask.NameToLayer("Ground"));
        bool lOnLeftWall = Physics2D.OverlapCircle(rb.position + Vector2.left * 0.5f, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        bool lOnCeiling = Physics2D.OverlapCircle(rb.position + Vector2.up * 0.5f, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        bool lOnFloor = Physics2D.OverlapCircle(rb.position + Vector2.down * 0.5f, 0.1f, 1 << LayerMask.NameToLayer("Ground"));

        if(lOnFloor)
        {
            anim.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            anim.transform.localPosition = new Vector2(0f, -0.5f);
            if (rb.velocity.x > 0)
            {
                anim.transform.localScale = new Vector2(1f, 1f);
            } else
            {
                anim.transform.localScale = new Vector2(-1f, 1f);
            }
        }
        if(lOnRightWall)
        {
            anim.transform.eulerAngles = new Vector3(0f, 0f, 90f);
            anim.transform.localPosition = new Vector2(0.5f, 0f);
            if(rb.velocity.y > 0)
            {
                anim.transform.localScale = new Vector2(1f, 1f);
            }
            else
            {
                anim.transform.localScale = new Vector2(-1f, 1f);
            }
        }
        if(lOnLeftWall)
        {
            anim.transform.eulerAngles = new Vector3(0f, 0f, 270f);
            anim.transform.localPosition = new Vector2(-0.5f, 0f);
            if (rb.velocity.y < 0)
            {
                anim.transform.localScale = new Vector2(1f, 1f);
            }
            else
            {
                anim.transform.localScale = new Vector2(-1f, 1f);
            }
        }
        if (lOnCeiling)
        {
            anim.transform.eulerAngles = new Vector3(0f, 0f, 180f);
            anim.transform.localPosition = new Vector2(0f, 0.5f);
            if (rb.velocity.x > 0)
            {
                anim.transform.localScale = new Vector2(-1f, 1f);
            }
            else
            {
                anim.transform.localScale = new Vector2(1f, 1f);
            }
        }

        float dist = Vector2.Distance(rb.position, targets[targetIndex].position);
        if (dist < nextWaypointDist)
        {
            if(!backPath) targetIndex++;
            if (backPath) targetIndex--;
        }
    }

    public void Damage(int amount)
    {
        if(!exploding)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                StartExplosion();
                return;
            }
        }
    }

    public void AddHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
