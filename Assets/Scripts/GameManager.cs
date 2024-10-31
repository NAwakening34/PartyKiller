using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public enum GameplayRoles
{
    Inocent,
    Traitor
}

public enum LevelManagerStates
{
    None,
    Waiting,
    Playing
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    PhotonView m_pv;
    LevelManagerStates m_currentManagerState;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            m_pv = GetComponent<PhotonView>();
        }
        else
        {
            Destroy(instance);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_pv = GetComponent<PhotonView>();
        SetManagerStates(LevelManagerStates.Waiting);
        if (PhotonNetwork.IsMasterClient)
        {
            //AssignRole();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AssignRole()
    {
        Player[] m_playersArray = PhotonNetwork.PlayerList;
        GameplayRoles[] m_gameplayRoles = { GameplayRoles.Inocent, GameplayRoles.Traitor };

        m_gameplayRoles = m_gameplayRoles.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < m_playersArray.Length; i++)
        {
            Hashtable m_playersProperties = new Hashtable();
            m_playersProperties["Role"] = m_gameplayRoles[i %  m_gameplayRoles.Length].ToString();
            m_playersArray[i].SetCustomProperties(m_playersProperties);
        }
    }

    public void SetManagerStates(LevelManagerStates p_newState)
    {
        //ratearme el codigo de la ardilla y llamar a está función desde ahí
        if (p_newState == m_currentManagerState)
        {
            return;
        }
        m_currentManagerState = p_newState;

        switch (m_currentManagerState)
        {
            case LevelManagerStates.None:
                break;
            case LevelManagerStates.Waiting:
                break;
            case LevelManagerStates.Playing:
                Playing();
                break;
        }
    }

    void Playing()
    {
        AssignRole();
    }

    public LevelManagerStates GetManagerState
    {
        get { return m_currentManagerState; }
    }
}
