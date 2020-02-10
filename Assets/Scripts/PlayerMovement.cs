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
    Vector3 previousMousePosition;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        var moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //getting the input
        moveDirection = transform.TransformDirection(moveDirection);                                        //localspace -> worldspace
        rigidbody.MovePosition(transform.position + moveDirection * speed * Time.fixedDeltaTime);           //applying movement

        var rotX = - (previousMousePosition - Input.mousePosition).x * Time.fixedDeltaTime * mouseSensitivityX;
        var rotY = (previousMousePosition - Input.mousePosition).y * Time.fixedDeltaTime * mouseSensitivityY;

        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0f, rotX, 0f));

        previousMousePosition = Input.mousePosition;
        if (Camera.main.transform.rotation.eulerAngles.x + rotY > 90f) { //fix clamp at 0 degrees
            rotY = 90f - Camera.main.transform.rotation.eulerAngles.x;
            print("clamp at 90");
        }
        /* else if (Camera.main.transform.rotation.eulerAngles.x + rotY < -90f){
            rotY = - 90f + Camera.main.transform.rotation.eulerAngles.x;
            print("clamp at -90");
            } */
        

        Camera.main.transform.rotation *= Quaternion.Euler(rotY, 0f, 0f);
    }
}
