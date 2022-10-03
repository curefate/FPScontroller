using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerSoundPlayer))]
[RequireComponent(typeof(CameraMouseLook))]

public class ControlManager : MonoBehaviour
{
    [Header("PlayerFrozen")]
    [SerializeField] private bool playerFrozen = false;

    [Header("Walk")]
    public float speed_Walk = 4f;
    public float backSpeedFactor = 0.9f;

    [Header("Run")]
    [SerializeField] private bool enabled_FovChange = true;
    [SerializeField] private float runningFOV = 65f;
    [SerializeField] private bool runAsToggle = true;
    public float speed_Run = 10f;

    [Header("Crouch")]
    [SerializeReference] private bool crouchAsToggle = false;
    public float speed_Crouch = 2f;

    [Header("Jump")]
    public float jumpForce = 6f;
    public bool enabled_secJump = true;
    private bool secJumped = false;

    [Header("Air Moving(Fall)")]
    public float speed_AirMoving = 1.5f;
    public float gravityMultiplier = 1f;

    [Header("Slide")]
    [SerializeField] private bool ifCurveByCode = true;
    public AnimationCurve slideSpeedCurve;
    public float maxSlideSpeedEnergy = 1000f;
    public float lowSlideSpeedEnergy = 750f;
    public float slideEnergyIncreseStep = 1f;
    public float slideEnergyConsumeStep = 2.5f;
    [HideInInspector] public float slideEnergy = 750f;
    [HideInInspector] public float lastSlideOverTime = -5f;
    [SerializeField] private float slideIntoCD = 0.5f;

    [Header("Wall Run")]
    public float minimumHeight = 1f;
    public float minimumFallDuration = 0.25f;
    public float maximumFallDuration = 1f;
    public float maximumWallDistance = 0.3f;
    public float gravityOnWall = 0.05f;
    public float cameraTilt = 3f;
    [HideInInspector] public RaycastHit wallHitInfo;

    [Header("Animator")]
    public Animator animator;
    private int anim_state_id;
    private int anim_input_x_id;
    private int anim_input_y_id;

    [Header("CameraBob")]
    public Vector3 defaultCameraPos = new Vector3(0, 0.7f, 0.22f);
    public Vector3 defaultModelPos = new Vector3(0, -1.05f, 0);
    public Transform cameraOffset;
    public Transform fallCamPos;
    public Transform landCamPos;
    public Transform crouchCamPos;
    public Transform runCamPos;
    public Transform slideCamPos;
    public Transform modelTransform;
    private Vector3 crouchModelpos = new Vector3(0, -0.545f, 0);
    private bool pullCamreaBack = true;
    private float zTilt = 0f;
    private bool pullTiltZero = true;

    // move info
    [HideInInspector] public float currentSpeed = 0f;
    [HideInInspector] public Vector2 m_Input = Vector2.zero;
    [HideInInspector] public Vector3 moveDir = Vector3.zero;
    [HideInInspector] public Vector3 desireMove = Vector3.zero;
    [HideInInspector] public Vector3 lastDesireMove = Vector3.zero;
    [HideInInspector] public float stickToGroundForce = 10f;

    // For audio
    private bool movementStarted = false;
    private float cumulativeStepCycleCount = 0.0f;
    private float nextStepInCycle = 0.0f;
    private float stepInterval = 5.0f;
    private float length_Walk = 1.0f;
    private float length_Run = 0.7f;
    private float length_Crouch = 3.0f;

    // height info
    private float crouchDivisor = 2f;
    private float standingHeight = 0f;
    private float crouchHeight = 0f;
    private float previousCharacterHeight;

    [HideInInspector] public ePlayerMotionStates statePointer;
    [HideInInspector] public ePlayerMotionStates lastStatePointer;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public PlayerSoundPlayer soundPlayer;
    private Camera camera;
    private StateMachine stateMachine;

    public enum ePlayerMotionStates
    {
        NONE = -1,
        IDLE,
        WALK,
        RUN,
        CROUCH,
        JUMP,
        FALL,
        SLIDE,
        WALLRUN,
        WALLJUMP
    }

