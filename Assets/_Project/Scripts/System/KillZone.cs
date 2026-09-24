using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerDied();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Ally ally = col.GetComponentInParent<Ally>();
            Met_Enemy patrol = col.GetComponentInParent<Met_Enemy>();
            if (ally != null) ally.life = 0f;
            else if (patrol != null) patrol.life = 0f;
            else Destroy(col.gameObject);
        }
    }
}
