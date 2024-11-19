using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.Xml.Serialization;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback
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
    [SerializeField] GameObject m_damage;
    [SerializeField] ParticleSystem m_particleSystem;
    [SerializeField] MeshRenderer m_icon;
    [SerializeField] Material[] m_material;
    [SerializeField] CapsuleCollider m_capsuleCollider;

    #endregion

    #region RunTimeVariables

    float m_hor, m_vert;
    Vector3 m_direction;
    [SerializeField]bool m_death, m_isDeath, m_canPlay, m_canAttack;
    string m_newPlayerRole;

    #endregion

    #region UnityMethods

    private void Start()
    {
        m_damage.SetActive(false);
        m_canAttack = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_particleSystem.Stop();
    }

    private void Update()
    {
        if (m_pv.IsMine)
        {
            if(!m_isDeath && m_canPlay && LevelManager.instance.m_currentState != LevelManagerState.Ending)
            {
                if (Input.GetKeyDown(KeyCode.E) && m_canAttack)
                {
                    m_damage.SetActive(true);
                    m_canAttack = false;
                    StartCoroutine(DeactivateAttack());
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DiedEvent(m_newPlayerRole);
                LevelManager.instance.disconnectFromCurrentRoom();
            }
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void OnTriggerStay(Collider other)
    {
        if (m_pv.IsMine && !m_isDeath && m_canPlay && LevelManager.instance.m_currentState != LevelManagerState.Ending)
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
            PhotonNetwork.AddCallbackTarget(this); 
        }
    }

    public void OnDisable()
    {
        if (m_pv.IsMine)
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    #endregion

    #region LocalMethods

    void Movement()
    {
        if (m_pv.IsMine && !m_death && m_canPlay)
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
            m_rb.velocity = m_direction.normalized * m_speed;
        }
    }

    void GetNewGameplayRole()
    {
        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object role))
        {
            m_newPlayerRole = role.ToString();

            switch (m_newPlayerRole)
            {
                case "Innocent":
                    m_icon.material = m_material[0];
                    break;
                case "Traitor":
                    m_icon.material = m_material[1];
                    break;
            }
            m_canPlay = true;
        }
    }

    void Victory()
    {
        if(!m_isDeath)
        {
            m_anim.SetTrigger("Victory");
            m_canPlay= false;
        }
    }

    #endregion

    #region PublicMethods

    public void DestroySelf()
    {
        if (!m_pv.IsMine && !m_isDeath)
        {
            m_pv.RPC("TakingDamage", RpcTarget.AllBuffered);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        switch (eventCode)
        {
            case 1:
                GetNewGameplayRole();
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                Victory();
                break;
            case 5:
                break;
            default:
                break;
        }
        //if (eventCode == 1)
        //{
        //    string data = (string)photonEvent.CustomData;
        //    GetNewGameplayRole();
        //}
    }

    #endregion

    #region RPCMethods

    [PunRPC]
    void TakingDamage()
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
        DiedEvent(m_newPlayerRole);
        m_playerRenderer.gameObject.SetActive(false);
        m_playerRenderer = m_ghost;
        m_playerRenderer.gameObject.SetActive(true);
        m_anim = m_ghost.GetComponent<Animator>();
        m_rb.useGravity = false;
        m_capsuleCollider.isTrigger = true;
        m_death = false;
    }

    IEnumerator DeactivateAttack()
    {
        yield return new WaitForSeconds(0.75f);
        m_damage.SetActive(false);
        yield return new WaitForSeconds(2.25f);
        m_canAttack = true;
    }

    #endregion

    #region Events

    void DiedEvent(string role)
    {
        if (m_pv.IsMine)
        {
            byte m_ID;
            if (role == "Innocent")
            {
                m_ID = 2;
            }
            else
            {
                m_ID = 3;
            }
            object content = role;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };

            PhotonNetwork.RaiseEvent(m_ID, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    #endregion
}
