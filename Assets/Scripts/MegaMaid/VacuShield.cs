using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuShield : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null)
        {
            switch (other.tag)
            {
                case "Player":

                    // play sound?

                    break;

                case "Laser":

                    Destroy(other.gameObject);

                    break;

                default:

                    break;
            }
        }
    }
}
