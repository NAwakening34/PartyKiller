using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    #region  Knobs

    [SerializeField] float m_speed;
    [SerializeField] float m_rotSpeed = 5;

    #endregion

    #region References

    [SerializeField] Rigidbody m_rb;
    [SerializeField] Animator m_anim;
    [SerializeField] PhotonView m_pv;
    [SerializeField] Transform m_camera;
    [SerializeField] Transform m_playerRenderer;
    [SerializeField] Transform m_ghost;
    [SerializeField] Transform m_orientation;
    [SerializeField] BoxCollider m_boxcollider;
    [SerializeField] ParticleSystem m_particleSystem;
    [SerializeField] MeshRenderer m_icon;
    [SerializeField] Material[] m_material;
    [SerializeField] TextMeshProUGUI m_textMeshProUGUI;

    #endregion

    #region RunTimeVariables

    float m_hor, m_vert;
    Vector3 m_direction;
    bool m_death, m_isDeath;

    #endregion

    #region UnityMethods

    private void Start()
    {
        m_boxcollider.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_particleSystem.Stop();
    }

    private void Update()
    {
        if (m_pv.IsMine && !m_isDeath)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                m_boxcollider.enabled = true;
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                m_boxcollider.enabled = false;
            }
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void OnTriggerStay(Collider other)
    {
        if (m_pv.IsMine && !m_isDeath)
        {
            if (other.CompareTag("NPC"))
            {
                other.GetComponent<EnemyNPCMovement>().DestroyNPC();
            }
            else if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>().DestroySelf();
            }
        }
    }

    public void OnEnable()
    {
        if (m_pv.IsMine)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent; 
        }
    }

    public void OnDisable()
    {
        if (m_pv.IsMine)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        }
    }

    #endregion

    #region LocalMethods

    void Movement()
    {
        if (m_pv.IsMine && !m_death)
        {
            m_orientation.forward = (transform.position - new Vector3(m_camera.position.x, transform.position.y, m_camera.position.z)).normalized;
            m_hor = Input.GetAxisRaw("Horizontal");
            m_vert = Input.GetAxisRaw("Vertical");
            m_direction = (m_hor * m_orientation.right + m_vert * m_orientation.forward).normalized;
            m_anim.SetFloat("MoveSpeed", m_direction.magnitude);
            if (m_direction.magnitude > 0)
            {
                m_playerRenderer.forward = Vector3.Slerp(m_playerRenderer.forward, m_direction.normalized, Time.fixedDeltaTime * m_rotSpeed);
            }
        }
    }

    void GetNewGameplayRole()
    {
        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object role))
        {
            string m_newPlayerRole = role.ToString();

            switch (m_newPlayerRole)
            {
                case "Inocent":
                    m_icon.material = m_material[0];
                    m_textMeshProUGUI.text = "Inocent";
                    m_textMeshProUGUI.color = Color.cyan;
                    break;
                case "Traitor":
                    m_icon.material = m_material[1];
                    m_textMeshProUGUI.text = "Traitor";
                    m_textMeshProUGUI.color = Color.red;
                    break;
            }
        }
    }

    void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            string data = (string)photonEvent.CustomData;
        }
    }

    #endregion

    #region PublicMethods

    public void DestroySelf()
    {
        
            m_pv.RPC("TakingDamage", RpcTarget.AllBuffered, 1);
        
    }

    #endregion

    #region RPCMethods

    [PunRPC]
    void TakingDamage(int p_damage)
    {
        Debug.Log("se murio");
        m_death = true;
        m_isDeath = true;
        StartCoroutine(WaitForParticleSystem());
    }

    #endregion

    #region IEnumarator

    IEnumerator WaitForParticleSystem()
    {
        m_particleSystem.Play();
        yield return new WaitForSeconds(m_particleSystem.main.duration);
        m_playerRenderer.gameObject.SetActive(false);
        m_playerRenderer = m_ghost;
        m_playerRenderer.gameObject.SetActive(true);
        m_anim = m_ghost.GetComponent<Animator>();
        m_death = false;
    }

    #endregion
}
