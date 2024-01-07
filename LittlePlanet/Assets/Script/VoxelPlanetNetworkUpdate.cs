
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[DefaultExecutionOrder(1), UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class VoxelPlanetNetworkUpdate : UdonSharpBehaviour
{
    [SerializeField]
    VoxelPlanet voxelPlanet;

    [SerializeField]
    public bool updateDataBool = false;
    ushort sendDataCounter = 0; //16bit
    float timer;

    bool[] voxelArraynetworkedGetData;

    [HideInInspector]
    public float percentageComplete; //this the veriable to indicate progress

    private void Start()
    {
        ResetThis();
    }

    private void ResetThis()
    {
        updateDataBool = false;
        sendDataCounter = 0; //16bit
        voxelArraynetworkedGetData = new bool[voxelPlanet.maxLength];
    }

    public void sendNew()
    {
        updateDataBool = true;
        sendDataCounter = 0; //16bit
        voxelArraynetworkedGetData = new bool[voxelPlanet.maxLength];
    }

    private void Update()
    {
        if (updateDataBool)
        {
            SendDataNetwork();
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(SendData))]  //health > 0 player in game
    private int _sendData;  //Unsigned 64-bit integer
    public int SendData
    {
        set
        {
            int tmp = _sendData = value;
            if (Networking.IsOwner(gameObject)) return; //master sends not recieve
            bool[] tempArray = new bool[19];

            if (tmp > 1073741824)
            {
                tmp -= 1073741824;
                tempArray[18] = true;
            }
            if (tmp > 536870912)
            {
                tmp -= 536870912;
                tempArray[17] = true;
            }
            if (tmp > 268435456)
            {
                tmp -= 268435456;
                tempArray[16] = true;
            }
            if (tmp > 134217728)
            {
                tmp -= 134217728;
                tempArray[15] = true;
            }
            if (tmp > 67108864)
            {
                tmp -= 67108864;
                tempArray[14] = true;
            }
            if (tmp > 33554432)
            {
                tmp -= 33554432;
                tempArray[13] = true;
            }
            if (tmp > 16777216)
            {
                tmp -= 16777216;
                tempArray[12] = true;
            }
            if (tmp > 8388608)
            {
                tmp -= 8388608;
                tempArray[11] = true;
            }

            if (tmp > 4194304)
            {
                tmp -= 4194304;
                tempArray[10] = true;
            }
            if (tmp > 2097152)
            {
                tmp -= 2097152;
                tempArray[9] = true;
            }
            if (tmp > 1048576)
            {
                tmp -= 1048576;
                tempArray[8] = true;
            }
            if (tmp > 524288)
            {
                tmp -= 524288;
                tempArray[7] = true;
            }
            if (tmp > 262144)
            {
                tmp -= 262144;
                tempArray[6] = true;
            }
            if (tmp > 131072)
            {
                tmp -= 131072;
                tempArray[5] = true;
            }
            if (tmp > 65536)
            {
                tmp -= 65536;
                tempArray[4] = true;
            }

            if (tmp > 32768)
            {
                tmp -= 32768;
                tempArray[3] = true;
            }
            if (tmp > 16384)
            {
                tmp -= 16384;
                tempArray[2] = true;
            }
            if (tmp > 8192)
            {
                tmp -= 8192;
                tempArray[1] = true;
            }
            if (tmp > 4096)
            {
                tmp -= 4096;
                tempArray[0] = true;
            }
            //Debug.Log(tmp);
            percentageComplete = (float)tmp / (float)voxelPlanet.maxLength;
            for (int i = 0; i < tempArray.Length; i++)
            {
                if (tmp + i < voxelArraynetworkedGetData.Length)
                {
                    voxelArraynetworkedGetData[tmp + i] = tempArray[i];
                }
                else
                {
                    if (tmp == 4096) return; //catching strange bug where only this value recieved just after mesh update rewriting mesh with nothing
                    Debug.Log("//////////////////////////////////////////////VoxelLandNetworking updating mesh");
                    if (checkIfDiffrences())
                    {
                        MergeWithNetworkingBuffer(); //should prevent loss on anything done while updating via network

                        voxelPlanet.NetworkedUpdateVoxelSettings(voxelArraynetworkedGetData);
                        Debug.Log("mesh updated");
                    }

                    return;
                }

            }
            /*2 4 8 16 32 64 128 256 
             512 1024 2048 4096 8192 16384 32768 65536
            131072 262144 524288 1048576 2097152 4194304 8388608 16777216
            33554432 67108864 134217728 268435456 536870912 1073741824 2147483648 //4294967296
            */
        }
        get => _sendData;
    }

    private void MergeWithNetworkingBuffer() //buffer is merged with networking data for most accurate result
    {
        for (int i = 0; i < voxelPlanet.voxelArrayBufferNetworking.Length; i++)
        {
            int pos = i + voxelPlanet.voxelArrayBufferNetworkingCounter + 1;
            if (pos >= voxelPlanet.voxelArrayBufferNetworking.Length) pos -= voxelPlanet.voxelArrayBufferNetworking.Length;

            if (voxelPlanet.voxelArrayBufferNetworking[pos] != 0)
            {
                if (voxelPlanet.voxelArrayBufferNetworking[pos] > 0)
                {
                    voxelArraynetworkedGetData[voxelPlanet.voxelArrayBufferNetworking[pos]] = true;
                }
                else
                {
                    voxelArraynetworkedGetData[-voxelPlanet.voxelArrayBufferNetworking[pos]] = false;
                }
            }

        }
    }

    private bool checkIfDiffrences()
    {
        for (int i = 0; i < voxelPlanet.voxelArray.Length; i++)
        {
            if (voxelPlanet.voxelArray[i] != voxelArraynetworkedGetData[i])
            {
                return true;
            }
        }
        return false;
    }

    //first 11 bits array index then the next 19 voxel bool
    private void SendDataNetwork() //2 power 11 = 2048 voxels of 10 by 10 by 20 is 2000
    {
        if (!Networking.IsOwner(gameObject)) return; //owner should be master
        if (timer < 0f)
        {
            timer = 0.5f; //send every so often
            int tmpData = sendDataCounter; //first 11 bits just array index next is bool for each array entity

            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 4096;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 8192;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 16384;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 32768;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 65536;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 131072;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 262144;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 524288;
                sendDataCounter++;
            }


            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 1048576;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 2097152;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 4194304;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 8388608;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 16777216;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 33554432;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 67108864;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 134217728;
                sendDataCounter++;
            }

            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 268435456;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 536870912;
                sendDataCounter++;
            }
            if (sendDataCounter < voxelPlanet.voxelArray.Length)
            {
                if (voxelPlanet.voxelArray[sendDataCounter]) tmpData += 1073741824;
                sendDataCounter++;
            }

            /*2 4 8 16 32 64 128 256 
             512 1024 2048 4096 8192 16384 32768 65536
            131072 262144 524288 1048576 2097152 4194304 8388608 16777216
            33554432 67108864 134217728 268435456 536870912 1073741824 2147483648 //4294967296
            */

            SendData = tmpData; //send data
            RequestSerialization();

            percentageComplete = (float)sendDataCounter / (float)voxelPlanet.maxLength;

            if (sendDataCounter >= voxelPlanet.voxelArray.Length)
            {
                ResetThis();
            }
        }
        timer -= Time.deltaTime;
    }


}
