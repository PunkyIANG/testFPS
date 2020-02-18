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
    public bool useGravity = true;
    public float gravity = 20.0f;
    public float speed = 10f; //deprecated
    public float moveSpeed = 7.0f;                // Ground move speed
    public float runAcceleration = 14.0f;         // Ground accel
    public float runDeceleration = 100f;
    public float airAcceleration = 2.0f;          // Air accel
    public float airDeceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
    public float airControl = 0.3f;               // How precise air control is
    public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
    public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
    public float minSpeed = 1f;
    public float friction = 6; //Ground friction

    public Vector2 mouseSensitivity = new Vector2(10f,20f);
    public float maxCameraXAngle = 90f;
    public bool invertY = false;
    public float jumpSpeed = 8.0f;                // The speed at which the character's up axis gains when hitting jump
    public bool holdJumpToBhop = false;
    private bool wishJump;
    private Vector3 abstractInput;
    private Vector3 playerVelocity = Vector3.zero;
    private Vector3 startPosition;

    bool rotated = false;
    Camera mainCam;
    Vector3 previousMousePosition;
    Quaternion lateRotation;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        Cursor.visible = false;
        mouse = Mouse.current;
        keyboard = Keyboard.current;
        mainCam = GetComponentInChildren<Camera>();
        QualitySettings.maxQueuedFrames = 0;
        startPosition = transform.position;
    }

    // void Update() {     //we're handling input in update for better gameplay feel
    //     //CharMovement();
    //     var wishDir = new Vector3 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Jump"), Input.GetAxisRaw("Vertical"));
    //     PM_Accelerate(wishDir, speed, 1f);
    //     CameraRotation();
    //     if (rotated)
    //         RotateCamera(lateRotation);

    // }


    void Update() {
        abstractInput = TestMovement();

        CameraRotation();
        if (rotated)
            RotateCamera(lateRotation); //pls refactor this shit
        
        QueueJump();

        if(IsGrounded()) 
            GroundMove();
        else
            AirMove();

        if (playerVelocity.y != 0f)
            print(playerVelocity.y);
        rigidbody.MovePosition(rigidbody.position + playerVelocity * Time.deltaTime);

    }

    private void AirMove() {
        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;
        
        wishdir = transform.TransformDirection(abstractInput);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDeceleration;
        else
            accel = airAcceleration;
        // If the player is ONLY strafing left or right
        if(abstractInput.z == 0 && abstractInput.x != 0) 
        {
            if(wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if(airControl > 0)
            AirControl(wishdir, wishspeed2);
        // !CPM: Aircontrol

        // Apply gravity
        if (useGravity)
            playerVelocity.y -= gravity * Time.deltaTime;
    }

        private void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(abstractInput.z) < 0.001 || Mathf.Abs(wishspeed) < 0.001)//(Mathf.Abs(_cmd.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;

        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }


    private void GroundMove() {

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        
        var wishDir = transform.TransformDirection(abstractInput);
        wishDir.Normalize();

        // if (wishDir != Vector3.zero)
        //     print("Normalized wish vector: " + wishDir);

        var wishSpeed = wishDir.magnitude;
        wishSpeed *= moveSpeed;

        Accelerate(wishDir, wishSpeed, runAcceleration);

        // Reset the gravity velocity
        //if (useGravity)
        //    playerVelocity.y = -gravity * Time.deltaTime;

        if(wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }


    private void ApplyFriction(float t) {
        Vector3 vec = playerVelocity; 
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if(IsGrounded())
        {
            control = speed < runDeceleration ? runDeceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;

        if(newspeed < 0)
            newspeed = 0;
        if(speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if(addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if(accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }


    bool IsGrounded() {
        float DistanceToTheGround = GetComponent<Collider>().bounds.extents.y;
        return Physics.Raycast(transform.position, Vector3.down, DistanceToTheGround + 0.1f);
    }

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
    private void QueueJump() {
        if(holdJumpToBhop)
        {
            wishJump = Input.GetButton("Jump");
            return;
        }

        if (Input.GetButtonDown("Jump") && !wishJump)
            wishJump = true;
        
        if (Input.GetButtonUp("Jump"))
            wishJump = false;
    }


    Vector2 GetMouseInputAsPositionDelta()
    {
        var result = new Vector2((Input.mousePosition - previousMousePosition).x, (Input.mousePosition - previousMousePosition).y * (invertY ? 1 : -1));
        previousMousePosition = Input.mousePosition;
        return result;
    }

    Vector2 GetMouseInputAsAPIDelta() {
        var result = mouse.delta.ReadUnprocessedValue();
        
        // if (result != Vector2.zero)
        //     print(result);

        if (!invertY)
            result.y *= -1;

        return result;
    }

    void RotateCamera(Quaternion rotation) {
        mainCam.transform.localRotation = rotation;
    }

    public void GetMovementEvent(InputAction.CallbackContext context) {
        var deviceInput = context.ReadValue<Vector2>();
        abstractInput = new Vector3(deviceInput.x, 0f, deviceInput.y);
    }

    Vector3 TestMovement() {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
    }

    public void ResetPlayer (InputAction.CallbackContext context)
    {
        transform.position = startPosition;
        print("reset");
    }

}
