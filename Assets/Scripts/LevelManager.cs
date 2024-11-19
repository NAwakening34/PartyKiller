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

    [Range(0.1f, 1f)]
    [SerializeField] float traitorporcent;

    [SerializeField]PhotonView m_photonView;
    [SerializeField] TextMeshProUGUI m_textMeshProUGUI;

    [SerializeField] int m_innocentCount, m_traitorCount;

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

    void setVictoryEvent()
    {
        byte m_ID = 4;//Codigo del Evento (1...199)
        object content = "Termino";
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
                InnocentDied();
                break;
            case 3:
                TraitorDied();
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
        Player[] m_playersArray = PhotonNetwork.PlayerList;
        List <GameplayRole> m_gameplayRole = new List<GameplayRole>();
        int totalPlayers = m_playersArray.Length;

        m_traitorCount = Mathf.Max(1, Mathf.RoundToInt(totalPlayers * traitorporcent));
        m_innocentCount = totalPlayers - m_traitorCount;
        Debug.Log(m_traitorCount + " " + m_innocentCount);

        m_gameplayRole.AddRange(Enumerable.Repeat(GameplayRole.Traitor, m_traitorCount));
        m_gameplayRole.AddRange(Enumerable.Repeat(GameplayRole.Innocent, m_innocentCount));

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

    //Fisher-Yates Shuffle
    //void shuffleRoleList(List<GameplayRole> p_roleList)
    //{
    //    for (int i = p_roleList.Count - 1; i > 0; i--)
    //    {
    //        int j = Random.Range(0, i + 1);
    //        GameplayRole temp_role = p_roleList[i];
    //        p_roleList[i] = p_roleList[j];
    //        p_roleList[j] = temp_role;
    //    }
    //}

    void InnocentDied()
    {
        Debug.Log("entro a event");
        m_innocentCount--;
        if (m_innocentCount == 0)
        {
            m_photonView.RPC("winnerInfo", RpcTarget.All, false);
            setLevelManagerSate(LevelManagerState.Ending);
        }
    }

    void TraitorDied()
    {
        Debug.Log("entro a event");
        m_traitorCount--;
        if (m_traitorCount == 0)
        {
            m_photonView.RPC("winnerInfo", RpcTarget.All, true);
            setLevelManagerSate(LevelManagerState.Ending);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 4)
        {
            StartCoroutine(timerToStart());
        }
    }

    [PunRPC]
    void winnerInfo(bool inocentwin)
    {
        if (inocentwin)
        {
            m_textMeshProUGUI.text = "Innocent Win";
            m_textMeshProUGUI.color = UnityEngine.Color.cyan;
        }
        else
        {
            m_textMeshProUGUI.text = "Traitor Win";
            m_textMeshProUGUI.color = UnityEngine.Color.red;
        }
        setVictoryEvent();
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
