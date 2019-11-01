using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserFollow : MonoBehaviour, Damagable
{
    [SerializeField] LayerMask visionBlockers;
    [SerializeField] LayerMask Damagables;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 aimTarget;
    private bool playerInSight;
    [SerializeField] private float elapsedTargetingTime, maxTargetingTime, targetingSpeed, elapsedFiringTime, maxFiringTime, firingTargetSpeed, beamLengthScalar, beamLength;
    private bool firingBeam;
    [SerializeField] private SpriteRenderer laserBeamRenderer;
    public enum TurretType
    {
        Shotgun,
        Laser
    }
    [SerializeField] private TurretType turretType;

    private Coroutine firingRoutine;

    // Start is called before the first frame update
    void Start()
    {
        target = PlayerBehaviour.instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, 100f, visionBlockers);
        if (hit)
        {
            PlayerBehaviour player = hit.collider.GetComponent<PlayerBehaviour>();
            if (player != null)
            {
                if (!playerInSight)
                {
                    aimTarget = (Vector2)hit.point;
                    elapsedTargetingTime = 0f;
                }
                playerInSight = true;
            }
            else
            {
                playerInSight = false;
                elapsedTargetingTime = 0f;
                beamLength = 0f;
                laserBeamRenderer.size = Vector2.zero;
                elapsedFiringTime = 0f;
                firingBeam = false;
            }
        }

        if (playerInSight == true)
        {
            if (elapsedTargetingTime < maxTargetingTime)
            {
                elapsedTargetingTime += Time.deltaTime;
                aimTarget = Vector2.Lerp(aimTarget, (Vector2)target.position, Time.deltaTime * targetingSpeed);
            }
            else
            {
                FireAtAimTarget();
            }
        }
        else
        {

        }
    }

    private void FireAtAimTarget()
    {
        if (turretType == TurretType.Laser)
        {
            if (firingBeam == false)
            {
                //Start of firing
            }
            if (elapsedFiringTime < maxFiringTime)
            {
                elapsedFiringTime += Time.deltaTime;
                aimTarget = Vector2.Lerp(aimTarget, (Vector2)target.position, Time.deltaTime * firingTargetSpeed);
                firingBeam = true;
            }
            else
            {
                firingBeam = false;
                beamLength = 0f;
                laserBeamRenderer.size = Vector2.zero;
                elapsedFiringTime = 0f;
                elapsedTargetingTime = 0f;
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, beamLength, visionBlockers);
            if (hit)
            {
                //Instantiate Laser Wall Hit anim object here
                PlayerBehaviour lPlayer = hit.collider.GetComponent<PlayerBehaviour>();
                if (lPlayer != null)
                {
                    lPlayer.Damage(1);
                }
            }
            else
            {
                beamLength = elapsedFiringTime * beamLengthScalar;
            }

            laserBeamRenderer.size = Vector2.right * beamLength + Vector2.up;
            float lAngle = Vector2.Angle(aimTarget - (Vector2)transform.position, Vector2.right);
            laserBeamRenderer.gameObject.transform.localEulerAngles = Vector3.forward * ((aimTarget.y > transform.position.y) ? lAngle : -lAngle);

        }
    }

    public void AddHealth(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Damage(int amount)
    {
        throw new System.NotImplementedException();
    }
}
