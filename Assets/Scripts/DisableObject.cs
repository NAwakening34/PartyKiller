using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DisableObject : MonoBehaviour
{
    PhotonView m_PV;

    [SerializeField] List<GameObject> m_listGameObj;
    [SerializeField] List<MonoBehaviour> m_listScripts;
    void Start()
    {
        m_PV = GetComponent<PhotonView>();
        if (!m_PV.IsMine)
        {
            disableObjects();
            disableScripts();
        }
    }

    void disableObjects()
    {
        foreach(GameObject obj in m_listGameObj)
        {
            Destroy(obj);
        }
    }

    void disableScripts()
    {
        foreach (MonoBehaviour scripts in m_listScripts)
        {
            Destroy(scripts);
            //scripts.enabled = false;
        }
    }
}
