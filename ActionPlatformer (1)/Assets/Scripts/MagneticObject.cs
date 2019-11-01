using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObject : MonoBehaviour
{
    public bool inPlayerRange;

    [SerializeField] private Rigidbody2D objectRB;
    public Rigidbody2D RB
    {
        get { return objectRB; }
    }

    public delegate void OnRelease();
    public event OnRelease onRelease;

    [SerializeField] private Collider2D clickBuffer;

    public bool beingPulled, beingPushed, isAttachedPlayer;

    private float isPulledMagnitude, pushedMagnitude;

    private GameObject target;

    public enum MagneticWeight
    {
        light,
        heavy,
        stop
    }

    public MagneticWeight weight;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void attachToPlayer(PlayerBehaviour aPlayer)
    {
        SetClickBuffer(false);
        isAttachedPlayer = true;
        releaseExternalForce();
        objectRB.velocity = Vector2.zero;
        objectRB.isKinematic = true;
        transform.parent.position = aPlayer.CatchPos.position;
        transform.parent.SetParent(aPlayer.transform);
    }

    public void releaseFromPlayer()
    {
        if (this == null) return;
        //onRelease();
        if(objectRB != null) objectRB.isKinematic = false;
        transform.parent.SetParent(null);
        SetClickBuffer(true);
        isAttachedPlayer = false;
    }

    public void SetClickBuffer(bool aState)
    {
        clickBuffer.enabled = aState;
    }

    public void pullThis(GameObject aTarget, float aPullMagnitude)
    {
        target = aTarget;
        isPulledMagnitude = aPullMagnitude;
        beingPulled = true;
    }
    public void pushThis(GameObject aTarget, float aPushMagnitude)
    {
        target = aTarget;
        pushedMagnitude = aPushMagnitude;
        beingPushed = true;
    }
    public void shootTowards(Vector2 pos, float aMagnitude)
    {
        Vector2 dir = (pos - (Vector2)transform.position).normalized;
        RB.velocity = dir * aMagnitude;
    }
    public void releaseExternalForce()
    {
        target = null;
        isPulledMagnitude = 0f;
        pushedMagnitude = 0f;
        beingPulled = false;
        beingPushed = false;
    }

    // Update is called once per frame
    void Update()
    {

        if(beingPulled == true)
        {
            Vector2 dir = (target.transform.position - this.transform.position).normalized;
            objectRB.velocity = dir * isPulledMagnitude;
        } else if (beingPushed == true)
        {
            Vector2 dir = (target.transform.position - this.transform.position).normalized;
            objectRB.velocity = -dir * pushedMagnitude;
        }

        if (transform.parent.GetComponent<SpriteRenderer>())
        {
            if (inPlayerRange == true)
            {
                //transform.parent.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                //transform.parent.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        MagnetControl playerRange = collision.GetComponent<MagnetControl>();
        if (playerRange != null)
        {
            inPlayerRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        MagnetControl playerRange = collision.GetComponent<MagnetControl>();
        if (playerRange != null)
        {
            inPlayerRange = false;
        }
    }
}
