using UnityEngine;
using System.Collections;

/// <summary>
/// Enforces play area rules. If a player leaves the play area, they die. This mitigates the
/// damage done by normally game-breaking collision bugs that can put the player out of the
/// play area with no way back in.
/// </summary>
public class PlayAreaEnforcer : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        //If it's a player that left our bounds, kill them so that they may return to
        //us and continue the fight. FIGHT FOR THE GODS, PUNY CART-PEOPLE!
        //MUAHHAHAHA FUCK YOU AAAAAALLLLLLLLLLLLL!
        if(other.GetComponentInParent<Player>() != null)
        {
            other.GetComponentInParent<Player>().Die();
        }
    }
}
