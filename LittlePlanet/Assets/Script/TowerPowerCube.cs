
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerPowerCube : Tower
{
    public override void OverwritableStart()
    {
        towerEnabled = true;
        powerAmount = int.MaxValue;
        Health = float.MaxValue;
    }

    public override void KillGun()
    {
        
    }
}
