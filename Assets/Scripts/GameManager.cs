using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

#region enum

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

#endregion

public class GameManager : MonoBehaviourPunCallbacks
{
    #region References

    public static GameManager instance;
    PhotonView m_pv;

    #endregion

    #region RunTimeVariables

    LevelManagerStates m_currentManagerState;

    #endregion

    #region UnityMethods

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

    void Start()
    {
        m_pv = GetComponent<PhotonView>();
        SetManagerStates(LevelManagerStates.Waiting);
        if (PhotonNetwork.IsMasterClient)
        {
            //AssignRole();
        }
    }

    void Update()
    {
        
    }

    //private void OnEnable()
    //{
    //    PhotonNetwork.AddCallbackTarget(this);
    //}

    //private void OnDisable()
    //{
    //    PhotonNetwork.RemoveCallbackTarget(this);
    //}

    #endregion

    #region LocalMethods

    void SetNewRoleEvent()
    {
        byte m_ID = 1;
        object content = "Listo para la partida";
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(m_ID, content, raiseEventOptions, SendOptions.SendReliable);
    }

    #endregion

    #region PublicMethods

    public void SetManagerStates(LevelManagerStates p_newState)
    {
        
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
                Debug.Log("cambio el estado a Playing");
                Playing();
                break;
        }
    }

    public static void AssignRole()
    {
        Debug.Log("roles asignados");
        Player[] m_playersArray = PhotonNetwork.PlayerList;
        GameplayRoles[] m_gameplayRoles = { GameplayRoles.Inocent, GameplayRoles.Traitor };

        m_gameplayRoles = m_gameplayRoles.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < m_playersArray.Length; i++)
        {
            Hashtable m_playersProperties = new Hashtable();
            m_playersProperties["Role"] = m_gameplayRoles[i % m_gameplayRoles.Length].ToString();
            m_playersArray[i].SetCustomProperties(m_playersProperties);
        }
    }

    public override void OnPlayerEnteredRoom(Player newplayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            TimerToStart();
        }
    }

    public void disconnectFromCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainMenu");
    }

    //public void OnEvent(EventData photonEvent)
    //{
    //    byte eventCode = photonEvent.Code;
    //    if (eventCode == 1)
    //    {
    //        string data = (string)photonEvent.CustomData;
    //        //GetNewGameplayRole();
    //    }
    //}

    #endregion

    #region IEnumerator

    IEnumerator TimerToStart()
    {
        yield return new WaitForSeconds(3);
        SetManagerStates(LevelManagerStates.Playing);
    }

    #endregion

    #region FSMMethods

    /// <summary>
    /// Inicialize el Estado de Playing
    /// </summary>
    void Playing()
    {
        AssignRole();
        SetNewRoleEvent();    }

    #endregion

    #region SettersAndGetters

    public LevelManagerStates GetManagerState
    {
        get { return m_currentManagerState; }
    }

    #endregion
}
