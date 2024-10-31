using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateIcon : MonoBehaviour
{
    #region References

    [SerializeField] float rotationSpeed;

    #endregion

    #region UnityMethods
    
    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(Vector3.right, rotationSpeed*Time.deltaTime);
    }
    #endregion
}
