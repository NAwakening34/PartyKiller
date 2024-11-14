using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PhotonConnection : MonoBehaviourPunCallbacks
{
    [SerializeField] 
    TextMeshProUGUI m_errortext;
    [SerializeField]
    GameObject m_Loading, m_Scene;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("se ha conectado al master");
        m_Loading.SetActive(false);
        m_Scene.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Se ha entrado al lobby Abstracto");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("se entro al room");
        PhotonNetwork.LoadLevel("Gameplay");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogWarning("Hubo un erro al crear un room: " + message);
        m_errortext.text = "An error ocurred while trying to create the room";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogWarning("Hubo un erro al entrar: " + message);
        m_errortext.text = "An error ocurred while trying to enter the room ";
    }

    RoomOptions NewRoomInfo()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 9;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        return roomOptions;
    }

    public void CreateJoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom("TTTRoom", NewRoomInfo(), null);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
