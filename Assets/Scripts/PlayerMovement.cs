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
    public float maxCameraXAngle = 90f;
    public bool invertY = false;
    Vector3 previousMousePosition;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {     //we're handling input in update for better gameplay feel
        CharMovement();
        CameraRotation();
    }

    void CharMovement() {
        var moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //getting the input
        moveDirection = transform.TransformDirection(moveDirection);                                        //localspace -> worldspace
        rigidbody.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);           //applying movement
    }
    void CameraRotation() {
        var rotX = (Input.mousePosition - previousMousePosition).x * Time.deltaTime * mouseSensitivityX;
        var rotY = - (Input.mousePosition - previousMousePosition).y * Time.deltaTime * mouseSensitivityY;

        if (invertY)
            rotY *= -1;

        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0f, rotX, 0f));
        
        //clamp it

        var rawCameraRotation = Camera.main.transform.rotation * Quaternion.Euler(rotY, 0f, 0f);    //getting the unclamped rotation

        if (Quaternion.Angle(rigidbody.rotation, rawCameraRotation) > maxCameraXAngle)  {           //if the rotation x angle is too big
            var lookAtPosition = Camera.main.transform.TransformDirection(Vector3.forward);         //we check the lookAt direction
            if (Vector3.Dot(lookAtPosition, Vector3.up) > 0f) {                                     //if player looks up
                Camera.main.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);    //we clamp it up
            } else {
                Camera.main.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.right);     //else we clamp it down
            }
        } else {
            Camera.main.transform.rotation *= Quaternion.Euler(rotY, 0f, 0f);                       //if not, we just add the rotation as usual
        }

        previousMousePosition = Input.mousePosition;

    }
}
