
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Bullet : Ammo
{
    public bool localDamage;

    public override void FireAmmo(float startVelocity, float distance, bool localDamage = false)
    {
        rb.velocity = transform.forward * startVelocity;
        this.localDamage = localDamage;
    }
}
