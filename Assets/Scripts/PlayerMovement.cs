using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody))]

public class PlayerMovement : MonoBehaviour
{
    private Mouse mouse;
    private Keyboard keyboard;
    new Rigidbody rigidbody;
    public float speed = 10f;
    public Vector2 mouseSensitivity = new Vector2(10f,20f);
    public float maxCameraXAngle = 90f;
    public bool invertY = false;

    bool rotated = false;
    Camera mainCam;
    Vector3 previousMousePosition;
    Quaternion lateRotation;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.visible = false;
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        mainCam = GetComponentInChildren<Camera>();
        QualitySettings.maxQueuedFrames = 0;
    }

    void Update() {     //we're handling input in update for better gameplay feel
        //CharMovement();
        var wishDir = new Vector3 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical"));
        PM_Accelerate(wishDir, speed, 1f);
        CameraRotation();
        if (rotated)
            RotateCamera(lateRotation);

    }

    

/*     void LateUpdate() {
    }
 */

    void CharMovement() {
        var moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));  //getting the input
        moveDirection = transform.TransformDirection(moveDirection);                                        //localspace -> worldspace
        rigidbody.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);                //applying movement
    }

    void CameraRotation() {
        if (GetMouseInputAsAPIDelta() != Vector2.zero) {
            var mouseDelta = Vector2.Scale(GetMouseInputAsAPIDelta(), mouseSensitivity);

            rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0f, mouseDelta.x, 0f));
            
            //clamp it

            var rawCameraRotation = mainCam.transform.rotation * Quaternion.Euler(mouseDelta.y, 0f, 0f);    //getting the unclamped rotation

            if (Quaternion.Angle(rigidbody.rotation, rawCameraRotation) > maxCameraXAngle)  {           //if the rotation x angle is too big
                var lookAtPosition = mainCam.transform.TransformDirection(Vector3.forward);         //we check the lookAt direction
                if (Vector3.Dot(lookAtPosition, Vector3.up) > 0f) {                                     //if player looks up
                    lateRotation = Quaternion.AngleAxis(-maxCameraXAngle, Vector3.right);    //we clamp it up
                } else {
                    lateRotation = Quaternion.AngleAxis(maxCameraXAngle, Vector3.right);     //else we clamp it down
                }
            } else {
                lateRotation = mainCam.transform.localRotation * Quaternion.Euler(mouseDelta.y, 0f, 0f);               //if not, we just add the rotation as usual
            }

            rotated = true;
        } else {
            rotated = false;
        }
        
    }


    Vector2 GetMouseInputAsPositionDelta()
    {
        var result = new Vector2((Input.mousePosition - previousMousePosition).x, (Input.mousePosition - previousMousePosition).y * (invertY ? 1 : -1));
        previousMousePosition = Input.mousePosition;
        return result;
    }

    Vector2 GetMouseInputAsAPIDelta() {
        var result = mouse.delta.ReadUnprocessedValue();

        if (result != Vector2.zero)
            print(result);

        if (!invertY)
            result.y *= -1;

        return result;
    }

    void RotateCamera(Quaternion rotation) {
        mainCam.transform.localRotation = rotation;
    }

    void PM_Accelerate(Vector3 wishDir, float wishSpeed, float accel) {
        int	i;
        float addspeed, accelspeed, currentspeed;

        currentspeed = Vector3.Dot(rigidbody.velocity, wishDir);
        addspeed = wishSpeed - currentspeed;
        if (addspeed <= 0) {
            return;
        }

        accelspeed = accel * Time.deltaTime * wishSpeed;
        if (accelspeed > addspeed) {
            accelspeed = addspeed;
        }

        wishDir = transform.TransformDirection(wishDir);
        rigidbody.MovePosition(transform.position + accelspeed * wishDir);        
    }
}
