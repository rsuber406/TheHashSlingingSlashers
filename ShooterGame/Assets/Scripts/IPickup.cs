using UnityEngine;

public interface IPickup 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void GrabGun(FirearmScriptable gun, Transform shootPos);
}
