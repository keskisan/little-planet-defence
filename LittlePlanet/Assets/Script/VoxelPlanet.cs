
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VoxelPlanet : UdonSharpBehaviour
{
    [SerializeField]
    MenuScript menuScript;

    [HideInInspector]
    public int maxLength;

    [SerializeField]
    int xDir = 20, yDir = 20, zDir = 20;

    [SerializeField]
    float VoxelScale = 1f;

    [SerializeField]
    MeshCollider meshCollider;

    [SerializeField]
    float offsetValue;

    [SerializeField, Range(1, 500)]
    int counter = 100;

    float workDone = 0;
    float workNeedToBeDone;

    [HideInInspector]
    public float percentageComplete; //this the veriable to indicate progress

    Vector3 offsetxPlus;
    Vector3 offsetxMinus;
    Vector3 offsetyPlus;
    Vector3 offsetyMinus;
    Vector3 offsetzPlus;
    Vector3 offsetzMinus;

    [HideInInspector]
    public bool[] voxelArray;

    [HideInInspector]
    public int[] voxelArrayBufferNetworking;
    [HideInInspector]
    public int voxelArrayBufferNetworkingCounter;

    int[] voxelArrayCorners;

    Mesh mesh;

    Vector3[] newVertices;
    int VerticeCounter;
    Vector2[] newUV;
    int[] newTriangles;
    int TriangleCounter;

    int voxelValuesX, voxelValuesY, voxelValuesZ, voxelValuesCounter;

    bool hasSetVoxelValues = false, hasCalculateVoxels = false, hasSetVoxelNoise = false, meshDrawed = false;


    [SerializeField]
    Vector3 planetCenter = Vector3.zero;
    [SerializeField]
    float planetRadiusSquared = 100;

    bool ShouldUpdateVoxels;

    [SerializeField]
    GameObject startPlatform;

    [SerializeField]
    TowerManager towerManager;

    int planetHealth = 0;

    public Vector3 gravityPosition
    {
        get
        {
            return planetCenter;
        }
    }

    [SerializeField]
    float _fallSpeed = 0.01f;
    public float fallSpeed
    {
        get => _fallSpeed;
    }

    [SerializeField]
    float _drag = 0.2f;
    public float drag
    {
        get => _drag;
    }

    void Start()
    {
        offsetxPlus = new Vector3(offsetValue, 0f, 0f);
        offsetxMinus = new Vector3(-offsetValue, 0f, 0f);
        offsetyPlus = new Vector3(0f, offsetValue, 0f);
        offsetyMinus = new Vector3(0f, -offsetValue, 0f);
        offsetzPlus = new Vector3(0f, 0f, offsetValue);
        offsetzMinus = new Vector3(0f, 0f, -offsetValue);


        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        DefaultValuesAndSettings();
    }

    public void NetworkedUpdateVoxelSettings(bool[] updatedVoxels)
    {
        maxLength = xDir * yDir * zDir;
        voxelArray = updatedVoxels;
        hasSetVoxelNoise = true;

        BasicVoxelSetup();
        workDone = maxLength;
    }

    void DefaultValuesAndSettings()
    {
        maxLength = xDir * yDir * zDir;
        workNeedToBeDone = maxLength * 3;
        voxelArray = new bool[maxLength];
        hasSetVoxelNoise = false;

        BasicVoxelSetup();
        ResetVoxelArrayBufferNetworking();
        workDone = 0;
    }

    public void ResetVoxelArrayBufferNetworking()
    {
        voxelArrayBufferNetworking = new int[100];
        voxelArrayBufferNetworkingCounter = 0;
    }

    void UpdateVoxelsSettings()
    {
        BasicVoxelSetup();
    }

    private void BasicVoxelSetup()
    {
        voxelArrayCorners = new int[(xDir + 1) * (yDir + 1) * (zDir + 1)];

        voxelValuesX = voxelValuesY = voxelValuesZ = voxelValuesCounter = 0;

        VerticeCounter = 0;
        TriangleCounter = 0;

        newVertices = new Vector3[24000];
        newUV = new Vector2[24000];
        newTriangles = new int[36000];

        hasSetVoxelValues = false;
        hasCalculateVoxels = false;
        meshDrawed = false;

        planetHealth = 0;
    }

    float beginDelay;

    private void Update()
    {
        if (beginDelay < 1f)
        {
            beginDelay += Time.deltaTime;
            return;
        }


        if (!hasSetVoxelNoise)
        {
            SetVoxelSphere();
        }
        else if (!hasSetVoxelValues)
        {
            SetVoxelsValues();
        }
        else if (!hasCalculateVoxels)
        {
            CalculateVoxels();
        }
        else if (!meshDrawed)
        {
            DrawMesh();
        }


        percentageComplete = workDone / workNeedToBeDone;
    }



    void SetVoxelSphere()
    {
        for (int i = 0; i < counter; i++)
        {
            workDone++;

            Vector3 offset = new Vector3(voxelValuesX, voxelValuesY, voxelValuesZ) + transform.position - planetCenter;

            voxelArray[voxelValuesCounter] = offset.sqrMagnitude < planetRadiusSquared? true : false;

            voxelValuesCounter++;
            voxelValuesZ++;
            if (voxelValuesZ == zDir)
            {
                voxelValuesZ = 0;
                voxelValuesY++;
            }
            if (voxelValuesY == yDir)
            {
                voxelValuesY = 0;
                voxelValuesX++;
            }
            if (voxelValuesX == xDir)
            {
                hasSetVoxelNoise = true;
                voxelValuesX = voxelValuesY = voxelValuesZ = voxelValuesCounter = 0;
                return;
            }
        }
    }

    void SetVoxelsValues()
    {
        for (int i = 0; i < counter; i++)
        {
            workDone++;

            if (voxelArray[voxelValuesCounter])
            {
                Add1voxelArrayCorners(voxelValuesX, voxelValuesY, voxelValuesZ);
                Add1voxelArrayCorners(voxelValuesX + 1, voxelValuesY, voxelValuesZ);
                Add1voxelArrayCorners(voxelValuesX, voxelValuesY + 1, voxelValuesZ);
                Add1voxelArrayCorners(voxelValuesX + 1, voxelValuesY + 1, voxelValuesZ);

                Add1voxelArrayCorners(voxelValuesX, voxelValuesY, voxelValuesZ + 1);
                Add1voxelArrayCorners(voxelValuesX + 1, voxelValuesY, voxelValuesZ + 1);
                Add1voxelArrayCorners(voxelValuesX, voxelValuesY + 1, voxelValuesZ + 1);
                Add1voxelArrayCorners(voxelValuesX + 1, voxelValuesY + 1, voxelValuesZ + 1);

            }
            voxelValuesCounter++;
            voxelValuesZ++;
            if (voxelValuesZ == zDir)
            {
                voxelValuesZ = 0;
                voxelValuesY++;
            }
            if (voxelValuesY == yDir)
            {
                voxelValuesY = 0;
                voxelValuesX++;
            }
            if (voxelValuesX == xDir)
            {
                hasSetVoxelValues = true;
                voxelValuesX = voxelValuesY = voxelValuesZ = voxelValuesCounter = 0;
                return;
            }
        }
    }

    void CalculateVoxels()
    {
        for (int i = 0; i < counter; i++)
        {
            workDone++;

            if (voxelArray[voxelValuesCounter]) //only add mesh if it is
            {
                planetHealth++;
                CheckZPosFace(voxelValuesX, voxelValuesY, voxelValuesZ);
                CheckZNegFace(voxelValuesX, voxelValuesY, voxelValuesZ);
                CheckYPosFace(voxelValuesX, voxelValuesY, voxelValuesZ);
                CheckYNegFace(voxelValuesX, voxelValuesY, voxelValuesZ);
                CheckXPosFace(voxelValuesX, voxelValuesY, voxelValuesZ);
                CheckXNegFace(voxelValuesX, voxelValuesY, voxelValuesZ);
            }

            voxelValuesCounter++;
            voxelValuesZ++;
            if (voxelValuesZ == zDir)
            {
                voxelValuesZ = 0;
                voxelValuesY++;
            }
            if (voxelValuesY == yDir)
            {
                voxelValuesY = 0;
                voxelValuesX++;
            }
            if (voxelValuesX == xDir)
            {
                hasCalculateVoxels = true;
                return;
            }
        }
    }

    

    public void SetVoxelValue(Vector3 position, float impactDistance) //position can be outside range
    {
        ShouldUpdateVoxels = false;

        position = position - transform.position / VoxelScale;
        int offset = Mathf.RoundToInt(impactDistance);
        int diameter = offset + offset;

        int xStart = Mathf.RoundToInt(position.x) - offset;
        int yStart = Mathf.RoundToInt(position.y) - offset;
        int zStart = Mathf.RoundToInt(position.z) - offset;

        int xEnd = xStart + diameter;
        int yEnd = yStart + diameter;
        int zEnd = zStart + diameter;

        float impactDistanceSquaredScaled = impactDistance * impactDistance / VoxelScale;

        for (int x = xStart; x < xEnd; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                for (int z = zStart; z < zEnd; z++)
                {
                    if ((position - new Vector3(x, y, z)).sqrMagnitude < impactDistanceSquaredScaled) //optimised distance test
                    {
                        SetVoxelValueIndv(x, y, z);
                    }  
                }
            }
        }

        if (ShouldUpdateVoxels)
        {
            UpdateVoxelsSettings();
        }
    }

    private void SetVoxelValueIndv(int x, int y, int z)
    {
        if (x < 0) return;
        if (y < 0) return;
        if (z < 0) return;
        if (x >= xDir) return;
        if (y >= yDir) return;
        if (z >= zDir) return;

        int positionInArray = z + zDir * y + zDir * yDir * x;

        if (voxelArray[positionInArray] != false)
        {
            voxelArray[positionInArray] = false;

            AddValueVoxelArrayBufferNetworking(-positionInArray); 
            ShouldUpdateVoxels = true;
        }
    }

    public void AddValueVoxelArrayBufferNetworking(int value) //positive values true negatives false
    {
        voxelArrayBufferNetworking[voxelArrayBufferNetworkingCounter] = value;
        voxelArrayBufferNetworkingCounter++;

        if (voxelArrayBufferNetworkingCounter == voxelArrayBufferNetworking.Length)
        {
            voxelArrayBufferNetworkingCounter = 0;
        }
    }

    public bool GetVoxelValue(int x, int y, int z)
    {
        return voxelArray[z + zDir * y + zDir * yDir * x];
    }


    void Add1voxelArrayCorners(int x, int y, int z)
    {
        voxelArrayCorners[z + zDir * y + zDir * yDir * x] += 1;
    }


    void Remove1voxelArrayCorners(int x, int y, int z)
    {
        voxelArrayCorners[z + zDir * y + zDir * yDir * x] -= 1;
        if (voxelArrayCorners[z + zDir * y + zDir * yDir * x] < 0)
        {
            voxelArrayCorners[z + zDir * y + zDir * yDir * x] = 0;
        }
    }

    public int GetvoxelArrayCorners(int x, int y, int z) //new int[(chunkManager.xDir + 1) * (chunkManager.yDir + 1) * (chunkManager.zDir + 1)
    {
        return voxelArrayCorners[z + zDir * y + zDir * yDir * x];
    }

    Vector3 GetPointOffset(int x, int y, int z)
    {
        Vector3 offsetVector = Vector3.zero;
        if (x > 0) //x minus
        {
            if (GetvoxelArrayCorners(x - 1, y, z) > 0 && GetvoxelArrayCorners(x - 1, y, z) < 8)
            {
                offsetVector += offsetxMinus;
            }
        }     
        if (x < xDir) //x plus
        {
            if (GetvoxelArrayCorners(x + 1, y, z) > 0 && GetvoxelArrayCorners(x + 1, y, z) < 8)
            {
                offsetVector += offsetxPlus;
            }
        }
        

        if (y > 0) //y minus
        {
            if (GetvoxelArrayCorners(x, y - 1, z) > 0 && GetvoxelArrayCorners(x, y - 1, z) < 8)
            {
                offsetVector += offsetyMinus;
            }
        }
        if (y < yDir) //y plus
        {
            if (GetvoxelArrayCorners(x, y + 1, z) > 0 && GetvoxelArrayCorners(x, y + 1, z) < 8)
            {
                offsetVector += offsetyPlus;
            }
        }

        if (z > 0) //z minus
        {
            if (GetvoxelArrayCorners(x, y, z - 1) > 0 && GetvoxelArrayCorners(x, y, z - 1) < 8)
            {
                offsetVector += offsetzMinus;
            }
        } 
        if (z < zDir) //zplus
        {
            if (GetvoxelArrayCorners(x, y, z + 1) > 0 && GetvoxelArrayCorners(x, y, z + 1) < 8)
            {
                offsetVector += offsetzPlus;
            }
        }
        
        return offsetVector;
    }

    void AddFaceZpos(int x, int y, int z)
    {
        if (y == yDir - 1) //top so be grass
        {
            AddQuad(
                   new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                   new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
        }
        else
        {
            if (GetVoxelValue(x, y + 1, z)) //block ontop so ground
            {
                AddQuad(
                   new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                   new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector2(0f, 0f),
                   new Vector2(0f, 0.33f),
                   new Vector2(1f, 0.33f),
                   new Vector2(1f, 0f)
               );
            }
            else //top so be grass
            {
                AddQuad(
                   new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                   new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
            }
        }
    }

    void AddFaceZneg(int x, int y, int z)
    {
        if (y == yDir - 1) //top so be grass
        {
            AddQuad(
                    new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                    new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                    new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                    new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
        }
        else
        {
            if (GetVoxelValue(x, y + 1, z)) //block ontop so ground
            {
                AddQuad(
                    new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                    new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                    new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                    new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                   new Vector2(0f, 0f),
                   new Vector2(0f, 0.33f),
                   new Vector2(1f, 0.33f),
                   new Vector2(1f, 0f)
               );
            }
            else //top so be grass
            {
                AddQuad(
                    new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                    new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                    new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                    new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
            }
        }
    }

    void AddFaceYpos(int x, int y, int z)
    {
        AddQuad(
                   new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                   new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                   new Vector2(0f, 0.66f),
                   new Vector2(0f, 1f),
                   new Vector2(1f, 1f),
                   new Vector2(1f, 0.66f)
               );
    }

    void AddFaceYneg(int x, int y, int z)
    {
        AddQuad(
            new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
            new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
            new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
            new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),

            new Vector2(0f, 0f),
            new Vector2(0f, 0.33f),
            new Vector2(1f, 0.33f),
            new Vector2(1f, 0f)
       );

    }

    void AddFaceXpos(int x, int y, int z)
    {
        if (y == yDir - 1) //top so be grass
        {
            AddQuad(
                new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                new Vector2(0f, 0.33f),
                new Vector2(0f, 0.66f),
                new Vector2(1f, 0.66f),
                new Vector2(1f, 0.33f)
               );
        }
        else
        {
            if (GetVoxelValue(x, y + 1, z)) //block ontop so ground
            {
                AddQuad(
                   new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                   new Vector2(0f, 0f),
                   new Vector2(0f, 0.33f),
                   new Vector2(1f, 0.33f),
                   new Vector2(1f, 0f)
               );
            }
            else //top so be grass
            {
                AddQuad(
                   new Vector3(x + 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x + 1, y, z),
                new Vector3(x + 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x + 1, y + 1, z),
                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x + 1, y + 1, z + 1),
                new Vector3(x + 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x + 1, y, z + 1),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
            }
        }
    }

    void AddFaceXneg(int x, int y, int z)
    {
        if (y == yDir - 1) //top so be grass
        {
            AddQuad(
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                   new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
        }
        else
        {
            if (GetVoxelValue(x, y + 1, z)) //block ontop so ground
            {
                AddQuad(
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                   new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                   new Vector2(0f, 0f),
                   new Vector2(0f, 0.33f),
                   new Vector2(1f, 0.33f),
                   new Vector2(1f, 0f)
               );
            }
            else //top so be grass
            {
                AddQuad(
                   new Vector3(x - 0.5f, y - 0.5f, z + 0.5f) + GetPointOffset(x, y, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z + 0.5f) + GetPointOffset(x, y + 1, z + 1),
                   new Vector3(x - 0.5f, y + 0.5f, z - 0.5f) + GetPointOffset(x, y + 1, z),
                   new Vector3(x - 0.5f, y - 0.5f, z - 0.5f) + GetPointOffset(x, y, z),
                   new Vector2(0f, 0.33f),
                   new Vector2(0f, 0.66f),
                   new Vector2(1f, 0.66f),
                   new Vector2(1f, 0.33f)
               );
            }
        }
    }

    void AddQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
    {
        newVertices[VerticeCounter] = p1 * VoxelScale; //1
        newUV[VerticeCounter] = v1;
        VerticeCounter++;
        newVertices[VerticeCounter] = p2 * VoxelScale; //2
        newUV[VerticeCounter] = v2;
        VerticeCounter++;
        newVertices[VerticeCounter] = p3 * VoxelScale; //3
        newUV[VerticeCounter] = v3;
        VerticeCounter++;
        newVertices[VerticeCounter] = p4 * VoxelScale; //4
        newUV[VerticeCounter] = v4;
        VerticeCounter++;

        newTriangles[TriangleCounter] = VerticeCounter - 4; //1
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 3; //2
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 2; //3
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 2; //3 
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 1; //4
        TriangleCounter++;
        newTriangles[TriangleCounter] = VerticeCounter - 4; //1
        TriangleCounter++;
    }


    void CheckZPosFace(int x, int y, int z)
    {
        if (z == zDir - 1) //if at edge add face
        {
            AddFaceZpos(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x, y, z + 1)) //if not voxel next to addface
            {
                AddFaceZpos(x, y, z);
            }
        }
    }

    void CheckZNegFace(int x, int y, int z)
    {
        if (z == 0) //if at edge add face
        {
            AddFaceZneg(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x, y, z - 1)) //if not voxel next to addface
            {
                AddFaceZneg(x, y, z);
            }
        }
    }

    void CheckYPosFace(int x, int y, int z)
    {
        if (y == yDir - 1) //if at edge add face
        {
            AddFaceYpos(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x, y + 1, z)) //if not voxel next to addface
            {
                AddFaceYpos(x, y, z);
            }
        }
    }

    void CheckYNegFace(int x, int y, int z)
    {
        if (y == 0) //if at edge add face
        {
            AddFaceYneg(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x, y - 1, z)) //if not voxel next to addface
            {
                AddFaceYneg(x, y, z);
            }
        }
    }

    void CheckXPosFace(int x, int y, int z)
    {
        if (x == xDir - 1) //if at edge add face
        {
            AddFaceXpos(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x + 1, y, z)) //if not voxel next to addface
            {
                AddFaceXpos(x, y, z);
            }
        }
    }

    void CheckXNegFace(int x, int y, int z)
    {
        if (x == 0) //if at edge add face
        {
            AddFaceXneg(x, y, z);
        }
        else
        {
            if (!GetVoxelValue(x - 1, y, z)) //if not voxel next to addface
            {
                AddFaceXneg(x, y, z);
            }
        }
    }

    void DrawMesh()
    {
        if (VerticeCounter == 0) return;
        mesh.Clear();
        Vector3[] tmpVertices = new Vector3[VerticeCounter];
        Array.Copy(newVertices, 0, tmpVertices, 0, VerticeCounter);
        Vector2[] tmpUVS = new Vector2[VerticeCounter];
        Array.Copy(newUV, 0, tmpUVS, 0, VerticeCounter);
        int[] tmpTriangles = new int[TriangleCounter];
        Array.Copy(newTriangles, 0, tmpTriangles, 0, TriangleCounter);
        mesh.vertices = tmpVertices;
        mesh.uv = tmpUVS;
        mesh.triangles = tmpTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        meshCollider.sharedMesh = mesh;
        meshDrawed = true;

        OtherGameStuffUpdate();
        
    }

    private void OtherGameStuffUpdate() 
    {
        menuScript.UpdatePlanetHealth(planetHealth);
        if (startPlatform.activeSelf) //need happen only once at start the game
        {
            startPlatform.SetActive(false);
            towerManager.whenGroundDissapears();


        }
    }
}
