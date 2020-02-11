using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovement : MonoBehaviour
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);  //importing dll for setting cursor position

    new Rigidbody rigidbody;
    public float speed = 10f;
    public float mouseSensitivity = 10f;
    public float maxCameraXAngle = 90f;
    public bool invertY = false;
    public MouseMovementType selectedMouseMovementType;
    Vector3 previousMousePosition;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;


    }

    void Update() {     //we're handling input in update for better gameplay feel
        CharMovement();
        CameraRotation();
    }

    void CharMovement() {
        var moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //getting the input
        moveDirection = transform.TransformDirection(moveDirection);                                        //localspace -> worldspace
        rigidbody.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);                //applying movement
    }

    void CameraRotation() {
        var mouseDelta = GetMouseInputAsPositionDelta() * mouseSensitivity * Time.deltaTime;

        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0f, mouseDelta.x, 0f));
        
        //clamp it

        var rawCameraRotation = Camera.main.transform.rotation * Quaternion.Euler(mouseDelta.y, 0f, 0f);    //getting the unclamped rotation

        if (Quaternion.Angle(rigidbody.rotation, rawCameraRotation) > maxCameraXAngle)  {           //if the rotation x angle is too big
            var lookAtPosition = Camera.main.transform.TransformDirection(Vector3.forward);         //we check the lookAt direction
            if (Vector3.Dot(lookAtPosition, Vector3.up) > 0f) {                                     //if player looks up
                Camera.main.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);    //we clamp it up
            } else {
                Camera.main.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.right);     //else we clamp it down
            }
        } else {
            Camera.main.transform.rotation *= Quaternion.Euler(mouseDelta.y, 0f, 0f);               //if not, we just add the rotation as usual
        }
    }

    Vector2 GetMouseInputAsPositionDelta()
    {
        var result = new Vector2((Input.mousePosition - previousMousePosition).x, (Input.mousePosition - previousMousePosition).y * (invertY ? 1 : -1));
        if (Mathf.Abs(Input.mousePosition.x - Screen.width / 2) > Screen.width / 4 || 
            Mathf.Abs(Input.mousePosition.y - Screen.height / 2) > Screen.height / 4)   //if cursor gets far off
            SetCursorPos(Screen.width / 2, Screen.height / 2);  //reset it
        previousMousePosition = Input.mousePosition;
        return result;
    }
}
