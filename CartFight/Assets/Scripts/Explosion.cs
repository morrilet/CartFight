using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : PausableObject
{
    private Collider2D coll;

    //How long the collider is able to do damage for. This is here 
    //because the smoke may stick around after being able to do damage.
    private const float DAMAGE_DURATION = 0.35f;
    private const float FORCE = 15f;

    public Player triggerer;

    private float timer = 0.0f;

    private void Update()
    {
        if(!IsPaused && timer <= DAMAGE_DURATION)
        {
            timer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 centerToOther = other.transform.position - transform.position;
        centerToOther.Normalize();

        if(timer < DAMAGE_DURATION) //If we can still do damage...
        {
            if(other.tag.Equals("Driver"))
            {
                //if (coll.IsTouching(other.transform.parent.GetComponent<Player>().CartObj.GetComponent<Collider2D>()))
                {
                    //other.transform.parent.GetComponent<Player>()
                    //    .CartObj.GetComponent<Rigidbody2D>().AddForce(centerToOther * FORCE, ForceMode2D.Impulse);
                }
                other.transform.parent.GetComponent<Player>().Die(triggerer);
            }
            else if (other.tag.Equals("Cart"))
            {
                if (other.transform.parent == null)
                {
                    other.GetComponent<Rigidbody2D>().AddForce(centerToOther * FORCE, ForceMode2D.Impulse);

                    Item[] cartItems = other.gameObject.GetComponentsInChildren<Item>();
                    for (int i = 0; i < cartItems.Length; ++i)
                    {
                        cartItems[i].GetRemovedFromCart();
                    }
                }
            }
            else if (other.tag.Equals("Item"))
            {
                if(other.GetComponent<Item>().itemType == Item.ItemType.Bomb)
                {
                    other.GetComponent<Bomb>().Trigger(this.triggerer);
                }

                if(other.GetComponent<Item>().isPickedUp())
                {
                    other.GetComponent<Item>().GetDropped();
                }
                other.GetComponent<Rigidbody2D>().AddForce(centerToOther * FORCE, ForceMode2D.Impulse);
            }
        }
    }
}
