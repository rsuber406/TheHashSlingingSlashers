using UnityEngine;

public class MeleeDamage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] int meleeDamage;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        IDamage dmg = other.GetComponent<IDamage>();
        if(dmg != null)
        {
            dmg.TakeDamage(meleeDamage);
        }
    }
}
