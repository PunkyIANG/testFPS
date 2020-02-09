using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rigidbody;
    public float speed = 10f;
    public float mouseSensitivityX = 10f;
    public float mouseSensitivityY = 10f;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //getting the input
        moveDirection = transform.TransformDirection(moveDirection);                                        //localspace -> worldspace
        rigidbody.MovePosition(transform.position + moveDirection * speed * Time.fixedDeltaTime);           //applying movement

        var rotX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * mouseSensitivityX;
        var rotY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * mouseSensitivityY;

        
    }
}
