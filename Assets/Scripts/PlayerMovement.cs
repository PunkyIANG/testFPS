using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMovement : MonoBehaviour
{
    private Mouse mouse;
    new Rigidbody rigidbody;
    public float speed = 10f;
    public float mouseSensitivity = 10f;
    public float maxCameraXAngle = 90f;
    public bool invertY = false;

    Camera mainCam;
    Vector3 previousMousePosition;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.visible = false;
        mouse = Mouse.current;
        mainCam = GetComponentInChildren<Camera>();
        mouse.delta.y.invert = !invertY;
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
        var mouseDelta = GetMouseInputAsAPIDelta() * mouseSensitivity;

        rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0f, mouseDelta.x, 0f));
        
        //clamp it

        var rawCameraRotation = mainCam.transform.rotation * Quaternion.Euler(mouseDelta.y, 0f, 0f);    //getting the unclamped rotation

        if (Quaternion.Angle(rigidbody.rotation, rawCameraRotation) > maxCameraXAngle)  {           //if the rotation x angle is too big
            var lookAtPosition = mainCam.transform.TransformDirection(Vector3.forward);         //we check the lookAt direction
            if (Vector3.Dot(lookAtPosition, Vector3.up) > 0f) {                                     //if player looks up
                mainCam.transform.localRotation = Quaternion.AngleAxis(-90f, Vector3.right);    //we clamp it up
            } else {
                mainCam.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.right);     //else we clamp it down
            }
        } else {
            mainCam.transform.rotation *= Quaternion.Euler(mouseDelta.y, 0f, 0f);               //if not, we just add the rotation as usual
        }
    }

    Vector2 GetMouseInputAsPositionDelta()
    {
        var result = new Vector2((Input.mousePosition - previousMousePosition).x, (Input.mousePosition - previousMousePosition).y * (invertY ? 1 : -1));
        previousMousePosition = Input.mousePosition;
        return result;
    }

    Vector2 GetMouseInputAsAPIDelta() {
        return mouse.delta.ReadUnprocessedValue();
    }
}
