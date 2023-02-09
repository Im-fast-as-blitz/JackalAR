using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = new Vector3(0, 2, 0);
        PhotonNetwork.Instantiate(player.name, pos, Quaternion.identity);
    }
}