using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwingTPSController : MonoBehaviour
{
    #region Variables

    // Movement speeds
    public float RunSpeed = 5;
    public float SprintingSpeed = 10;
    public float RotationSpeed = 15;

    // Tracks how long the character has been in the air
    private float _inAirTimer;

    // Components
    public Animator Animator;
    public LineRenderer LineRenderer;
    public BuildingManager ClimbingObject;
    public GameObject Hand;

    // Physics related variables
    private Rigidbody _rigidbody;
    private SpringJoint _joint;
    private RaycastHit _hit;

    // Direction of movement
    private Vector3 _moveDirection;

    // Bools to track the character's state
    private bool _isMoving,
        _isClimbing,
        _isFalling,
        _isSwinging,
        _isGrounded = true;

    #endregion

    #region Unity LifeCycle

    // Start is called before the first frame update
    void Start()
    {
        // Get the Rigidbody component
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the character is grounded
        CheckGroundPass();

        // Handle what happens when the character is falling
        HandleFalling();

        // Handle character movement
        HandleMovement();

        // Handle character rotation
        HandleRotation();

        // Handle shooting the web
        HandleShootingWeb();

        // Handle releasing the web
        HandleReleasingWeb();
    }

    // LateUpdate is called after all other Update functions have finished
    void LateUpdate()
    {
        // Draw the web if the character is swinging
        DrawWeb();
    }

    #endregion

    #region Collision Detection

    // Called when the character collides with another object
    void OnCollisionEnter(Collision collision)
    {
        // Get a reference to the object the character collided with
        var colObject = collision.collider.gameObject;

        // Check if the character is climbing and collided with a climbable object
        if (!_isFalling && collision.collider.CompareTag("Climbable"))
        {
            // If the character isn't already climbing an object, set the ClimbingObject script reference
            if (ClimbingObject == null)
            {
                ClimbingObject = colObject.GetComponent<BuildingManager>();
            }

            // Start climbing and set bools to track climbing state
            _isClimbing = true;
            _isGrounded = false;
            StopSwinging();

            // Play the climbing sound effect
            if (!SoundManager.instance.ClimbingAudioSource.isPlaying)
            {
                SoundManager.instance.ClimbingAudioSource.pitch = Random.Range(0.95f, 1.05f);
                SoundManager.instance.ClimbingAudioSource.Play();
            }
        }

        // Check if the character is in the air and collided with a walkable object
        if (!_isGrounded && collision.collider.CompareTag("Walkable"))
        {
            // Stop falling, climbing, and swinging
            _isFalling = false;
            _isClimbing = false;
            StopSwinging();

            // Play the landing sound effect or hard landing sound effect depending on how long the character has been in the air
            if (
                _inAirTimer > 2.5f
                && Animator.GetInteger("State") == (int)SwingAnimationState.Falling
            )
            {
                SoundManager.instance.FootAudioSource.Play();
                Animator.SetInteger("State", (int)SwingAnimationState.HardLanding);
                Invoke("Grounded", 0.5f);
            }
            else
            {
                _isGrounded = true;
            }

            // Reset the in air timer
            _inAirTimer = 0;
        }
    }

    // Called when the character exits a collision with another object
    void OnCollisionExit(Collision collision)
    {
        // Check if the character was grounded and is now no longer grounded
        if (_isGrounded && collision.collider.CompareTag("Walkable") && transform.position.y > 1)
        {
            _isGrounded = false;
        }
    }

    #endregion

    #region Climb

    // Called to set the character back to grounded state
    private void Grounded()
    {
        _isGrounded = true;
    }

    // Called to stop climbing
    private void StopClimbing()
    {
        SoundManager.instance.ClimbingAudioSource.Stop();
        _rigidbody.AddForce(-transform.forward * 1000);
        Animator.SetInteger("State", (int)SwingAnimationState.ClimbJump);
        _isClimbing = false;
        ClimbingObject = null;
        _isFalling = true;
    }

    // Check if the character has fallen below the level boundary
    private void CheckGroundPass()
    {
        if (transform.position.y < -2)
        {
            SceneManager.LoadScene(0);
        }
    }

    #endregion

    #region Swing

    // Handle shooting the web
    private void HandleShootingWeb()
    {
        // Cannot shoot web while climbing
        if (_isClimbing)
        {
            return;
        }

        // If already swinging, update animation based on swing direction
        if (_isSwinging)
        {
            if (transform.position.y < _hit.point.y && transform.position.x < _hit.point.x)
            {
                Animator.SetInteger("State", (int)SwingAnimationState.SwingingBothArms);
            }
            else
            {
                Animator.SetInteger("State", (int)SwingAnimationState.Swinging);
            }
            return;
        }

        // If left mouse button is pressed and not already swinging
        if (Input.GetMouseButton(0) && _joint == null)
        {
            // Cast a ray from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // If the ray hits something
            if (Physics.Raycast(ray, out hit, 75))
            {
                // Calculate distance to the hit point
                float distanceFromPoint = Vector3.Distance(transform.position, hit.point);

                // Ensure the hit point is above the character and has a minimum height
                if (hit.point.y > 3 && hit.point.y > transform.position.y)
                {
                    // Store the hit point
                    _hit = hit;
                    _hit.point += Vector3.up * 1.5f;
                    Animator.SetInteger("State", (int)SwingAnimationState.Swinging);

                    // Look towards the swing point
                    Vector3 targetPostition = new Vector3(
                        _hit.point.x,
                        transform.position.y,
                        _hit.point.z
                    );
                    transform.LookAt(targetPostition);

                    // Start swinging and update state bools
                    _isSwinging = true;
                    _isGrounded = false;
                    _isFalling = false;
                    _isClimbing = false;

                    // Create a SpringJoint to simulate the swing
                    _joint = gameObject.AddComponent<SpringJoint>();
                    _joint.autoConfigureConnectedAnchor = false;
                    _joint.connectedAnchor = _hit.point;

                    // Set SpringJoint properties
                    _joint.maxDistance = distanceFromPoint * 0.7f;
                    _joint.minDistance = distanceFromPoint * 0.1f;
                    _joint.spring = 35f;
                    _joint.damper = 20f;

                    // Play the web shooting sound effect
                    SoundManager.instance.WebAudioSource.pitch = Random.Range(0.85f, 1.15f);
                    SoundManager.instance.WebAudioSource.Play();

                    // Schedule a release of the web after 2 seconds
                    CancelInvoke("ReleaseWeb");
                    Invoke("ReleaseWeb", 2f);
                }
            }
        }
    }

    // Handle releasing the web
    private void HandleReleasingWeb()
    {
        // Release the web if the left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseWeb();
        }
    }

    // Release the web
    private void ReleaseWeb()
    {
        if (!_isSwinging)
        {
            return;
        }

        // If the left mouse button is still held down, don't release the web
        if (Input.GetMouseButton(0)) { }
        else
        {
            Animator.SetInteger("State", (int)SwingAnimationState.Falling);
        }
        StopSwinging();
    }

    // Stop swinging and destroy the SpringJoint
    private void StopSwinging()
    {
        _isSwinging = false;
        Destroy(_joint);
    }

    // Draw the web if the character is swinging
    private void DrawWeb()
    {
        if (_isSwinging)
        {
            LineRenderer.positionCount = 2;
            LineRenderer.SetPosition(0, Hand.transform.position + new Vector3(0, 0.15f, 0));
            LineRenderer.SetPosition(1, _hit.point);
        }
        else
        {
            LineRenderer.positionCount = 0;
        }
    }

    #endregion

    #region Locomotion
    private void HandleFalling()
    {
        // Check if the character has fallen outside the level boundaries
        if (
            _isGrounded
            && (
                transform.position.z < -24.6f
                || transform.position.z > 126f
                || transform.position.x < -18
                || transform.position.x > 107
            )
        )
        {
            _isGrounded = false;
        }

        // If not grounded, not climbing, and not swinging, handle falling physics
        if (!_isGrounded && !_isClimbing && !_isSwinging)
        {
            // Increase the in-air timer
            _inAirTimer += Time.deltaTime * 2.5f;

            // Apply downward force to simulate gravity
            _rigidbody.AddForce(-Vector3.up * 125f * _inAirTimer);

            // Check if the character is far enough from the ground and should transition to the falling animation
            Physics.Raycast(transform.position, -transform.up, out RaycastHit hit);
            if (
                _inAirTimer > (_isFalling ? 3 : 1.5f)
                && hit.distance > 5
                && hit.collider.CompareTag("Walkable")
            )
            {
                Animator.SetInteger("State", (int)SwingAnimationState.Falling);
            }
        }
    }

    private void HandleMovement()
    {
        // Stop running sound if not moving or not grounded
        if ((!_isMoving || !_isGrounded) && SoundManager.instance.RunningAudioSource.isPlaying)
        {
            SoundManager.instance.RunningAudioSource.Stop();
        }

        // Stop climbing sound if not climbing
        if (!_isClimbing && SoundManager.instance.ClimbingAudioSource.isPlaying)
        {
            SoundManager.instance.ClimbingAudioSource.Stop();
        }

        // If falling, stop all movement
        if (_isFalling)
        {
            _rigidbody.velocity = Vector3.zero;
            return;
        }

        // If swinging, apply swing forces
        if (_isSwinging)
        {
            _rigidbody.velocity +=
                (
                    transform.forward
                    + transform.up * (_hit.point.y > transform.position.y + 5 ? 1.75f : 0) // Add upward force if swinging high
                )
                * 10f
                * Time.deltaTime;
            return;
        }

        // If climbing and not grounded, check if reached the top of the climbable object
        if (!_isGrounded && _isClimbing)
        {
            if (
                ClimbingObject != null
                && transform.position.y
                    > ClimbingObject.GetComponent<MeshRenderer>().bounds.max.y
                        * ClimbingObject.HeightPercent
            )
            {
                SoundManager.instance.ClimbingAudioSource.Stop();
                _isClimbing = false;
                ClimbingObject = null;
                transform.position += Vector3.up * 2.25f + transform.forward / 1.5f; // Slight forward movement after climbing
                SoundManager.instance.FootAudioSource.Play();
                Animator.SetInteger("State", (int)SwingAnimationState.HardLanding);
            }
        }

        // Calculate movement direction based on input and climbing state
        if (_isClimbing)
        {
            _moveDirection = transform.up * Input.GetAxisRaw("Vertical");
            _moveDirection += transform.right * Input.GetAxisRaw("Horizontal");
            _moveDirection.Normalize();
            _moveDirection *= 2f;
        }
        else
        {
            _moveDirection = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
            _moveDirection += Camera.main.transform.right * Input.GetAxisRaw("Horizontal");
            _moveDirection.Normalize();
            _moveDirection.y = 0;
        }

        // Handle jumping input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJumping();
        }

        // Determine if the character is moving
        _isMoving = Mathf.Abs(_moveDirection.magnitude) > 0;

        // Determine if sprinting
        bool isSprinting = false;
        if (_isMoving)
        {
            if (Input.GetKey(KeyCode.LeftShift) && !_isClimbing)
            {
                if (_isGrounded)
                {
                    Animator.SetInteger("State", (int)SwingAnimationState.Sprint);
                }
                _moveDirection *= SprintingSpeed;
                isSprinting = true;
            }
            else
            {
                if (_isGrounded)
                {
                    Animator.SetInteger("State", (int)SwingAnimationState.Run);
                }
                _moveDirection *= RunSpeed;
            }
        }
        else if (_isGrounded)
        {
            Animator.SetInteger("State", (int)SwingAnimationState.Idle);
        }

        // Play running sound if grounded and moving
        if (_isGrounded && _isMoving)
        {
            SoundManager.instance.RunningAudioSource.pitch = isSprinting ? 1.5f : 1;
            if (!SoundManager.instance.RunningAudioSource.isPlaying)
            {
                SoundManager.instance.RunningAudioSource.Play();
            }
        }

        // Handle climbing movement
        if (_isClimbing && !_isGrounded)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
            {
                if (hit.collider.gameObject.name == ClimbingObject.name && hit.distance > 0.5f)
                {
                    _moveDirection += transform.forward;
                }
                else if (hit.distance > 2)
                {
                    StopClimbing();
                }
            }
            if (_isMoving)
            {
                if (!SoundManager.instance.ClimbingAudioSource.isPlaying)
                {
                    SoundManager.instance.ClimbingAudioSource.pitch = Random.Range(0.95f, 1.05f);
                    SoundManager.instance.ClimbingAudioSource.Play();
                }
                _moveDirection /= 5;
                Animator.SetInteger("State", (int)SwingAnimationState.Climbing);
            }
            else
            {
                SoundManager.instance.ClimbingAudioSource.Stop();
                Animator.SetInteger("State", (int)SwingAnimationState.ClimbingIdle);
            }
        }

        // Apply calculated velocity to the Rigidbody
        _rigidbody.velocity = _moveDirection;
    }

    private void HandleRotation()
    {
        if (_isClimbing)
        {
            if (ClimbingObject != null)
            {
                Vector3 targetPostition = new Vector3(
                    ClimbingObject.transform.position.x,
                    transform.position.y,
                    ClimbingObject.transform.position.z
                );
                transform.LookAt(targetPostition);
            }
            return;
        }

        if (_isFalling)
        {
            return;
        }

        var targetDirection = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
        targetDirection += Camera.main.transform.right * Input.GetAxisRaw("Horizontal");
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            RotationSpeed * Time.deltaTime
        );

        transform.rotation = playerRotation;
    }

    private void HandleJumping()
    {
        if (_isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * 800);
            Animator.SetInteger(
                "State",
                _isMoving ? (int)SwingAnimationState.RunningJump : (int)SwingAnimationState.Jump
            );
            _isGrounded = false;
        }
        else if (_isClimbing)
        {
            StopClimbing();
        }
    }

    #endregion

    #region  Display Controls
    void OnGUI()
    {
        // Set GUI style
        GUIStyle guiStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperLeft,
        };

        // Define the control instructions
        string controlsText =
            "Swing and Locomotion Controls:\n\n"
            + "Movement:\n"
            + "- W/A/S/D or Arrow Keys: Move\n"
            + "- Left Shift: Sprint\n\n"
            + "Swing:\n"
            + "- Left Mouse Button: Shoot web\n"
            + "- Release Left Mouse Button: Release web\n\n"
            + "Jumping:\n"
            + "- Spacebar: Jump\n\n"
            + "Climbing:\n"
            + "- W/A/S/D or Arrow Keys: Climb\n"
            + "- Spacebar: Stop climbing\n\n"
            + "Camera:\n"
            + "- Mouse Movement: Rotate camera";

        // Draw the control instructions panel
        GUI.Box(new Rect(10, 10, 300, 250), controlsText, guiStyle);
    }

    #endregion

    #region DrawGizmos
    private void OnDrawGizmos()
    {
        if (_isClimbing)
        {
            // Draw a green gizmo sphere at the character's hand position to indicate the web attachment point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Hand.transform.position, 0.2f);

            // Draw a red gizmo line from the hand position to the climbable object's top if in range
            if (ClimbingObject != null)
            {
                float climbableTopY =
                    ClimbingObject.GetComponent<MeshRenderer>().bounds.max.y
                    * ClimbingObject.HeightPercent;
                if (Mathf.Abs(transform.position.y - climbableTopY) < 2)
                {
                    Gizmos.DrawLine(
                        Hand.transform.position,
                        new Vector3(
                            Hand.transform.position.x,
                            climbableTopY,
                            Hand.transform.position.z
                        )
                    );
                }
            }
        }

        if (_isSwinging)
        {
            // Draw a yellow gizmo line from the hand position to the swing point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(Hand.transform.position, _hit.point);
        }

        // Draw a blue gizmo raycast from the character's position downwards to visualize the ground check
        if (!_isGrounded)
        {
            Gizmos.color = Color.blue;
            Ray groundRay = new Ray(transform.position, -transform.up);
            Gizmos.DrawLine(groundRay.origin, groundRay.origin + groundRay.direction * 2);
        }
    }
    #endregion
}

#region SwingAnimationState
public enum SwingAnimationState
{
    Idle = 0,
    Run = 1,
    Sprint = 2,
    Jump = 3,
    RunningJump = 4,
    Climbing = 5,
    ClimbingIdle = 6,
    ClimbJump = 7,
    HardLanding = 8,
    Falling = 9,
    Swinging = 10,
    SwingingBothArms = 11,
    Death = 12,
}
#endregion
