using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour, Damagable
{
    [SerializeField] LayerMask visionBlockers;
    [SerializeField] LayerMask Damagables;
    [SerializeField] LayerMask laserInterupts;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 aimTarget;
    private bool playerInSight;
    [SerializeField] private float elapsedAlertTime, alertTime, targetingSpeed, elapsedFiringTime, maxFiringTime, firingTargetSpeed, beamLengthScalar, beamLength, timeSinceLastFire, fireCooldownTime;
    private bool firingBeam;
    [SerializeField] private SpriteRenderer laserBeamRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject shotgunBulletPrefab;
    [SerializeField] private float currentHealth, maxHealth;
    [SerializeField] private Transform turretFireLeft, turretFireRight, turretFireUp, turretFireDown;
    [SerializeField] private RuntimeAnimatorController HorizontalController, VerticalController;
    [SerializeField] private GameObject chargeShot;
    [SerializeField] private GameObject metalFragPrefab;
    private GameObject chargeShotObject;
    public enum TurretAxis
    {
        vertical,
        horizontal
    }
    [SerializeField] private TurretAxis axis;

    public enum TurretAimDirection
    {
        up,
        down,
        left,
        right
    }
    [SerializeField] private TurretAimDirection turretFacing;

    public enum TurretType
    {
        Shotgun,
        Laser
    }
    [SerializeField] private TurretType turretType;

    private Coroutine firingRoutine;

    private void Awake()
    {
        FindObjectOfType<AudioHandler>().Play("Turret Spawn");
    }

    // Start is called before the first frame update
    void Start()
    {

        switch(axis)
        {
            case TurretAxis.horizontal:
                anim.runtimeAnimatorController = HorizontalController;
                break;
            case TurretAxis.vertical:
                anim.runtimeAnimatorController = VerticalController;
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.instance.CurrentGameState == GameManager.GameState.paused) return;

        if(axis == TurretAxis.horizontal)
        {
            RaycastHit2D leftHit = Physics2D.Raycast(turretFireLeft.position, Vector2.left, 100f, visionBlockers);

            RaycastHit2D rightHit = Physics2D.Raycast(turretFireRight.position, Vector2.right, 100f, visionBlockers);

            bool lLeftSight = false;
            bool lRightSight = false;

            if (leftHit)
            {
                PlayerBehaviour player = leftHit.collider.GetComponent<PlayerBehaviour>();
                if (player != null)
                {
                    lLeftSight = true;

                } else
                {
                    lLeftSight = false;
                }
            }
            if (rightHit)
            {
                PlayerBehaviour player = rightHit.collider.GetComponent<PlayerBehaviour>();
                if (player != null)
                {
                    lRightSight = true;
                } else
                {
                    lRightSight = false;
                }
            }
            if(lLeftSight || lRightSight)
            {
                if (!playerInSight)
                {
                    elapsedAlertTime = 0f;
                    timeSinceLastFire = fireCooldownTime;
                }
                if (!firingBeam)
                {
                    if (turretFacing != TurretAimDirection.left && lLeftSight)
                    {
                        anim.SetBool("Rotate", false);
                        turretFacing = TurretAimDirection.left;
                        timeSinceLastFire = -0.4f;
                    }
                    else if (turretFacing != TurretAimDirection.right && lRightSight)
                    {
                        anim.SetBool("Rotate", true);
                        turretFacing = TurretAimDirection.right;
                        timeSinceLastFire = -0.4f;
                    }
                }
                playerInSight = true;
            }
            else
            {
                playerInSight = false;
            }
        }

        if(axis == TurretAxis.vertical)
        {
            if(turretFacing == TurretAimDirection.up)
            {
                anim.transform.localScale = new Vector3(1f, -1f, 1f);
            } else
            {
                anim.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            RaycastHit2D downHit = Physics2D.Raycast(turretFireDown.position, Vector2.down, 100f, visionBlockers);

            RaycastHit2D upHit = Physics2D.Raycast(turretFireDown.position, Vector2.up, 100f, visionBlockers);

            bool lDownSight = false;
            bool lUpSight = false;

            if (downHit)
            {
                PlayerBehaviour player = downHit.collider.gameObject.GetComponent<PlayerBehaviour>();
                if (player != null)
                {
                    lDownSight = true;
                }
                else
                {
                    lDownSight = false;
                }
            }
            if (upHit)
            {
                PlayerBehaviour player = upHit.collider.gameObject.GetComponent<PlayerBehaviour>();
                if (player != null)
                {
                    lUpSight = true;
                }
                else
                {
                    lUpSight = false;
                }
            }
            if (lDownSight || lUpSight)
            {
                if (!playerInSight)
                {
                    elapsedAlertTime = 0f;
                    timeSinceLastFire = fireCooldownTime;
                }
                playerInSight = true;

            }
            else
            {
                playerInSight = false;
            }
        }

        if(playerInSight == true)
        {
            if (elapsedAlertTime < alertTime)
            {
                elapsedAlertTime += Time.deltaTime;
                if(chargeShotObject == null && turretType == TurretType.Laser)
                {
                    Vector2 lFireStart = Vector2.zero;
                    float lFireAngle = 0f;
                    switch (turretFacing)
                    {
                        case TurretAimDirection.right:
                            lFireStart = turretFireRight.position;
                            lFireAngle = 0f;
                            break;
                        case TurretAimDirection.left:
                            lFireStart = turretFireLeft.position;
                            lFireAngle = 180f;
                            break;
                        case TurretAimDirection.up:
                            lFireStart = turretFireDown.position;
                            lFireAngle = 90f;
                            break;
                        case TurretAimDirection.down:
                            lFireStart = turretFireDown.position;
                            lFireAngle = -90f;
                            break;
                    }
                    chargeShotObject = Instantiate(chargeShot, lFireStart, Quaternion.Euler(0f, 0f, lFireAngle));
                }
            }
            else
            {
                if (firingBeam == false)
                {
                    firingBeam = true;
                    if(turretType == TurretType.Laser)
                    {
                        Destroy(chargeShotObject);
                        chargeShotObject = null;
                    }
                }

            }
        } else
        {
            if(chargeShotObject != null)
            {
                Destroy(chargeShotObject);
                chargeShotObject = null;
            }
        }

        if(firingBeam)
        {
            FireAtAimTarget();
        }
    }

    private void FireAtAimTarget()
    {
        if(turretType == TurretType.Laser)
        {
            if (elapsedFiringTime < maxFiringTime)
            {
                elapsedFiringTime += Time.deltaTime;
                anim.SetBool("Fire", true);
                firingBeam = true;
            } else
            {
                anim.SetBool("Fire", false);
                firingBeam = false;
                FindObjectOfType<AudioHandler>().Stop("Laser Fire");
                beamLength = 0f;
                laserBeamRenderer.size = Vector2.up*laserBeamRenderer.size.y;
                elapsedFiringTime = 0f;
                return;
            }

            if (elapsedFiringTime <= 0.1f)
            {
                FindObjectOfType<AudioHandler>().Play("Laser Fire");
            }

            Vector2 lFireDir = Vector2.zero;
            Vector2 lFireStart = Vector2.zero;
            switch(turretFacing)
            {
                case TurretAimDirection.right:
                    lFireStart = turretFireRight.position;
                    lFireDir = Vector2.right;
                    laserBeamRenderer.transform.position = turretFireRight.position;
                    break;
                case TurretAimDirection.left:
                    lFireStart = turretFireLeft.position;
                    lFireDir = Vector2.left;
                    laserBeamRenderer.transform.position = turretFireLeft.position;
                    break;
                case TurretAimDirection.up:
                    lFireStart = turretFireDown.position;
                    lFireDir = Vector2.up;
                    laserBeamRenderer.transform.position = turretFireDown.position;
                    break;
                case TurretAimDirection.down:
                    lFireStart = turretFireDown.position;
                    lFireDir = Vector2.down;
                    laserBeamRenderer.transform.position = turretFireDown.position;
                    break;
            }

            RaycastHit2D hit = Physics2D.Raycast(lFireStart, lFireDir, beamLength, laserInterupts);
            if(hit)
            {
                //TODO: Instantiate Laser Wall Hit anim object here
                PlayerBehaviour lPlayer = hit.collider.GetComponent<PlayerBehaviour>();
                if(lPlayer != null)
                {
                    if (!lPlayer.Invulnerable)
                    {
                        lPlayer.StartBounceBack(lFireDir*0.5f + Vector2.up*0.5f);
                        lPlayer.Damage(1);
                    }
                }
                float lDistToHit = Vector2.Distance(lFireStart, hit.point);
                beamLength = Mathf.Clamp(beamLength, -lDistToHit, lDistToHit);
            } else
            {
                beamLength += Time.deltaTime * beamLengthScalar;
            }

            laserBeamRenderer.size = Vector2.left * beamLength * ((turretFacing == TurretAimDirection.down) ? -1 : 1) + Vector2.up*laserBeamRenderer.size.y;
            float lAngle = Vector2.Angle(lFireDir, Vector2.right);
            laserBeamRenderer.gameObject.transform.localEulerAngles = Vector3.forward * ((lFireDir.y < 0) ? lAngle : -lAngle);

        }

        if(turretType == TurretType.Shotgun)
        {
            if (!playerInSight)
            {
                firingBeam = false;
                anim.SetBool("Fire", false);
                return;
            }

            bool lFireThisFrame = false;
            if (timeSinceLastFire < fireCooldownTime)
            {
                timeSinceLastFire += Time.deltaTime;
                lFireThisFrame = false;
                anim.SetBool("Fire", false);
            }
            else
            {
                lFireThisFrame = true;
                anim.SetBool("Fire", true);
            }

            if (!lFireThisFrame)
                return;

            Vector2 lFireDir = Vector2.zero;
            Vector2 lFireStart = Vector2.zero;
            switch (turretFacing)
            {
                case TurretAimDirection.right:
                    lFireStart = turretFireRight.position;
                    lFireDir = Vector2.right;
                    break;
                case TurretAimDirection.left:
                    lFireStart = turretFireLeft.position;
                    lFireDir = Vector2.left;
                    break;
                case TurretAimDirection.up:
                    lFireStart = turretFireDown.position;
                    lFireDir = Vector2.up;
                    break;
                case TurretAimDirection.down:
                    lFireStart = turretFireDown.position;
                    lFireDir = Vector2.down;
                    break;
            }

            float lAngle = ((lFireDir.y < 0) ? 1 : -1) * Vector2.Angle(lFireDir, Vector2.right);
            GameObject lBulletObject = Instantiate(shotgunBulletPrefab, lFireStart, Quaternion.Euler(0f,0f,lAngle));
            FindObjectOfType<AudioHandler>().Play("Turret Bullet");
            lBulletObject.GetComponent<ShotgunProjectile>().SetCourse(lFireDir);
            lBulletObject.GetComponent<ShotgunProjectile>().SetParent(gameObject);

            timeSinceLastFire = 0f;
        }
    }

    public void AddHealth(int amount)
    {
        if(currentHealth < maxHealth)
        {
            currentHealth++;
        }
    }

    public void Damage(int amount)
    {
        currentHealth--;
        anim.SetTrigger("Damage");
        if(currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        anim.SetBool("Die", true);
        yield return new WaitForSeconds(0.1f);
        CameraControl.instance.Shake(0.15f, 8, 7f);
        yield return new WaitForSeconds(0.5f);
        MetalFrag frag = GameManager.instance.SpawnMetalFrag(transform.position, Quaternion.identity);
        frag.spawnParent = gameObject;
        int randF_X = Random.Range(-10, 10);
        int randF_Y = Random.Range(-10, 10);
        frag.GetComponent<Rigidbody2D>().velocity = new Vector2(randF_X / 2f, randF_Y / 2f);
        EnemyWaveManager.instance.RemoveEnemy(this.gameObject);
        Destroy(this.gameObject);
    }
}
