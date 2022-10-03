using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseLook : MonoBehaviour
{
    [Header("Custom Flags")]
    [Tooltip("Toggle mouse look on and off")]
    public bool enableMouseLook = true;

    [Header("Sensitivity")]
    [SerializeField, Tooltip("鼠标输入敏感度")]
    private Vector2 lookSensitivity = new Vector2(3.0f, 3.0f);

    [Header("Camera Smooth")]
    public bool lookSmoothing = false;
    [SerializeField, Tooltip("如果开启了平滑输入，这个属性会决定鼠标输入的平滑程度，越低则越平滑")]
    private float lookSmoothFactor = 10.0f;

    [Header("Camera Clamp")]
    public bool clampVerticalRotation = true;
    public float MinimumX = -80f;
    public float MaximumX = 80f;

    private InputManager inputManager;
    private Vector2 lastRotationChanges = Vector2.zero;
    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    public void Init(Transform character, Transform camera)
    {
        m_CharacterTargetRot = character.localRotation;
        m_CameraTargetRot = camera.localRotation;
    }

    void Start()
    {
        inputManager = InputManager.Instance;
        if (!inputManager)
        {
            Debug.Log("MouseLook:: Cannot find an instance of FPEInputManager in the scene. Mouse look will not function correctly!");
        }
    }

    public void LookRotation(Transform character, Transform camera)
    {
        if (enableMouseLook)
        {
            lastRotationChanges.x = inputManager.GetAxis(InputManager.eInputState.INPUT_MOUSELOOKX) * lookSensitivity.x;
            lastRotationChanges.y = inputManager.GetAxis(InputManager.eInputState.INPUT_MOUSELOOKY) * lookSensitivity.y;
        }
        /*
        if (lastRotationChanges.x == 0 & lastRotationChanges.y == 0)
        {
            lastRotationChanges.x = inputManager.GetAxis(InputManager.eInputState.INPUT_LOOKX);
            lastRotationChanges.y = inputManager.GetAxis(InputManager.eInputState.INPUT_LOOKY);
        }
        */
        
        m_CharacterTargetRot *= Quaternion.Euler(0.0f, lastRotationChanges.y, 0.0f);
        m_CameraTargetRot *= Quaternion.Euler(-lastRotationChanges.x, 0.0f, 0.0f);

        // Clamp
        if (clampVerticalRotation)
        {
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
        }

        // LookSmooth
        if (lookSmoothing)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, lookSmoothFactor * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, lookSmoothFactor * Time.deltaTime);
        }
        else
        {
            character.localRotation = m_CharacterTargetRot;
            camera.localRotation = m_CameraTargetRot;
        }
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }

    /// <summary>
    /// Makes player camera look at a position
    /// </summary>
    /// <param name="character">Character's transform</param>
    /// <param name="camera">Character's 'Main' camera transform</param>
    /// <param name="focalPoint">The world position of the focal point to make player look at</param>
    public void LookAtPosition(Transform character, Transform camera, Vector3 focalPoint)
    {
        // Make character face target //
        Vector3 relativeCharPosition = focalPoint - character.position;
        Quaternion rotation = Quaternion.LookRotation(relativeCharPosition);
        Vector3 flatCharRotation = rotation.eulerAngles;
        flatCharRotation.x = 0.0f;
        flatCharRotation.z = 0.0f;
        character.localRotation = Quaternion.Euler(flatCharRotation);
        m_CharacterTargetRot = character.localRotation;

        // Make Camera face target //
        Vector3 relativeCamPosition = focalPoint - camera.position;
        Quaternion camRotation = Quaternion.LookRotation(relativeCamPosition);
        Vector3 flatCamRotation = camRotation.eulerAngles;
        flatCamRotation.y = 0.0f;
        flatCamRotation.z = 0.0f;
        camera.localRotation = Quaternion.Euler(flatCamRotation);
        m_CameraTargetRot = camera.localRotation;
    }
}

public class CameraLerpControlleredBob
{
    public float BobDuration;
    public float BobAmount;
    private float m_Offset = 0f;

    // provides the offset that can be used
    public float Offset()
    {
        return m_Offset;
    }

    public IEnumerator DoBobCycle()
    {
        // make the camera move down slightly
        float t = 0f;
        while (t < BobDuration)
        {
            m_Offset = Mathf.Lerp(0f, BobAmount, t / BobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // make it move back to neutral
        t = 0f;

        while (t < BobDuration)
        {
            m_Offset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        m_Offset = 0f;
    }
}