    void Awake()
    {
        camera = Camera.main;
        gameObject.GetComponent<CameraMouseLook>().Init(transform, camera.transform);
        controller = gameObject.GetComponent<CharacterController>();
        soundPlayer = GetComponent<PlayerSoundPlayer>();

        if (ifCurveByCode)
        {
            Keyframe[] kf = new Keyframe[3];
            kf[0] = new Keyframe(0, 0);
            kf[1] = new Keyframe(750, 8);
            kf[2] = new Keyframe(1000, 12);
            slideSpeedCurve = new AnimationCurve(kf);
            slideSpeedCurve.SmoothTangents(1, -3);
            slideSpeedCurve.preWrapMode = WrapMode.Default;
            slideSpeedCurve.postWrapMode = WrapMode.Default;
        }

        PlayerMotionStates.IdleState idleState = new PlayerMotionStates.IdleState(0, this);
        PlayerMotionStates.WalkState walkState = new PlayerMotionStates.WalkState(1, this);
        PlayerMotionStates.RunState runState = new PlayerMotionStates.RunState(2, this);
        PlayerMotionStates.CrouchState crouchState = new PlayerMotionStates.CrouchState(3, this);
        PlayerMotionStates.JumpState jumpState = new PlayerMotionStates.JumpState(4, this);
        PlayerMotionStates.FallState fallState = new PlayerMotionStates.FallState(5, this);
        PlayerMotionStates.SlideState slideState = new PlayerMotionStates.SlideState(6, this);
        PlayerMotionStates.WallRunState wallRunState = new PlayerMotionStates.WallRunState(7, this);
        PlayerMotionStates.WallJumpState wallJumpState = new PlayerMotionStates.WallJumpState(8, this);

        stateMachine = new StateMachine(idleState);
        stateMachine.AddState(walkState);
        stateMachine.AddState(runState);
        stateMachine.AddState(crouchState);
        stateMachine.AddState(jumpState);
        stateMachine.AddState(fallState);
        stateMachine.AddState(slideState);
        stateMachine.AddState(wallRunState);
        stateMachine.AddState(wallJumpState);
    }

    void Start()
    {
        statePointer = ePlayerMotionStates.IDLE;
        lastStatePointer = ePlayerMotionStates.NONE;

        standingHeight = controller.height;
        crouchHeight = standingHeight / crouchDivisor;

        cumulativeStepCycleCount = 0f;
        nextStepInCycle = cumulativeStepCycleCount / 2f;

        anim_state_id = Animator.StringToHash("state");
        anim_input_x_id = Animator.StringToHash("input_x");
        anim_input_y_id = Animator.StringToHash("input_y");
    }

