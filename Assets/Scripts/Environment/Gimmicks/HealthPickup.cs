using UnityEngine;
using static Constants;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int HealthRestore = 30;

    private void Update()
    {
        transform.eulerAngles += new Vector3(0f, 180f * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent.CompareTag(PlayerTag))
        {
            var target = collision.transform.parent.GetComponentInChildren<IHealable>();
            if (target == null) return;
            if (target.Heal(HealthRestore))
            {
                Destroy(gameObject);
            }
        }
    }
}