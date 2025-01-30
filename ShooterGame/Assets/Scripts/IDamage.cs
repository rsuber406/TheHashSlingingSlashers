using UnityEngine;

public interface IDamage
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void TakeDamage(int amount);
    void TakeDamage(int amount, Vector3 origin);

}
