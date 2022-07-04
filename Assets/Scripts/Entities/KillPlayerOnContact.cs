using UnityEngine;

namespace Entities
{
    public class KillPlayerOnContact : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                //other.gameObject.SetActive(false);
                Destroy(other.gameObject);
            }
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                //other.gameObject.SetActive(false);
                Destroy(other.gameObject);
            }
        }
    }
}
