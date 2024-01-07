
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class ChairNetworking : UdonSharpBehaviour
{
    //networking
    [UdonSynced, FieldChangeCallback(nameof(PowerTransferPos))]
    private Vector3 _powerTransferPos;

    public Vector3 PowerTransferPos
    {
        set
        {
            _powerTransferPos = value;
            towerManager.PTT(value);
        }
        get => _powerTransferPos;
    }

    [UdonSynced, FieldChangeCallback(nameof(GunTowerPos))]
    private Vector3 _gunTowerPos;

    public Vector3 GunTowerPos
    {
        set
        {
            _gunTowerPos = value;
            towerManager.SGT(value);
        }
        get => _gunTowerPos;
    }

    [UdonSynced, FieldChangeCallback(nameof(MissileTowerPos))]
    private Vector3 _missileTowerPos;

    public Vector3 MissileTowerPos
    {
        set
        {
            _missileTowerPos = value;
            towerManager.SMT(value);
        }
        get => _missileTowerPos;
    }

    [UdonSynced, FieldChangeCallback(nameof(RailgunTowerPos))]
    private Vector3 _railgunTowerPos;

    public Vector3 RailgunTowerPos
    {
        set
        {
            _railgunTowerPos = value;
            towerManager.SRT(value);
        }
        get => _railgunTowerPos;
    }

    [UdonSynced, FieldChangeCallback(nameof(HandgunPos))]
    private Vector3 _handgunPos;

    public Vector3 HandgunPos
    {
        set
        {
            _handgunPos = value;
            towerManager.HGT(value);
        }
        get => _handgunPos;
    }
    //

    [SerializeField]
    TowerManager towerManager;

    //-
    public void TrySpawnPowerTransferTower(Vector3 position)
    {
        PowerTransferPos = position;
        RequestSerialization();
    }

    //-
    public void TrySpawnGunTower(Vector3 position)
    {
        GunTowerPos = position;
        RequestSerialization();
    }

    //-
    public void TrySpawnMissileTower(Vector3 position)
    {
        MissileTowerPos = position;
        RequestSerialization();
    }

    //-
    public void TrySpawnRailgunTower(Vector3 position)
    {
        RailgunTowerPos = position;
        RequestSerialization();
    }

    //-
    public void TrySpawnhandgunTower(Vector3 position)
    {
        HandgunPos = position;
        RequestSerialization();
    }

    
}
