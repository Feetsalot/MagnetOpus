using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemeyRange : MonoBehaviour
{
    [SerializeField] private CircleCollider2D rangeCollider;
    private bool playerInRange;
    public bool PlayerInRange
    {
        get { return playerInRange; }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, this.transform.position - collision.transform.position, rangeCollider.radius + 0.5f, 1 << LayerMask.NameToLayer("Player"));
            if(hit)
            {
                playerInRange = true;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInRange = false;
        }
    }
}
