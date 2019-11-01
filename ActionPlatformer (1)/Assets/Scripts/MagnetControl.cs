using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetControl : MonoBehaviour
{
    [SerializeField] MagneticObject target;
    [Header("Layers")]
    public LayerMask magnetLayers;

    public bool pull, isObjectAttached;
    private MagneticObject attachedObject;
    private Collider2D objectOnPlayer;

    [SerializeField] private SpriteRenderer magBeamRenderer;

    private Vector2 hitPoint;

    [SerializeField] private float maxTimeToPull, elapsedPullTime, distToTarget, pullStartDist;

    [SerializeField] private LayerMask catchMask;
    [SerializeField] private Vector2 catchBox;

    [SerializeField] private GameObject pushEffect, pullEffect;

    [SerializeField] private Animator magnetAnim;

    [SerializeField] private float magBeamHeight;

    [SerializeField] private GameObject missEffect;

    [SerializeField] private PlayerBehaviour player;
    // Start is called before the first frame update
    void Start()
    {
        magBeamRenderer.size = Vector2.up * magBeamHeight;
        pull = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(this.transform.position, catchBox);
        Gizmos.DrawRay(transform.position, hitPoint - (Vector2)transform.position);
    }

    private void releaseAttachedObject()
    {
        attachedObject.releaseFromPlayer();
        attachedObject = null;
        isObjectAttached = false;
        player.SetHolding(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.CurrentGameState == GameManager.GameState.paused) return;

        if(isObjectAttached && attachedObject == null)
        {
            isObjectAttached = false;
            player.SetHolding(false);
            player.SetAimNothing();
        }

        if(isObjectAttached && attachedObject != null)
        {
            if (!attachedObject.isAttachedPlayer)
            {
                releaseAttachedObject();
                player.SetAimNothing();
            }
            else
            {
                attachedObject.transform.parent.position = player.CatchPos.position;
                player.SetHolding(true);
                player.SetAimForward(true);
            }
        }

        SurfaceStick();

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            FindObjectOfType<AudioHandler>().Play("Magnet Switch");
            pull = !pull;
            magnetAnim.SetBool("isPushing", !pull);
            if (!pull)
            {
                magBeamRenderer.size = Vector2.zero;
            }
            //Switch push/pull state
        }

        if(Input.GetMouseButtonDown(0) && isObjectAttached)
        {
            if(!pull)
            {
                MagneticObject temp = attachedObject;
                releaseAttachedObject();
                Vector2 lMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                temp.shootTowards(lMousePos, player.PushMagnitude * 1.5f);
                player.SetPush(true);
                CameraControl.instance.Shake(0.08f, 5, 8f);
                FindObjectOfType<AudioHandler>().Play("Magnet Push");
                CreatePushEffect();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            player.pullStartBonus = true;
            HandleSetTarget();
        }

        if(Input.GetMouseButton(0))
        {
            if(target!=null)
            {
                HandleAttratctionBeam();

                if (target == null)
                {
                    magBeamRenderer.size = Vector2.zero;
                    elapsedPullTime = 0f;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (target != null)
            {
                switch (target.weight)
                {
                    case MagneticObject.MagneticWeight.heavy:
                        player.realseExternalForce();
                        break;
                    case MagneticObject.MagneticWeight.light:
                        target.releaseExternalForce();
                        player.stopChannel();
                        break;
                    case MagneticObject.MagneticWeight.stop:
                        target.releaseExternalForce();
                        player.stopChannel();
                        break;
                }
                target = null;

            }
            if(!attachedObject)player.SetAimNothing();
            
            magBeamRenderer.size = Vector2.up * magBeamHeight;
            elapsedPullTime = 0f;
            distToTarget = 0f;
        }
    }

    private void HandleAttratctionBeam()
    {
        RaycastHit2D lHitCheck = Physics2D.Raycast(transform.position, hitPoint - (Vector2)transform.position, 100f, magnetLayers);
        if (lHitCheck)
        {
            ObjectStick();

            MagneticObject lBeamIntterupt = lHitCheck.collider.GetComponent<MagneticObject>();
            if (lBeamIntterupt == null)
            {
                resetTarget();
                return;
            }

            if (lBeamIntterupt != target)
            {
                resetTarget();
                //HandleSetTarget(false, lHitCheck);
                return;
            }

            hitPoint = lHitCheck.point;
            distToTarget = Vector2.Distance(transform.position, hitPoint);
            if (target.weight == MagneticObject.MagneticWeight.light && pull)
            {
                distToTarget = Vector2.Distance(transform.position, target.transform.position);
            }

            float lAngle = ((hitPoint.y > transform.position.y) ? 1 : -1) * Vector2.Angle(hitPoint - (Vector2)transform.position, Vector2.right);
            if (lAngle < 45f && lAngle > -45f)
            {
                player.SetAimForward(true);
            }
            else if (lAngle > 45f)
            {
                player.SetAimAbove(true);
            }
            else if (lAngle < -45f)
            {
                player.SetAimBelow(true);
            }

            if (pull)
            {
                magBeamRenderer.size = Vector2.right * distToTarget * Mathf.Clamp01(elapsedPullTime / (maxTimeToPull * Mathf.Clamp01(pullStartDist / 7.5f))) + Vector2.up * magBeamHeight;

                magBeamRenderer.gameObject.transform.localEulerAngles = Vector3.forward * (lAngle);

            }
            if (elapsedPullTime < maxTimeToPull * Mathf.Clamp01(pullStartDist / 7.5f))
            {

                elapsedPullTime += Time.deltaTime;
            }
            else
            {
                if (pull)
                {
                    switch (target.weight)
                    {
                        case MagneticObject.MagneticWeight.heavy:
                            if (target.gameObject != player.surfaceStuckTo)
                            {
                                player.pullPlayerTowards(hitPoint);
                            }
                            break;
                        case MagneticObject.MagneticWeight.light:
                            if (!isObjectAttached)
                            {
                                target.pullThis(this.gameObject, player.PullMagnitude);
                                player.channelPull();
                            }
                            break;
                    }
                }
            }

            if (target.inPlayerRange == false)
            {
                resetTarget();
            }
        }
    }

    private void resetTarget()
    {
        switch (target.weight)
        {
            case MagneticObject.MagneticWeight.heavy:
                player.realseExternalForce();
                break;
            case MagneticObject.MagneticWeight.light:
                target.releaseExternalForce();
                player.stopChannel();
                break;
            case MagneticObject.MagneticWeight.stop:
                target.releaseExternalForce();
                player.stopChannel();
                break;
        }
        target = null;
    }

    private void HandleSetTarget(bool useMouseCast = true, RaycastHit2D aHit = new RaycastHit2D())
    {
        RaycastHit2D hit = (useMouseCast) ? Physics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position , 100f, magnetLayers) : aHit;
        if (hit)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                FindObjectOfType<AudioHandler>().Play("Miss Sound");
                CreateMissEffect();
                return;
            }
            target = hit.collider.GetComponent<MagneticObject>();

            if (target == null)
            {
                FindObjectOfType<AudioHandler>().Play("Miss Sound");
                CreateMissEffect();
                return;
            }

            if (target.inPlayerRange == false)
                return;

            hitPoint = hit.point;

            switch (target.weight)
            {
                case MagneticObject.MagneticWeight.heavy:
                    if (pull)
                    {
                        elapsedPullTime = 0f;
                        CreatePullEffect();
                        CameraControl.instance.Shake(0.08f, 5, 8f);
                        player.PullStartBonus(hit.point);
                    }
                    else
                    {
                        player.pushPlayerFrom(hit.point);
                        CreatePushEffect();
                        FindObjectOfType<AudioHandler>().Play("Magnet Push");
                        CameraControl.instance.Shake(0.08f, 5, 8f);
                        player.SetPush(true);
                        //player.realseExternalForce();
                    }
                    distToTarget = Vector2.Distance(transform.position, hitPoint);
                    pullStartDist = distToTarget;
                    break;
                case MagneticObject.MagneticWeight.light:
                    if (!isObjectAttached)
                    {
                        if (pull)
                        {
                            elapsedPullTime = 0f;
                            CreatePullEffect();
                            CameraControl.instance.Shake(0.08f, 5, 8f);
                        }
                        else
                        {
                            if (isObjectAttached) { target = null; break; }
                            target.shootTowards(hit.point, -player.PushMagnitude);
                            CreatePushEffect();
                            FindObjectOfType<AudioHandler>().Play("Magnet Push");
                            CameraControl.instance.Shake(0.08f, 5, 8f);
                            player.SetPush(true);
                        }
                        distToTarget = Vector2.Distance(transform.position, hitPoint);
                        pullStartDist = distToTarget;
                    }
                    break;
                case MagneticObject.MagneticWeight.stop:
                    if (!pull)
                    {
                        target.transform.parent.GetComponent<ShotgunProjectile>().Stop();
                        CreatePushEffect();
                        FindObjectOfType<AudioHandler>().Play("Magnet Push");
                        CameraControl.instance.Shake(0.08f, 5, 8f);
                        player.SetPush(true);
                    }
                    break;
            }

        }
    }

    private void CreatePushEffect()
    {
        Vector2 lMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lPointDir = lMousePos - (Vector2)transform.position;
        float lAngle = ((lMousePos.y > transform.position.y) ? 1 : -1) * Vector2.Angle(lMousePos - (Vector2)transform.position, Vector2.right);
        Instantiate(pushEffect, (Vector2)transform.position + (player.CatchPos.position - transform.position).magnitude * lPointDir.normalized, Quaternion.Euler(0f, 0f, lAngle + 90f));
    }

    private void CreatePullEffect()
    {
        Vector2 lMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lPointDir = lMousePos - (Vector2)transform.position;
        float lAngle = ((lMousePos.y > transform.position.y) ? 1 : -1) * Vector2.Angle(lMousePos - (Vector2)transform.position, Vector2.right);
        Instantiate(pullEffect, (Vector2)transform.position + (player.CatchPos.position - transform.position).magnitude * lPointDir.normalized, Quaternion.Euler(0f, 0f, lAngle + 90f));
    }

    private void CreateMissEffect()
    {
        Vector2 lMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lPointDir = lMousePos - (Vector2)transform.position;
        float lAngle = ((lMousePos.y > transform.position.y) ? 1 : -1) * Vector2.Angle(lMousePos - (Vector2)transform.position, Vector2.right);
        Instantiate(missEffect, (Vector2)transform.position + (player.CatchPos.position - transform.position).magnitude * lPointDir.normalized, Quaternion.Euler(0f, 0f, lAngle + 90f));
    }

    private void SurfaceStick()
    {
        MagneticObject magObj = null;
        ContactFilter2D lFilter = new ContactFilter2D();
        lFilter.layerMask = 1 << LayerMask.NameToLayer("Ground");
        lFilter.useTriggers = true;
        lFilter.useOutsideDepth = true;
        Collider2D[] contactResults = new Collider2D[10];
        int lNumCol = Physics2D.OverlapBox((Vector2)this.transform.position, catchBox, 0f, lFilter, contactResults);
        if (lNumCol == 0) return;

        foreach (Collider2D coll in contactResults)
        {
            if (coll == null) continue;
            MagneticObject lmagObj = coll.GetComponent<MagneticObject>();
            if (lmagObj == null) continue;
            if (lmagObj.weight == MagneticObject.MagneticWeight.heavy)
            {
                magObj = lmagObj;
                break;
            }
        }

        if (magObj == null || (!player.onCeiling && !player.onGround && !player.onWall))
        {
            player.stuckToSurface = false;
            player.surfaceStuckTo = null;
            return;
        }

        if (magObj.weight == MagneticObject.MagneticWeight.heavy)
        {
            if(!player.stuckToSurface)
            {
                player.stuckToSurface = true;
                player.surfaceStuckTo = magObj.gameObject;
                player.realseExternalForce();
                player.SetFalling(false);
            }
        }
    }

    private void ObjectStick()
    {
        if (!isObjectAttached && pull && attachedObject == null)
        {
            ContactFilter2D lFilter = new ContactFilter2D();
            lFilter.layerMask = catchMask;
            lFilter.useTriggers = true;
            lFilter.useOutsideDepth = true;
            Collider2D[] contactResults = new Collider2D[10];
            int lNumCol = Physics2D.OverlapBox((Vector2)this.transform.position, catchBox, 0f, lFilter, contactResults);
            if (lNumCol == 0) return;
            foreach (Collider2D coll in contactResults)
            {
                if (coll == null) continue;
                MagneticObject lmagObj = coll.GetComponent<MagneticObject>();
                if (lmagObj == null) continue;
                if (lmagObj.weight == MagneticObject.MagneticWeight.light && lmagObj == target)
                {
                    attachedObject = lmagObj;
                    isObjectAttached = true;
                    lmagObj.attachToPlayer(player);
                    break;
                }
            }
        }
    }

}
