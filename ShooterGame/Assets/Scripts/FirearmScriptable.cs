using UnityEngine;
[CreateAssetMenu]
public class FirearmScriptable : ScriptableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public GameObject model;
    public int damage;
    public int range;
    public float fireRate;
    public int ammoCurrent, ammoMax;
    public Transform shootPos;
    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    public float shootSoundVolume;
}
