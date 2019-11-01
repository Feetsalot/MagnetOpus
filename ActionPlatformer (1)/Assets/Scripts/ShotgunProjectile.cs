using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunProjectile : MonoBehaviour
{
    private Vector2 bulletDir;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private LayerMask bulletInterupts;
    [SerializeField] private Collider2D bulletCollider;
    private GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        //bulletDir = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCourse(Vector2 aDir)
    {
        bulletDir = aDir;
        rb.velocity = bulletDir * bulletSpeed;
    }

    public void SetParent(GameObject aParent)
    {
        parent = aParent;
    }

    public void Stop()
    {
        rb.velocity = Vector2.zero;
        bulletCollider.enabled = false;
        anim.SetTrigger("Hit");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
    if (bulletInterupts == (bulletInterupts | (1 << collision.gameObject.layer)))
        {
            if (collision.gameObject == parent) return;
            bulletCollider.enabled = false;
            Debug.Log(collision.gameObject);
            PlayerBehaviour lPlayer = collision.GetComponent<PlayerBehaviour>();
            Damagable lDmg = collision.gameObject.GetComponent<Damagable>();
            if(lPlayer != null)
            {
                if(!lPlayer.Invulnerable)
                {
                    Vector2 lBounceDir = Vector2.zero;
                    if(rb.velocity.y < 0)
                    {
                        int lRand = Random.Range(0, 2);
                        lBounceDir = ((lRand == 0) ? Vector2.left : Vector2.right)*0.5f + Vector2.up*0.5f;
                    }
                    if(rb.velocity.y > 0)
                    {
                        int lRand = Random.Range(0, 2);
                        lBounceDir = ((lRand == 0) ? Vector2.left : Vector2.right)*0.5f;
                    }
                    if(rb.velocity.x > 0 || rb.velocity.x < 0)
                    {
                        lBounceDir = bulletDir + Vector2.up * 0.5f;
                    }
                    lPlayer.StartBounceBack(lBounceDir);
                    lPlayer.Damage(1);
                }
            }
            else if(lDmg != null)
            {
                lDmg.Damage(1);
            }
            rb.velocity = Vector2.zero;
            anim.SetTrigger("Hit");
        }
    }
}
