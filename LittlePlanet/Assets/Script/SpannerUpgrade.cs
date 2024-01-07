
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class SpannerUpgrade : UdonSharpBehaviour
{
    Rigidbody rb;

    VRCPlayerApi localPlayer;

    [SerializeField]
    MenuScript menuScript;

    [SerializeField]
    float rayLength = 3f;

    [SerializeField]
    LayerMask towerLayer;

    [SerializeField]
    Text costText;

    [SerializeField]
    Text descriptionText;

    VRCPickup pickup;

    private Tower tower;

    private int cost;

    float canUseTimer = 0;
    float useTime = 0.5f;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        rb = GetComponent<Rigidbody>();
        pickup = GetComponent<VRCPickup>();
    }

    private void Update()
    {
        canUseTimer += Time.deltaTime;


        cost = 0;
        if (pickup.IsHeld)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayLength, towerLayer))
            {
                tower = hit.collider.GetComponent<Tower>();

                if (tower != null)
                {
                    if (tower.Health < tower.healthMax) //fix tower first
                    {
                        descriptionText.text = "Fix: ";
                        float healthCost = tower.healthMax - tower.Health;
                        costText.text = healthCost.ToString();
                        if (healthCost <= menuScript.money)
                        {
                            costText.color = Color.cyan;
                        }
                        else
                        {
                            costText.color = Color.red;
                        }
                    }
                    else //upgrade second
                    {
                        descriptionText.text = "Upgrade: ";
                        cost = tower.UpgradeCost();
                        costText.text = cost == 0 ? "NA" : cost.ToString();
                        if (cost <= menuScript.money)
                        {
                            costText.color = Color.cyan;
                        }
                        else
                        {
                            costText.color = Color.red;
                        }
                    }

                        
                }
                else
                {
                    costText.text = "NA";
                }
            }
            else
            {
                costText.text = "NA";
            }
        }
       
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

    //--
    public void USE()
    {
        if (Networking.IsMaster)
        {
            if (canUseTimer > useTime)
            {
                canUseTimer = 0f;
                if (tower != null)
                {
                    if (tower.Health < tower.healthMax) //fix tower first
                    {
                        float healthCost = tower.healthMax - tower.Health;
                        if (healthCost <= menuScript.money)
                        {
                            menuScript.AddMoney(-(int)healthCost);
                            tower.Health = tower.healthMax;
                        }
                        else
                        {
                            tower.Health += menuScript.money;
                            menuScript.AddMoney(-menuScript.money);
                        }

                    }
                    else if (tower.UpgradeLevel <= 2) //upgrade tower second
                    {
                        if (cost != 0)
                        {
                            if (cost <= menuScript.money)
                            {
                                menuScript.AddMoney(-cost);
                                tower.UpgradeGun(tower.UpgradeLevel + 1);
                            }
                        }
                    }
                }
            }   
        }
    }

    //--

    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "USE");
    }
}
