
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class RadarGun : UdonSharpBehaviour
{
    [SerializeField]
    TowerManager towerManager;

    VRCPlayerApi localPlayer;
    Rigidbody rb;
    VRCPickup pickup;

    RaycastHit hit;

    [SerializeField]
    LayerMask layerAsteroids;

    [SerializeField]
    GameObject laser, target;

    [SerializeField]
    float visibleTime;

    [SerializeField]
    Text distanceText;

    Asteroid targetAsteroid;

    void Start()
    {
        pickup = GetComponent<VRCPickup>();
        localPlayer = Networking.LocalPlayer;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (visibleTime > 0)
        {
            if (targetAsteroid != null)
            {
                target.transform.position = targetAsteroid.transform.position;
            }  
            visibleTime -= Time.deltaTime;
        }
        else
        {
            target.SetActive(false);
            laser.SetActive(false);
        }
    }

    public void SpawnRadar()
    {
        if (!pickup.IsHeld)
        {
            Networking.SetOwner(localPlayer, gameObject);
            transform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }    
    }

    


    //cals function on asteroid that call network function to make asteroid target
    public override void OnPickupUseDown()
    {
        visibleTime = 0.5f;
        laser.SetActive(true);
        if (Physics.SphereCast(transform.position, 15f, transform.forward, out hit, 1000f, layerAsteroids))
        {
            distanceText.text = hit.distance.ToString();
            if (hit.distance < 400f)
            targetAsteroid = hit.transform.gameObject.GetComponent<Asteroid>();
            if (targetAsteroid != null)
            {
                target.transform.position = targetAsteroid.transform.position;
                target.transform.localScale = Vector3.one * Vector3.Distance(transform.position, hit.point) * 5f;
                target.SetActive(true);

                targetAsteroid.MakeTarget();
            }
        }
    }
}
