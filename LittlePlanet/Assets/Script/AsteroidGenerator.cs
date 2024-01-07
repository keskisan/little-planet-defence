
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AsteroidGenerator : UdonSharpBehaviour
{
    // networked
    //[UdonSynced]
    //int asteroidsCounter = 0;
    [UdonSynced]
    int astroidsHasSpawned = 1; //avoid division by 0 //this is spawned in menu is destroyed

    //
    [SerializeField]
    MenuScript menuScript;

    public Asteroid[] asteroidPool;
    int asteroidPoolCounter = 0;

    float timer = 0f;

    [SerializeField]
    float minRandomSpawn = 50f, maxRandomSpawn = 100f;

    [SerializeField]
    float minRandomOrbit = 200f, maxRandomOrbit = 1000f;

    [SerializeField]
    float minRandomImpactSize = 10f, maxRandomImpactSize = 200f;

    [SerializeField]
    float spawnSphereRadius = 500f;

    float SpawnNextAsteroidTime;

    private void Start()
    {
        Random.InitState(78); //all games be sort same
    }

    private void Update()
    {
        if (!Networking.IsMaster) return;
        if (menuScript.gameHasStarted)
        timer += Time.deltaTime;

        if (timer > SpawnNextAsteroidTime) //Spawn asteroid//asteroids become more frequent over time
        {
            Asteroid nextAsteroid = FindNextAsteroid();

            if (nextAsteroid == null) return; //theres nothing in pool

            timer = 0f;

            float ratio = (1 / Mathf.Sqrt(Mathf.Sqrt(astroidsHasSpawned))); //this number becomes ever smaller sqrt slows decay rate

            astroidsHasSpawned++;

            SpawnNextAsteroidTime = Random.Range(minRandomSpawn * ratio, maxRandomSpawn * ratio); //asteroids become more frequent over time

            Vector3 outOfCentre = Random.onUnitSphere;
            Vector3 randomVector = Random.onUnitSphere;
            Vector3 tangentVector = Vector3.Cross(outOfCentre, randomVector);

            nextAsteroid.transform.position =outOfCentre * spawnSphereRadius; //always far

            Vector3 startVelocity = tangentVector * ratio + randomVector * (1 - ratio); //orbits start circular and becomes ever more excentric

            nextAsteroid.SetStartVelocity(startVelocity * Random.Range(minRandomOrbit * ratio, maxRandomOrbit * ratio)); //asteroids slow over time so fall down quicker

            nextAsteroid.SetImpactSize(Random.Range(minRandomImpactSize + astroidsHasSpawned, maxRandomImpactSize + astroidsHasSpawned * 10f)); //asteroids increase in size over time

            nextAsteroid.MakeActive();

            RequestSerialization();
        }

    }

    private Asteroid FindNextAsteroid()
    {
        for (int i = asteroidPoolCounter; i < asteroidPool.Length; i++)
        {
            if (!asteroidPool[i].GetActive())
            {
                asteroidPoolCounter = i;
                return asteroidPool[i];
            }
        }
        for (int i = 0; i < asteroidPoolCounter; i++)
        {
            if (!asteroidPool[i].GetActive())
            {
                asteroidPoolCounter = i;
                return asteroidPool[i];
            }
        }
        return null;
    }

    //spawn on the surface of imaginary sphere defined by startposition
    //nextAsteroid.transform.position = asteroidsStartPosition[asteroidsCounter].normalized * spawnSphereRadius;


    //nextAsteroid.SetStartVelocity(asteroidsStartVelocity[asteroidsCounter]);
    //nextAsteroid.SetAngularVelocity(asteroidsAngularVelocity[asteroidsCounter]);
    //nextAsteroid.SetImpactSize(asteroidsImpactSize[asteroidsCounter]);

    //Seems like udon doesnt support struct or scriptable object so this is ugly
    //these arrays need to be same length if they not ascrip may crash
    //[SerializeField] float[] asteroidsTime;
    ////[SerializeField] Vector3[] asteroidsStartPosition;
    //[SerializeField] Vector3[] asteroidsStartVelocity;
    //[SerializeField] Vector3[] asteroidsAngularVelocity;
    //[SerializeField] float[] asteroidsImpactSize;

    /*private void Update()
    {
        if (asteroidsCounter >= asteroidsTime.Length) return;


        timer += Time.deltaTime;

        if (timer > asteroidsTime[asteroidsCounter]) //Spawn asteroid
        {
            timer = 0f;

            Asteroid nextAsteroid = FindNextAsteroid();

            if (nextAsteroid == null) return; //theres nothing in pool

            nextAsteroid.MakeActive();

            //spawn on the surface of imaginary sphere defined by startposition
            nextAsteroid.transform.position = asteroidsStartPosition[asteroidsCounter].normalized * spawnSphereRadius;


            nextAsteroid.SetStartVelocity(asteroidsStartVelocity[asteroidsCounter]);
            nextAsteroid.SetAngularVelocity(asteroidsAngularVelocity[asteroidsCounter]);
            nextAsteroid.SetImpactSize(asteroidsImpactSize[asteroidsCounter]);

            asteroidsCounter++;
            RequestSerialization();
        }

    }*/
}



