
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class SpannerSell : UdonSharpBehaviour
{
    Rigidbody rb;

    VRCPlayerApi localPlayer;

    [SerializeField]
    TowerManager towerManager;

    [SerializeField]
    MenuScript menuScript;

    [SerializeField]
    float rayLength = 3f;

    [SerializeField]
    LayerMask towerLayer;

    [SerializeField]
    Text sellPriceText;

    VRCPickup pickup;

    private Tower tower;

    private int price = 0;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        rb = GetComponent<Rigidbody>();
        pickup = GetComponent<VRCPickup>();
    }

    private void Update()
    {
        price = 0;

        if (pickup.IsHeld)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayLength, towerLayer))
            {
                tower = hit.collider.GetComponent<Tower>();

                if (tower != null)
                {
                    price = tower.SellPrice();
                    sellPriceText.text = price == 0 ? "NA" : price.ToString(); 
                }
                else
                {
                    sellPriceText.text = "NA";
                }
            }
            else
            {
                sellPriceText.text = "NA";
            }
        }

    }

    public void USE()
    {
        if (Networking.IsMaster)
        {
            if (tower != null)
            {
                if (price != 0)
                {
                    menuScript.money += price;
                    tower.KillGun(); 
                }
            }
        }
    }

    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "USE");
    }
    //--
    public void SpawnSpanner()
    {
        if (!pickup.IsHeld)
        {
            Networking.SetOwner(localPlayer, gameObject);
            transform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
