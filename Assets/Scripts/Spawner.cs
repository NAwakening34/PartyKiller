using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spawner : MonoBehaviour
{
    //[SerializeField] Transform[] spawnpos;
    // Start is called before the first frame update
    [SerializeField] PhotonView m_pv;
    void Start()
    {
        PhotonNetwork.Instantiate("Player", new Vector3 (Random.Range(-23, 23), transform.position.y, Random.Range(-23, 23)), Quaternion.identity);
    }
}
