using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public BoxCollider2D BodyCollider = null;
    public BoxCollider2D TipCollider = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!TipCollider.enabled && collision.rigidbody.name == "Ground")
        {
            Destroy(gameObject, 0.5f);
        }
    }
}
