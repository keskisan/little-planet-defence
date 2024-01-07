
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ExplosionManager : UdonSharpBehaviour
{
    [SerializeField]
    ParticleSystem[] explosions;
    int counter;
    public void AddExplosion(Vector3 pos, float duration)
    {
        explosions[counter].transform.position = pos;
        var main = explosions[counter].main;
        main.duration = duration;
        explosions[counter].Play();
        counter++;
        if (counter >= explosions.Length) counter = 0;
    }
}
