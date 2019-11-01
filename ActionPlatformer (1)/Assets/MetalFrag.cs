using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalFrag : MonoBehaviour
{
    [SerializeField] private MagneticObject magObj;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] LayerMask Damagables;
    [SerializeField] private GameObject hitPrefab;

    public GameObject spawnParent;

    private bool canDamage;
    // Start is called before the first frame update
    void Start()
    {
        //magObj.onRelease += 
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.velocity.magnitude > 10f && !magObj.isAttachedPlayer)
        {
            canDamage = true;
        } else
        {
            if(magObj.isAttachedPlayer || magObj.beingPushed || magObj.beingPulled)
            {
                spawnParent = PlayerBehaviour.instance.gameObject;
            }
            canDamage = false;
        }
        if (canDamage) HandleDamageCheck();
    }

    private void HandleDamageCheck()
    {
        ContactFilter2D lFilter = new ContactFilter2D();
        lFilter.useLayerMask = true;
        lFilter.useTriggers = false;
        lFilter.layerMask = Damagables;
        Collider2D[] contactResults = new Collider2D[10];
        int lNumCol = Physics2D.OverlapBox(rb.position, new Vector2(1f,0.5f), 0f, lFilter, contactResults);
        if (lNumCol > 0)
        {
            foreach (Collider2D lColl in contactResults)
            {
                if (lColl != null && lColl.gameObject != gameObject && lColl.gameObject != spawnParent)
                {
                    Damagable dmg = lColl.GetComponent<Damagable>();
                    if(dmg != null)
                    {
                        dmg.Damage(1);
                        Instantiate(hitPrefab, rb.position, Quaternion.identity);
                        GameManager.instance.RemoveMetalFrag(this);
                        Destroy(gameObject);
                        break;
                    }
                }
            }
        }
        contactResults = new Collider2D[10];
    }
}
