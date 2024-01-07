
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;


//made bullet transfer udonsynced //test this
//Modify handgun so its bullets calculate locally and not on master //test this


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TowerManager : UdonSharpBehaviour
{
    public MenuScript menuScript;

    public Tower[] towersGuns;

    public TowerPowerTransfer[] towersPowerTransfer;


    public int towersPowerTransferArrayLength = 20;

    public TowerPowerCube[] towersPowerSupply;

    public AsteroidGenerator asteroids;

    [SerializeField]
    ConnectLaser connectLaser;

    [SerializeField]
    float connectionDistance;
    float squaredDistance;

    VRCPlayerApi localPlayer;

    public VoxelPlanet planet;

    public float rotationsSpeed = 5f;

    public float rayLength = 0.3f;

    public LayerMask floorLayer;

    public AmmoPool bullets;
    public AmmoPool misiles;

    public float powerTransferFrequency = 0.1f; //
    public float powerTransferFrequency1 = 0.075f; //
    public float powerTransferFrequency2 = 0.05f; //

    [Header("Max power")]
    public int maxPowerAmountMissileLauncher = 6; //
    public int maxPowerAmountMissileLauncher1 = 9; //
    public int maxPowerAmountMissileLauncher2 = 12; //
    public int maxPowerAmounthandgun = 14; //
    public int maxPowerAmounthandgun1 = 18; //
    public int maxPowerAmounthandgun2 = 22; //
    public int maxPowerAmountRailgun = 20; //
    public int maxPowerAmountRailgun1 = 30; //
    public int maxPowerAmountRailgun2 = 40; //
    public int maxPowerAmountTowergun = 40; //
    public int maxPowerAmountTowergun1 = 55; //
    public int maxPowerAmountTowergun2 = 70; //

    [Header("Power use per shot")]
    public int shotCostMissileLauncher = 3; //
    public int shotCostMissileLauncher1 = 3; //
    public int shotCostMissileLauncher2 = 3; //
    public int shotCostAmounthandgun = 1; //
    public int shotCostAmounthandgun1 = 1; //
    public int shotCostAmounthandgun2 = 1; //
    public int shotCostAmountRailgun = 10; //
    public int shotCostAmountRailgun1 = 12; //
    public int shotCostAmountRailgun2 = 14; //
    public int shotCostAmountTowergun = 1; //
    public int shotCostAmountTowergun1 = 1;  //
    public int shotCostAmountTowergun2 = 1; //

    [Header("shoot damage")]
    public float damageScaleMissile = 4f;
    public float damageBullet = 5f;
    public float damageRailgun = 100f; //
    public float damageRailgun1 = 150f; //
    public float damageRailgun2 = 200f; //

    [Header("shoot distance")]
    [SerializeField] float maxShootDistanceMissileLancher = 75f;
    [SerializeField] float maxShootDistanceMissileLancher1 = 90f;
    [SerializeField] float maxShootDistanceMissileLancher2 = 110f;
    [SerializeField] float maxShootDistanceRailgun = 200f;
    [SerializeField] float maxShootDistanceRailgun1 = 200f;
    [SerializeField] float maxShootDistanceRailgun2 = 200f;
    [SerializeField] float maxShootDistanceTowergun = 110f;
    [SerializeField] float maxShootDistanceTowergun1 = 110f;
    [SerializeField] float maxShootDistanceTowergun2 = 110f;

    [HideInInspector] public float maxShootDistanceMissileLancherSqrd; //
    [HideInInspector] public float maxShootDistanceMissileLancher1Sqrd; //
    [HideInInspector] public float maxShootDistanceMissileLancher2Sqrd; //
    [HideInInspector] public float maxShootDistanceRailgunSqrd; //
    [HideInInspector] public float maxShootDistanceRailgun1Sqrd; //
    [HideInInspector] public float maxShootDistanceRailgun2Sqrd; //
    [HideInInspector] public float maxShootDistanceTowergunSqrd; //
    [HideInInspector] public float maxShootDistanceTowergun1Sqrd; //
    [HideInInspector] public float maxShootDistanceTowergun2Sqrd; //

    [Header("Shoot frequency")]
    public float SecondsToShootMissileLauncer = 3f; //
    public float SecondsToShootMissileLauncer1 = 3f; //
    public float SecondsToShootMissileLauncer2 = 3f; //
    public float SecondsToShootRailgun = 7f; //
    public float SecondsToShootRailgun1 = 7f; //
    public float SecondsToShootRailgun2 = 7f; //
    public float SecondsToShootTowergun = 0.5f; //
    public float SecondsToShootTowergun1 = 0.5f; //
    public float SecondsToShootTowergun2 = 0.5f; //
    public float SecondsToShootHandgun = 0.5f; //
    public float SecondsToShootHandgun1 = 0.4f; //
    public float SecondsToShootHandgun2 = 0.3f; //

    [Header("Shoot Velocity")]
    public float velocityMissileLauncer = 90f; //
    public float velocityMissileLauncer1 = 90f; //
    public float velocityMissileLauncer2 = 90f; //
    public float velocityRailgun = 10000f; //
    public float velocityRailgun1 = 12000f; //
    public float velocityRailgun2 = 14000f; //
    public float velocityTowergun = 200f; //
    public float velocityTowergun1 = 200f; //
    public float velocityTowergun2 = 200f; //
    public float velocityHandgun = 200f; //
    public float velocityHandgun1 = 400f; //
    public float velocityHandgun2 = 600f; //

    [Header("Tower Cost")]
    public int towerCostPowerTransfer = 50; //
    public int towerCostPowerTransfer1 = 150; //
    public int towerCostPowerTransfer2 = 400; //
    public int towerCostMissileLauncer = 100; //
    public int towerCostMissileLauncer1 = 200; //
    public int towerCostMissileLauncer2 = 400; //
    public int towerCostRailgun = 150; //
    public int towerCostRailgun1 = 300; //
    public int towerCostRailgun2 = 500; //
    public int towerCostTowergun = 100; //
    public int towerCostTowergun1 = 200; //
    public int towerCostTowergun2 = 400; //
    public int towerCostHandgun = 50; //
    public int towerCostHandgun1 = 75; //
    public int towerCostHandgun2 = 100; //

    [Header("Health")]
    public int HealthPowerTransfer = 100; //
    public int HealthPowerTransfer1 = 150; //
    public int HealthPowerTransfer2 = 200; //
    public int HealthMissileLauncer = 100; //
    public int HealthMissileLauncer1 = 150; //
    public int HealthMissileLauncer2 = 200; //
    public int HealthRailgun = 100; //
    public int HealthRailgun1 = 150; //
    public int HealthRailgun2 = 200; //
    public int HealthTowergun = 100; //
    public int HealthTowergun1 = 150; //
    public int HealthTowergun2 = 200; //
    public int HealthHandgun = 100;
    public int HealthHandgun1 = 150;
    public int HealthHandgun2 = 200;

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;

        squaredDistance = connectionDistance * connectionDistance;

        maxShootDistanceMissileLancherSqrd = maxShootDistanceMissileLancher * maxShootDistanceMissileLancher;
        maxShootDistanceMissileLancher1Sqrd = maxShootDistanceMissileLancher1 * maxShootDistanceMissileLancher1;
        maxShootDistanceMissileLancher2Sqrd = maxShootDistanceMissileLancher2 * maxShootDistanceMissileLancher2;
        maxShootDistanceRailgunSqrd = maxShootDistanceRailgun * maxShootDistanceRailgun;
        maxShootDistanceRailgun1Sqrd = maxShootDistanceRailgun1 * maxShootDistanceRailgun1;
        maxShootDistanceRailgun2Sqrd = maxShootDistanceRailgun2 * maxShootDistanceRailgun2;
        maxShootDistanceTowergunSqrd = maxShootDistanceTowergun * maxShootDistanceTowergun;
        maxShootDistanceTowergun1Sqrd = maxShootDistanceTowergun1 * maxShootDistanceTowergun1;
        maxShootDistanceTowergun2Sqrd = maxShootDistanceTowergun2 * maxShootDistanceTowergun2;

    }

    public void UpdateConnections()
    {
        //compare all power transfer towers to power transfer towers
        for (int i = 0; i < towersPowerTransfer.Length; i++)
        {
            towersPowerTransfer[i].ResetArrays();
            if (!towersPowerTransfer[i].towerEnabled) continue; //dont add not in game towers

            
            for (int n = i + 1; n < towersPowerTransfer.Length; n++)
            {
                if (!towersPowerTransfer[n].towerEnabled) continue;

                if ((towersPowerTransfer[i].transform.position - towersPowerTransfer[n].transform.position).sqrMagnitude < squaredDistance) //optimised distance test
                {
                    towersPowerTransfer[i].AddTowerPowerTower(towersPowerTransfer[n]); //two way power transfer
                }
            }
        }

        //compare all power transfer towers to gun towers
        for (int i = 0; i < towersPowerTransfer.Length; i++)
        {
            if (!towersPowerTransfer[i].towerEnabled) continue;

            for (int n = 0; n < towersGuns.Length; n++)
            {
                if (!towersGuns[n].towerEnabled) continue;

                if ((towersPowerTransfer[i].transform.position - towersGuns[n].transform.position).sqrMagnitude < squaredDistance) //optimised distance test
                {
                    towersPowerTransfer[i].AddTowerGunTower(towersGuns[n]);
                }        
            }
        }

        
        //compare all power transfer towers to power supply towers
        for (int i = 0; i < towersPowerTransfer.Length; i++)
        {
            if (!towersPowerTransfer[i].towerEnabled) continue;

            for (int n = 0; n < towersPowerSupply.Length; n++)
            {
                if (!towersPowerSupply[n].towerEnabled) continue;

                if ((towersPowerTransfer[i].transform.position - towersPowerSupply[n].transform.position).sqrMagnitude < squaredDistance) //optimised distance test
                {
                    towersPowerTransfer[i].AddTowerPowerTower(towersPowerSupply[n]);
                } 
            }
        }

        connectLaser.UpdateConnections();
    }



    //menuscript - (chairmanager, chair) chairnetworking[networked callback] - here
    //-
    public void PTT(Vector3 position)
    {
        if (menuScript.money < towerCostPowerTransfer) return; //not enougth money dont spawn
        if (Networking.IsMaster)
        {
            for (int i = 0; i < towersPowerTransfer.Length; i++)
            {
                if (!towersPowerTransfer[i].towerEnabled)
                {
                    towersPowerTransfer[i].gameObject.transform.position = position;
                    towersPowerTransfer[i].UpgradeGun(1);
                    menuScript.AddMoney(-towerCostPowerTransfer);
                    return;
                }
            }
        }
    }



    //-
    public void SGT(Vector3 position)
    {
        if (menuScript.money < towerCostTowergun) return; //not enougth money dont spawn
        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerGunTower gunTower = towersGuns[i].GetComponent<TowerGunTower>();
                if (gunTower != null)
                {
                    gunTower.gameObject.transform.position = position;
                    gunTower.UpgradeGun(1);
                    menuScript.AddMoney(-towerCostTowergun);
                    return;
                }
            }
        }
    }



    //-
    public void SMT(Vector3 position)
    {
        if (menuScript.money < towerCostMissileLauncer) return; //not enougth money dont spawn
        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerMissileLauncher missileLauncherTower = towersGuns[i].GetComponent<TowerMissileLauncher>();
                if (missileLauncherTower != null)
                {
                    missileLauncherTower.gameObject.transform.position = position;
                    missileLauncherTower.UpgradeGun(1);
                    menuScript.AddMoney(-towerCostMissileLauncer);
                    return;
                }
            }
        }
    }



    public void SRT(Vector3 position)
    {
        if (menuScript.money < towerCostRailgun) return; //not enougth money dont spawn
        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerRailGun railGunTower = towersGuns[i].GetComponent<TowerRailGun>();
                if (railGunTower != null)
                {
                    railGunTower.gameObject.transform.position = position;
                    railGunTower.UpgradeGun(1);
                    menuScript.AddMoney(-towerCostRailgun);
                    return;
                }
            }
        }
    }



    public void HGT(Vector3 position)
    {
        if (menuScript.money < towerCostHandgun) return; //not enougth money dont spawn
        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerHandGun handgun = towersGuns[i].GetComponent<TowerHandGun>();
                if (handgun != null)
                {
                    handgun.gameObject.transform.position = position;
                    handgun.UpgradeGun(1);
                    menuScript.AddMoney(-towerCostHandgun);
                    return;
                }
            }
        }
    }


    public int AmountOfUnspawnedPowerTransferTowers()
    {
        int amount = 0;
        for (int i = 0; i < towersPowerTransfer.Length; i++)
        {
            if (!towersPowerTransfer[i].towerEnabled)
            {
                amount++;
            }
        }
        return amount; 
    }

    public int AmountOfUnspawnedTowerGunTower()
    {
        int amount = 0;

        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerGunTower gunTower = towersGuns[i].GetComponent<TowerGunTower>();
                if (gunTower != null)
                {
                    amount++;
                }
            }
        }
        return amount;
    }

    public int AmountOfUnspawnedMissileLaunchers()
    {
        int amount = 0;

        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerMissileLauncher missileLauncherTower = towersGuns[i].GetComponent<TowerMissileLauncher>();
                if (missileLauncherTower != null)
                {
                    amount++;
                }
            }
        }
        return amount;
    }

    public int AmountOfUnspawnedRailGunTowers()
    {
        int amount = 0;

        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerRailGun railGunTower = towersGuns[i].GetComponent<TowerRailGun>();
                if (railGunTower != null)
                {
                    amount++;
                }
            }
        }
        return amount;
    }

    public int AmountOfUnspawnedHandgunTowers()
    {
        int amount = 0;

        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (!towersGuns[i].towerEnabled)
            {
                TowerHandGun handgun = towersGuns[i].GetComponent<TowerHandGun>();
                if (handgun != null)
                {
                    amount++;
                }
            }
        }
        return amount;
    }

    public void WGD()
    {
        for (int i = 0; i < towersGuns.Length; i++)
        {
            if (towersGuns[i].towerEnabled)
            {
                towersGuns[i].fallDownIfGroundDissapear();
            }
        }

        for (int i = 0; i < towersPowerTransfer.Length; i++)
        {
            if (towersPowerTransfer[i].towerEnabled)
            {
                towersPowerTransfer[i].fallDownIfGroundDissapear();
            }
        }

        for (int i = 0; i < towersPowerSupply.Length; i++)
        {
            if (towersPowerSupply[i].towerEnabled)
            {
                towersPowerSupply[i].fallDownIfGroundDissapear();
            }
        }
    }

    public void whenGroundDissapears()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WGD");
    }
}