    void Update()
    {
        if (playerFrozen)
            return;

        StateJudge();

        // Footstep audio special case: If player moves a little, but not a "full stride", there should still be a foot step sound. And if they just stopped walking, there should also be one
        if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_HORIZONTAL) || InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_VERTICAL) && !movementStarted)
        {
            movementStarted = true;
            cumulativeStepCycleCount = 0f;
            nextStepInCycle = cumulativeStepCycleCount + stepInterval;
            if (controller.isGrounded)
            {
                soundPlayer.PlayFootstepSounds();
            }
        }
        if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_HORIZONTAL) || InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_VERTICAL) && movementStarted)
        {
            movementStarted = false;
            if (controller.isGrounded)
            {
                soundPlayer.PlayFootstepSounds();
            }
        }

        //update
        previousCharacterHeight = controller.height;
        if (controller.isGrounded || statePointer == ePlayerMotionStates.WALLRUN)
        {
            secJumped = false;
        }
        if (pullCamreaBack)
        {
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, defaultCameraPos, 0.05f);
        }
        if (pullTiltZero)
        {
            zTilt = Mathf.Lerp(zTilt, 0f, 0.2f);
        }
    }

    void FixedUpdate()
    {
        GetComponent<CameraMouseLook>().LookRotation(transform, camera.transform);
        camera.transform.localRotation *= Quaternion.Euler(0f, 0f, zTilt);

        if (playerFrozen)
            return;

        if (statePointer != ePlayerMotionStates.SLIDE && slideEnergy < maxSlideSpeedEnergy)
        {
            slideEnergy += slideEnergyIncreseStep;
        }

        desireMove = Vector3.zero;
        m_Input = CalcInput();
        moveDir = CalcMoveDirection(m_Input);

        StateUpdate();
        stateMachine.StateOnStay();

        controller.Move(desireMove * Time.fixedDeltaTime);

            lastDesireMove = desireMove;

        UpdateHeight();
        UpdateCameraFov();
        ProgressStepCycle();
        //Debug.Log(stateMachine.GetCurrentStateID());
        //Debug.Log(currentSpeed);
    }

    private Vector2 CalcInput()
    {
        float horizontal = InputManager.Instance.GetAxis(InputManager.eInputState.INPUT_HORIZONTAL);
        float vertical = InputManager.Instance.GetAxis(InputManager.eInputState.INPUT_VERTICAL);
        Vector2 input = new Vector2(horizontal, vertical);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }
        return input;
    }
    private Vector3 CalcMoveDirection(Vector2 input)
    {
        Vector3 dir = transform.forward * input.y + transform.right * input.x;
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, controller.radius, Vector3.down, out hitInfo, controller.height / 2f);
        dir = Vector3.ProjectOnPlane(dir, hitInfo.normal).normalized;
        return dir;
    }

    private void UpdateCameraFov()
    {
        if (enabled_FovChange)
        {
            if (currentSpeed > (speed_Run + speed_Walk) / 2 && (statePointer == ePlayerMotionStates.RUN || statePointer == ePlayerMotionStates.SLIDE || statePointer == ePlayerMotionStates.WALLRUN))
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, runningFOV, 0.1f);
            }
            else
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 60f, 0.1f);
            }
        }
    }
    private void UpdateHeight()
    {
        if (statePointer == ePlayerMotionStates.CROUCH || statePointer == ePlayerMotionStates.SLIDE)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, 0.4f);
            //modelTransform.localPosition = Vector3.Lerp(modelTransform.localPosition, crouchModelpos, 0.5f);
            modelTransform.localPosition = crouchModelpos;
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standingHeight, 0.4f);
            //modelTransform.localPosition = Vector3.Lerp(modelTransform.localPosition, defaultModelPos, 0.5f);
            modelTransform.localPosition = defaultModelPos;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y + (controller.height - previousCharacterHeight) / 2, transform.position.z);
    }
    private void ProgressStepCycle()
    {
        if (controller.velocity.sqrMagnitude > 0f && (m_Input.x != 0f || m_Input.y != 0f))
        {
            if (statePointer == ePlayerMotionStates.CROUCH)
            {
                cumulativeStepCycleCount += (controller.velocity.magnitude + currentSpeed * length_Crouch) * Time.fixedDeltaTime;
            }
            else if (statePointer == ePlayerMotionStates.RUN || statePointer == ePlayerMotionStates.WALLRUN)
            {
                cumulativeStepCycleCount += (controller.velocity.magnitude + currentSpeed * length_Run) * Time.fixedDeltaTime;
            }
            else if (statePointer == ePlayerMotionStates.WALK)
            {
                cumulativeStepCycleCount += (controller.velocity.magnitude + currentSpeed * length_Walk) * Time.fixedDeltaTime;
            }
        }
        if (!(cumulativeStepCycleCount > nextStepInCycle))
        {
            return;
        }
        nextStepInCycle = cumulativeStepCycleCount + stepInterval;
        if ((controller.isGrounded || statePointer == ePlayerMotionStates.WALLRUN) && statePointer != ePlayerMotionStates.SLIDE)
        {
            soundPlayer.PlayFootstepSounds();
        }
    }
    private bool haveHeadRoomToStand()
    {
        bool haveHeadRoom = true;

        RaycastHit headRoomHit;
        if (Physics.Raycast(gameObject.transform.position, gameObject.transform.up, out headRoomHit, (standingHeight - (controller.height / 2.0f))))
        {
            haveHeadRoom = false;
        }

        return haveHeadRoom;
    }

    private void StateUpdate()
    {
        switch (statePointer)
        {
            case ePlayerMotionStates.IDLE:
                stateMachine.TranslateState(0);
                animator.SetInteger(anim_state_id, 0);
                break;
            case ePlayerMotionStates.WALK:
                stateMachine.TranslateState(1);
                animator.SetInteger(anim_state_id, 1);
                break;
            case ePlayerMotionStates.RUN:
                stateMachine.TranslateState(2);
                animator.SetInteger(anim_state_id, 2);
                break;
            case ePlayerMotionStates.CROUCH:
                stateMachine.TranslateState(3);
                animator.SetInteger(anim_state_id, 3);
                break;
            case ePlayerMotionStates.JUMP:
                stateMachine.TranslateState(4);
                animator.SetInteger(anim_state_id, 4);
                break;
            case ePlayerMotionStates.FALL:
                stateMachine.TranslateState(5);
                animator.SetInteger(anim_state_id, 5);
                break;
            case ePlayerMotionStates.SLIDE:
                stateMachine.TranslateState(6);
                animator.SetInteger(anim_state_id, 6);
                break;
            case ePlayerMotionStates.WALLRUN:
                stateMachine.TranslateState(7);
                animator.SetInteger(anim_state_id, 7);
                break;
            case ePlayerMotionStates.WALLJUMP:
                stateMachine.TranslateState(8);
                animator.SetInteger(anim_state_id, 8);
                break;
        }
        animator.SetFloat(anim_input_x_id, m_Input.x);
        animator.SetFloat(anim_input_y_id, m_Input.y);
    }
    private void StateJudge()
    {
        lastStatePointer = statePointer;
        switch (statePointer)
        {
            case ePlayerMotionStates.IDLE:
                if (!controller.isGrounded)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP))
                {
                    statePointer = ePlayerMotionStates.JUMP;
                }
                else if (m_Input.magnitude != 0)
                {
                    statePointer = ePlayerMotionStates.WALK;
                }
                else if (InputManager.Instance.GetButton(InputManager.eInputState.INPUT_CROUCH))
                {
                    statePointer = ePlayerMotionStates.CROUCH;
                }
                break;
            case ePlayerMotionStates.WALK:
                if (!controller.isGrounded)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (m_Input.magnitude == 0)
                {
                    statePointer = ePlayerMotionStates.IDLE;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP))
                {
                    statePointer = ePlayerMotionStates.JUMP;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_RUN) && m_Input.y >= 0)
                {
                    statePointer = ePlayerMotionStates.RUN;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_CROUCH))
                {
                    statePointer = ePlayerMotionStates.CROUCH;
                }
                break;
            case ePlayerMotionStates.RUN:
                if (!controller.isGrounded)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (m_Input.magnitude == 0)
                {
                    statePointer = ePlayerMotionStates.IDLE;
                }
                else if (m_Input.y <= 0)
                {
                    statePointer = ePlayerMotionStates.WALK;
                }
                else if (!runAsToggle && !InputManager.Instance.GetButton(InputManager.eInputState.INPUT_RUN))
                {
                    statePointer = ePlayerMotionStates.WALK;
                }
                else if (runAsToggle && InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_RUN))
                {
                    statePointer = ePlayerMotionStates.WALK;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP))
                {
                    statePointer = ePlayerMotionStates.JUMP;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_CROUCH) && (Time.realtimeSinceStartup - lastSlideOverTime) > slideIntoCD)
                {
                    statePointer = ePlayerMotionStates.SLIDE;
                }
                break;
            case ePlayerMotionStates.CROUCH:
                if (!controller.isGrounded)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (!crouchAsToggle && !InputManager.Instance.GetButton(InputManager.eInputState.INPUT_CROUCH) && haveHeadRoomToStand())
                {
                    statePointer = ePlayerMotionStates.IDLE;
                }
                else if (crouchAsToggle && InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_CROUCH) && haveHeadRoomToStand())
                {
                    statePointer = ePlayerMotionStates.IDLE;
                }
                break;
            case ePlayerMotionStates.JUMP:
                break;
            case ePlayerMotionStates.FALL:
                if(InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP) && !secJumped)
                {
                    secJumped = true;
                    statePointer = ePlayerMotionStates.JUMP;
                }
                break;
            case ePlayerMotionStates.SLIDE:
                if (!controller.isGrounded)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP) && haveHeadRoomToStand())
                {
                    statePointer = ePlayerMotionStates.JUMP;
                }
                else if (!crouchAsToggle && !InputManager.Instance.GetButton(InputManager.eInputState.INPUT_CROUCH))
                {
                    if (!haveHeadRoomToStand())
                    {
                        statePointer = ePlayerMotionStates.CROUCH;
                    }
                    else if (currentSpeed >= speed_Walk)
                    {
                        statePointer = ePlayerMotionStates.RUN;
                    }
                    else
                    {
                        statePointer = ePlayerMotionStates.WALK;
                    }
                }
                else if (crouchAsToggle && InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_CROUCH))
                {
                    if (!haveHeadRoomToStand())
                    {
                        statePointer = ePlayerMotionStates.CROUCH;
                    }
                    else if (currentSpeed >= speed_Walk)
                    {
                        statePointer = ePlayerMotionStates.RUN;
                    }
                    else
                    {
                        statePointer = ePlayerMotionStates.WALK;
                    }
                }
                else if (currentSpeed <= speed_Crouch)
                {
                    statePointer = ePlayerMotionStates.CROUCH;
                }
                break;
            case ePlayerMotionStates.WALLRUN:
                if (!Physics.Raycast(transform.position, -wallHitInfo.normal, out wallHitInfo, maximumWallDistance + controller.radius))
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (!controller.isGrounded && (wallHitInfo.normal.y >= 1f || wallHitInfo.normal.y <= -1f))
                {
                    statePointer = ePlayerMotionStates.FALL;
                    Debug.Log(wallHitInfo.normal.y);
                }
                else if (m_Input.y <= 0)
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                /*
                else if (runAsToggle && InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_RUN))
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (!runAsToggle && !InputManager.Instance.GetButton(InputManager.eInputState.INPUT_RUN))
                {
                    statePointer = ePlayerMotionStates.FALL;
                }
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_CROUCH))
                {
                    statePointer = ePlayerMotionStates.FALL;
                }*/
                else if (InputManager.Instance.GetButtonDown(InputManager.eInputState.INPUT_JUMP))
                {
                    statePointer = ePlayerMotionStates.WALLJUMP;
                }
                break;
            case ePlayerMotionStates.WALLJUMP:
                break;

        }
    }

    public void ActiveBob(string str)
    {
        switch (str)
        {
            case "WALK":
                StartCoroutine(WalkBob());
                break;
            case "RUN":
                StartCoroutine(RunBob());
                break;
            case "FALL":
                StartCoroutine(FallBob());
                break;
            case "LAND":
                StartCoroutine(LandBob());
                break;
            case "CROUCH":
                StartCoroutine(CrouchBob());
                break;
            case "SLIDE":
                StartCoroutine(SlideBob());
                break;
            case "WALLRUN":
                StartCoroutine(WallRunBob());
                break;
        }
    }
    IEnumerator WalkBob()
    {
        pullCamreaBack = false;
        float timer = 0f;
        float offsetX, offsetY;
        while (statePointer == ePlayerMotionStates.WALK)
        {
            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(timer) / 15;
            offsetY = Mathf.Cos(timer) / 15;
            Vector3 newPos = defaultCameraPos;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator RunBob()
    {
        pullCamreaBack = false;
        float timer = 0f;
        float offsetX, offsetY;
        while (statePointer == ePlayerMotionStates.RUN)
        {
            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(1.5f * timer) / 8;
            offsetY = Mathf.Cos(1.5f * timer) / 8;
            Vector3 newPos = runCamPos.localPosition;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);
            yield return 0;
        }
        for (timer = 0; timer < 0.3f; timer += Time.fixedDeltaTime)
        {
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator FallBob()
    {
        pullCamreaBack = false;
        float timer = 0f;
        float offsetX, offsetY;
        while (statePointer == ePlayerMotionStates.FALL)
        {
            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(timer) / 15;
            offsetY = Mathf.Cos(timer) / 15;
            Vector3 newPos = fallCamPos.localPosition;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator LandBob()
    {
        pullCamreaBack = false;
        for (float timer = 0; timer < 1.5f; timer += Time.fixedDeltaTime)
        {
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, landCamPos.localPosition, 0.05f);
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator CrouchBob()
    {
        pullCamreaBack = false;
        float timer = 0f;
        float offsetX, offsetY;
        while (statePointer == ePlayerMotionStates.CROUCH)
        {
            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(0.5f * timer) / 30;
            offsetY = Mathf.Cos(0.5f * timer) / 30;
            Vector3 newPos = crouchCamPos.localPosition;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator SlideBob()
    {
        pullCamreaBack = false;
        float timer;
        float offsetX, offsetY;
        for (timer = 0; timer < 1.5f; timer += Time.fixedDeltaTime)
        {
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, crouchCamPos.localPosition, 0.2f);
            yield return 0;
        }
        while (statePointer == ePlayerMotionStates.SLIDE)
        {
            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(0.25f * timer) / 35;
            offsetY = Mathf.Cos(0.25f * timer) / 35;
            Vector3 newPos = slideCamPos.localPosition;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);
            yield return 0;
        }
        pullCamreaBack = true;
    }
    IEnumerator WallRunBob()
    {
        pullTiltZero = false;
        pullCamreaBack = false;
        float timer = 0f;
        float offsetX, offsetY;
        while (statePointer == ePlayerMotionStates.WALLRUN)
        {
            if(Vector3.Cross(transform.forward, wallHitInfo.normal).y > 0f)
            {
                zTilt = Mathf.Lerp(zTilt, -cameraTilt, 0.2f);
            }
            else
            {
                zTilt = Mathf.Lerp(zTilt, cameraTilt, 0.2f);
            }

            timer += Time.fixedDeltaTime;
            offsetX = Mathf.Sin(1.5f * timer) / 8;
            offsetY = Mathf.Cos(1.5f * timer) / 8;
            Vector3 newPos = runCamPos.localPosition;
            newPos.x += offsetX;
            newPos.y += offsetY;
            cameraOffset.localPosition = Vector3.Lerp(cameraOffset.localPosition, newPos, 0.2f);

            yield return 0;
        }
        pullTiltZero = true;
        pullCamreaBack = true;
    }
}