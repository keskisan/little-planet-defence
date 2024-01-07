
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Asteroid : UdonSharpBehaviour
{
    //networking
    [UdonSynced]
    public float impactSize;

    [UdonSynced, FieldChangeCallback(nameof(PositionToRemove))]
    private Vector3 _positionToRemove;
    public Vector3 PositionToRemove
    {
        set
        {
            _positionToRemove = value;
            planet.SetVoxelValue(_positionToRemove, Mathf.Sqrt(impactSize) * impactScale);
        }
        get => _positionToRemove;
    }

    [UdonSynced, FieldChangeCallback(nameof(CurrentPosition))]
    private Vector3 _currentPosition;
    public Vector3 CurrentPosition
    {
        set
        {
            _currentPosition = value;
            transform.position = _currentPosition;
        }
        get => _currentPosition;
    }

    [UdonSynced, FieldChangeCallback(nameof(StartVelocity))]
    public Vector3 _startVelocity;
    public Vector3 StartVelocity
    {
        set
        {
            _startVelocity = value;
            velocity = _startVelocity;
        }
        get => _startVelocity;
    }
    //

    int bulletLayer = 25;
    float impactScale = 0.3f;

    [SerializeField]
    TowerManager towerManager;

    [SerializeField]
    VoxelPlanet planet;

    [SerializeField]
    ExplosionManager explosionManager;

    [SerializeField]
    MenuScript menuScript;

    public Rigidbody rb;

    [SerializeField]
    MeshRenderer meshRenderer;

    [SerializeField]
    SphereCollider sphereCollider;

    VRCPlayerApi localPlayer;

    private Vector3 velocity;
    
    private Vector3 downVector;

    TrailRenderer trail;

    [SerializeField]
    float damageToCashRatio = 0.1f;

    private void Start()
    {
        localPlayer = Networking.LocalPlayer;
        trail = GetComponent<TrailRenderer>();
        MakeNotActive();
    }

    public void SetStartVelocity(Vector3 startVelocity)
    {
        StartVelocity = startVelocity;
    }

    public void SetAngularVelocity(Vector3 angularVelocity)
    {
        rb.angularVelocity = angularVelocity;
    }

    public void SetImpactSize(float value)
    {
        impactSize = value;
        transform.localScale = new Vector3(impactSize * 0.01f, impactSize * 0.01f, impactSize * 0.01f);
        trail.startWidth = impactSize * 0.03f;
    }

    private void FixedUpdate()
    {
        CalculateNewAxis();

        velocity -= velocity * planet.drag * Time.fixedDeltaTime;
        velocity += downVector * planet.fallSpeed;
        rb.velocity = velocity;
        
    }

    private void CalculateNewAxis()
    {
        downVector = (transform.position - planet.gravityPosition).normalized * -1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CurrentPosition = transform.position;
        RequestSerialization();

        if (collision.gameObject.layer == bulletLayer) //hit by bullet
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                if (bullet.localDamage) //handgun does local damage for everyone
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "BDD");
                    TakeDamage(towerManager.damageBullet, 0.05f);
                }
                else //only effects
                {
                    TakeDamage(towerManager.damageBullet, 0.05f);
                }
            }
            
        }

        if (!Networking.IsOwner(localPlayer, gameObject)) return; //master does collision use callback to modify geometry all
        
        if (collision.transform == planet.transform) //colides with planet
        {
            PositionToRemove = collision.GetContact(0).point;
            explosionManager.AddExplosion(PositionToRemove, impactSize * 0.01f);
            MakeNotActive();
            RequestSerialization();
            towerManager.whenGroundDissapears();
        }
    }

    public void BDD() //for local handgun bullets
    {
        TakeDamage(towerManager.damageBullet, 0.05f);
    }

    public void TakeDamage(float damage, float explosionSize) //Master calculates damage, local gets effects
    {
        if (impactSize > 1)
        {
            transform.localScale = new Vector3(impactSize * 0.01f, impactSize * 0.01f, impactSize * 0.01f);
            trail.startWidth = impactSize * 0.01f;
        }
        explosionManager.AddExplosion(transform.position, explosionSize);

        if (!Networking.IsOwner(localPlayer, gameObject)) return; //master does collision use callback to modify geometry all
        if (damage < impactSize)
        {
            menuScript.AddMoney((int)(damage * damageToCashRatio));
        }
        else
        {
            menuScript.AddMoney((int)(impactSize * damageToCashRatio));
            menuScript.DestroyedAnAsteroid();
        }
        impactSize -= damage;

        
        if (impactSize <= 1) //crater size less than 1 become larger again. sqrt function
        {
            MakeNotActive();
        }
    }

    public void MAA()
    {
        rb.isKinematic = false;
        meshRenderer.enabled = true;
        sphereCollider.enabled = true;
        trail.enabled = trail;
        SetAngularVelocity(Random.onUnitSphere);
    }

    public void MakeActive()
    {
        RequestSerialization();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "MAA");
    }


    public void MNA()
    {
        transform.localPosition = Vector3.zero;
        rb.isKinematic = true;
        meshRenderer.enabled = false;
        sphereCollider.enabled = false;
        trail.enabled = false;
    }

    public void MakeNotActive()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "MNA");
    }


    public bool GetActive()
    {
        return meshRenderer.enabled;
    }

    public void MAT()
    {
        for (int i = 0; i < towerManager.towersGuns.Length; i++)
        {
            TowerGun towerGun = towerManager.towersGuns[i].GetComponent<TowerGun>();
            if (towerGun != null)
            {
                towerGun.currentAsteroid = this;
            }
        }
    }

    public void MakeTarget()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "MAT");
    }
}
