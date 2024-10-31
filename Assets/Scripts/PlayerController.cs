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
    int m_life = 1;

    #endregion

    #region UnityMethods

    private void Start()
    {
        m_life = 1;
        m_boxcollider.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_particleSystem.Stop();
    }

    private void Update()
    {
        if (m_pv.IsMine)
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
        if (m_pv.IsMine)
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

    #endregion

    #region LocalMethods

    void Movement()
    {
        if (m_pv.IsMine)
        {
            m_orientation.forward = (transform.position - new Vector3(m_camera.position.x, transform.position.y, m_camera.position.z)).normalized;
            m_anim.SetFloat("MoveSpeed", m_direction.magnitude);
            m_hor = Input.GetAxisRaw("Horizontal");
            m_vert = Input.GetAxisRaw("Vertical");
            m_direction = (m_hor * m_orientation.right + m_vert * m_orientation.forward).normalized;
            if (m_direction.magnitude > 0)
            {
                m_playerRenderer.forward = Vector3.Slerp(m_playerRenderer.forward, m_direction.normalized, Time.fixedDeltaTime * m_rotSpeed);
            }
            m_rb.velocity = m_direction * m_speed;
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
        m_life -= p_damage;
        if (m_life <= 0)
        {
            StartCoroutine(WaitForParticleSystem());
        }
    }

    #endregion

    #region IEnumarator

    IEnumerator WaitForParticleSystem()
    {
        m_particleSystem.Play();
        yield return new WaitForSeconds(m_particleSystem.main.duration);
        Destroy(gameObject);
    }

    #endregion
}
