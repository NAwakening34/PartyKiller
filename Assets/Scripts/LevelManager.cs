using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using TMPro;

//este script se lo copie a Carpi
public class LevelManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static LevelManager instance;
    public LevelManagerState m_currentState;

    PhotonView m_photonView;
    [SerializeField] TextMeshProUGUI m_textMeshProUGUI;

    int m_innocentCount, m_traitorCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    void Start()
    {
        m_photonView = GetComponent<PhotonView>();

        setLevelManagerSate(LevelManagerState.Waiting);
    }

    public void OnEnable()
    {
        if (m_photonView.IsMine)
        {
            PhotonNetwork.AddCallbackTarget(this);
        }
    }

    public void OnDisable()
    {
        if (m_photonView.IsMine)
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    /// <summary>
    /// Levanta el Evento cuando los jugadores esten listos para la partida
    /// </summary>
    void setNewRoleEvent()
    {
        byte m_ID = 1;//Codigo del Evento (1...199)
        object content = "Asignacion de nuevo rol...";
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

        PhotonNetwork.RaiseEvent(m_ID, content, raiseEventOptions, SendOptions.SendReliable);
    }
    public LevelManagerState CurrentState { get { return m_currentState; } }
    public LevelManagerState getLevelManagerSate()
    {
        return m_currentState;
    }

    public void setLevelManagerSate(LevelManagerState p_newState)
    {
        if (p_newState == m_currentState)
        {
            return;
        }
        m_currentState = p_newState;

        switch (m_currentState)
        {
            case LevelManagerState.None:
                break;
            case LevelManagerState.Waiting:
                break;
            case LevelManagerState.Playing:
                playing();
                break;
        }

    }

    //estás dos funciones las hice yo
    public void disconnectFromCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainMenu");
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        switch (eventCode)
        {
            case 1:
                break;
            case 2:
                PlayerDied(photonEvent.Parameters.ToString());
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Inicializa el estado de Playing
    /// </summary>
    void playing()
    {
        assignRole();
        setNewRoleEvent();
    }

    //Está función la hice yo
    void assignRole(){
        print("Se crea Hastable con la asignacion del nuevo rol");
        Player[] m_playersArray = PhotonNetwork.PlayerList;
        List <GameplayRole> m_gameplayRole = new List<GameplayRole>();

        if (m_playersArray.Length <=5 && m_playersArray.Length >= 4)
        {
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_traitorCount = 1;
            m_innocentCount = m_playersArray.Length - m_traitorCount;
        }
        else if (m_playersArray.Length <= 7 && m_playersArray.Length >= 6)
        {
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_traitorCount = 2;
            m_innocentCount = m_playersArray.Length - m_traitorCount;
        }
        else if (m_playersArray.Length <= 9 && m_playersArray.Length >= 8)
        {
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_gameplayRole.Add(GameplayRole.Traitor);
            m_traitorCount = 3;
            m_innocentCount = m_playersArray.Length - m_traitorCount;
        }

        for (int i = m_gameplayRole.Count; i < m_playersArray.Length; i++)
        {
            m_gameplayRole.Add(GameplayRole.Innocent);
        }

        int index = 0;
        for (int i = 0; i < m_playersArray.Length; i++)
        {
            Hashtable m_playerProperties = new Hashtable();
            index = Random.Range(0, m_gameplayRole.Count);
            m_playerProperties["Role"] = m_gameplayRole[index].ToString();
            m_gameplayRole.RemoveAt(index);
            m_playersArray[i].SetCustomProperties(m_playerProperties);
        }
    }

    void PlayerDied(string role)
    {
        switch (role)
        {
            case "Innocent":
                m_innocentCount--;
                if (m_innocentCount == 0)
                {
                    m_textMeshProUGUI.text = "Innocents wins";
                    m_textMeshProUGUI.color = Color.cyan;
                    setLevelManagerSate(LevelManagerState.Ending);
                }
                break;
            case "Traitor":
                m_traitorCount--;
                if (m_traitorCount == 0)
                {
                    m_textMeshProUGUI.text = "Traitors wins";
                    m_textMeshProUGUI.color = Color.red;
                    setLevelManagerSate(LevelManagerState.Ending);
                }
                break;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 4)
        {
            StartCoroutine(timerToStart());
        }
    }

    //Probablemente Se necesite RPC
    IEnumerator timerToStart()
    {
        yield return new WaitForSeconds(3);
        setLevelManagerSate(LevelManagerState.Playing);
    }
}
public enum LevelManagerState
{
    None,
    Waiting,
    Playing,
    Ending
}


public enum GameplayRole
{
    Innocent,
    Traitor
}
