using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get { return _instance; }
    }

    protected Dictionary<eInputState, InputAxis> VirtualAxes = new Dictionary<eInputState, InputAxis>();
    protected Dictionary<eInputState, InputButton> VirtualButtons = new Dictionary<eInputState, InputButton>();

    // 摇杆相关
    /*
    [SerializeField, Tooltip("The absolute value the right stick must meet or exceed before a virtual 'right' button press is captured.")]
    private float rightStickButtonThreshold = 0.9f;
    [SerializeField, Tooltip("The number of seconds before a right stick 'button press' will be repeated if the stick is held left or right in position.")]
    private float rightStickButtonRepeatTime = 0.2f;
    private float rightStickButtonRepeatCounter = 0.0f;

    // An additional factor automatically applied to all gamepad analog stick inputs for a reasonable baseline
    private float gamepadAnalogStickSensitivityMultplier = 60.0f;

    [SerializeField, Tooltip("If true, player look movement sensitivity will be boosted up (by gamepadLookSensitivityBoost)")]
    private bool useGamepad = false;
    public bool UseGamepad {
        get { return useGamepad; }
        set { useGamepad = value; }
    }
    [SerializeField,Tooltip("If Use Gamepad is true, gamepad look input is multiplied by this factor.")]
    private float gamepadBoostMultiplier = 2.0f;
    private float appliedGamepadBoost = 1.0f;

    [SerializeField, Tooltip("The absolute value the right stick must meet or exceed before a virtual 'right' button press is captured.")]
    private float rightStickButtonThreshold = 0.9f;
    [SerializeField, Tooltip("The number of seconds before a right stick 'button press' will be repeated if the stick is held left or right in position.")]
    private float rightStickButtonRepeatTime = 0.2f;
    private float rightStickButtonRepeatCounter = 0.0f;

    [SerializeField, Tooltip("Deadzone for game pad analog sticks. Any value less than this will be considered 'zero'")]
    private float analogStickDeadzone = 0.1f;

    private float triggerDeadzone = 0.05f;
    private float previousLeftTriggerValue = 0.0f;
    */

    public enum eInputState
    {
        // Action buttons
        INPUT_INTERACT = 0,
        INPUT_FIRE,
        INPUT_GUNRELOAD, 
        INPUT_AIM,
        INPUT_THROW,
        INPUT_SKILL,
        INPUT_CLOSE,
        INPUT_MENU,
        //INPUT_MENU_PREVIOUS_TAB,
        //INPUT_MENU_NEXT_TAB,
        //INPUT_MENU_PREVIOUS_PAGE,
        //INPUT_MENU_NEXT_PAGE,

        // Looking
        INPUT_MOUSELOOKX,
        INPUT_MOUSELOOKY,
        //INPUT_LOOKX,
        //INPUT_LOOKY,

        // Movement
        INPUT_HORIZONTAL, //此处的水平和垂直对应输入轴
        INPUT_VERTICAL,
        INPUT_JUMP,
        INPUT_CROUCH,
        INPUT_RUN
    }

    void Awake()
    {
        if (_instance != null)
        {
            Debug.LogWarning("FPEInputManager:: Duplicate instance of FPEInputManager, deleting second one.");
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        VirtualButtons.Add(eInputState.INPUT_INTERACT, new InputButton(eInputState.INPUT_INTERACT, "Interact"));
        VirtualButtons.Add(eInputState.INPUT_FIRE, new InputButton(eInputState.INPUT_FIRE, "Fire"));
        VirtualButtons.Add(eInputState.INPUT_GUNRELOAD, new InputButton(eInputState.INPUT_GUNRELOAD, "GunReload"));
        VirtualButtons.Add(eInputState.INPUT_AIM, new InputButton(eInputState.INPUT_AIM, "Aim"));
        VirtualButtons.Add(eInputState.INPUT_THROW, new InputButton(eInputState.INPUT_THROW, "Throw"));
        VirtualButtons.Add(eInputState.INPUT_SKILL, new InputButton(eInputState.INPUT_SKILL, "Skill"));
        VirtualButtons.Add(eInputState.INPUT_CLOSE, new InputButton(eInputState.INPUT_CLOSE, "Close"));
        VirtualButtons.Add(eInputState.INPUT_MENU, new InputButton(eInputState.INPUT_MENU, "Menu"));
        //VirtualButtons.Add(eInputState.INPUT_MENU_PREVIOUS_TAB, new InputButton(eInputState.INPUT_MENU_PREVIOUS_TAB, "Menu Previous Tab"));
        //VirtualButtons.Add(eInputState.INPUT_MENU_NEXT_TAB, new InputButton(eInputState.INPUT_MENU_NEXT_TAB, "Menu Next Tab"));
        //VirtualButtons.Add(eInputState.INPUT_MENU_PREVIOUS_PAGE, new InputButton(eInputState.INPUT_MENU_PREVIOUS_PAGE, "Menu Previous Page"));
        //VirtualButtons.Add(eInputState.INPUT_MENU_NEXT_PAGE, new InputButton(eInputState.INPUT_MENU_NEXT_PAGE, "Menu Next Page"));

        VirtualAxes.Add(eInputState.INPUT_MOUSELOOKX, new InputAxis(eInputState.INPUT_MOUSELOOKX, "Mouse X"));
        VirtualAxes.Add(eInputState.INPUT_MOUSELOOKY, new InputAxis(eInputState.INPUT_MOUSELOOKY, "Mouse Y"));
        //VirtualAxes.Add(eInputState.INPUT_LOOKX, new InputAxis(eInputState.INPUT_LOOKX, "Look X"));
        //VirtualAxes.Add(eInputState.INPUT_LOOKY, new InputAxis(eInputState.INPUT_LOOKY, "Look Y"));
        VirtualAxes.Add(eInputState.INPUT_HORIZONTAL, new InputAxis(eInputState.INPUT_HORIZONTAL, "Horizontal"));
        VirtualAxes.Add(eInputState.INPUT_VERTICAL, new InputAxis(eInputState.INPUT_VERTICAL, "Vertical"));

        VirtualButtons.Add(eInputState.INPUT_HORIZONTAL, new InputButton(eInputState.INPUT_HORIZONTAL, "Horizontal"));
        VirtualButtons.Add(eInputState.INPUT_VERTICAL, new InputButton(eInputState.INPUT_VERTICAL, "Vertical"));
        VirtualButtons.Add(eInputState.INPUT_JUMP, new InputButton(eInputState.INPUT_JUMP, "Jump"));
        VirtualButtons.Add(eInputState.INPUT_CROUCH, new InputButton(eInputState.INPUT_CROUCH, "Crouch"));
        VirtualButtons.Add(eInputState.INPUT_RUN, new InputButton(eInputState.INPUT_RUN, "Run"));


    }

    void Update()
    {
        // This Update is the core of all FPE input. 
        // The hardware for your various platform and implementation is polled directly from Unity Input in this update.
        //
        // Note: The Input settings that ship with this asset are assumed to still be in place for this Update function
        // to work properly. If changes or additions are made, the names referenced in the Update() below must also change.
        // For example, the string "Gamepad Interact" must exist in Edit > Project Settings > Input if it is to be checked
        // against and have its value/state put into the virtual FPE_INPUT_INTERACT button.



        // -- Interact -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Interact"))
        {
            VirtualButtons[eInputState.INPUT_INTERACT].Pressed();
        }
        if (Input.GetButtonUp("Interact"))
        {
            VirtualButtons[eInputState.INPUT_INTERACT].Released();
        }
        // Game Pad

        // -- Fire -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Fire"))
        {
            VirtualButtons[eInputState.INPUT_FIRE].Pressed();
        }
        if (Input.GetButtonUp("Fire"))
        {
            VirtualButtons[eInputState.INPUT_FIRE].Released();
        }

        // -- GunReload -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("GunReload"))
        {
            VirtualButtons[eInputState.INPUT_GUNRELOAD].Pressed();
        }
        if (Input.GetButtonUp("GunReload"))
        {
            VirtualButtons[eInputState.INPUT_GUNRELOAD].Released();
        }

        // -- Aim -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Aim"))
        {
            VirtualButtons[eInputState.INPUT_AIM].Pressed();
        }
        if (Input.GetButtonUp("Aim"))
        {
            VirtualButtons[eInputState.INPUT_AIM].Released();
        }

        // -- Throw -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Throw"))
        {
            VirtualButtons[eInputState.INPUT_THROW].Pressed();
        }
        if (Input.GetButtonUp("Throw"))
        {
            VirtualButtons[eInputState.INPUT_THROW].Released();
        }

        // -- Skill -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Skill"))
        {
            VirtualButtons[eInputState.INPUT_SKILL].Pressed();
        }
        if (Input.GetButtonUp("Skill"))
        {
            VirtualButtons[eInputState.INPUT_SKILL].Released();
        }

        // -- Close -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Close"))
        {
            VirtualButtons[eInputState.INPUT_CLOSE].Pressed();
        }
        if (Input.GetButtonUp("Close"))
        {
            VirtualButtons[eInputState.INPUT_CLOSE].Released();
        }

        // -- Open Menu -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Menu"))
        {
            VirtualButtons[eInputState.INPUT_MENU].Pressed();
        }
        if (Input.GetButtonUp("Menu"))
        {
            VirtualButtons[eInputState.INPUT_MENU].Released();
        }

        // -- Look -- //
        // Mouse
        // 因为轴不相同 所以XY互换
        VirtualAxes[eInputState.INPUT_MOUSELOOKX].Update(Input.GetAxis("Mouse Y"));
        VirtualAxes[eInputState.INPUT_MOUSELOOKY].Update(Input.GetAxis("Mouse X"));

        // -- Movement -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Horizontal"))
        {
            VirtualButtons[eInputState.INPUT_HORIZONTAL].Pressed();
        }
        if (Input.GetButtonUp("Horizontal"))
        {
            VirtualButtons[eInputState.INPUT_HORIZONTAL].Released();
        }
        if (Input.GetButtonDown("Vertical"))
        {
            VirtualButtons[eInputState.INPUT_VERTICAL].Pressed();
        }
        if (Input.GetButtonUp("Vertical"))
        {
            VirtualButtons[eInputState.INPUT_VERTICAL].Released();
        }
        // Continuous axis values for things like player movement
        VirtualAxes[eInputState.INPUT_HORIZONTAL].Update(Input.GetAxis("Horizontal"));
        VirtualAxes[eInputState.INPUT_VERTICAL].Update(Input.GetAxis("Vertical"));

        // -- Jump -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Jump"))
        {
            VirtualButtons[eInputState.INPUT_JUMP].Pressed();
        }
        if (Input.GetButtonUp("Jump"))
        {
            VirtualButtons[eInputState.INPUT_JUMP].Released();
        }

        // -- Crouch -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Crouch"))
        {
            VirtualButtons[eInputState.INPUT_CROUCH].Pressed();
        }
        if (Input.GetButtonUp("Crouch"))
        {
            VirtualButtons[eInputState.INPUT_CROUCH].Released();
        }

        // -- Run -- //
        // Keyboard and Mouse
        if (Input.GetButtonDown("Run"))
        {
            VirtualButtons[eInputState.INPUT_RUN].Pressed();
        }
        if (Input.GetButtonUp("Run"))
        {
            VirtualButtons[eInputState.INPUT_RUN].Released();
        }
    }

    public bool GetButton(eInputState buttonID)
    {
        if (VirtualButtons.ContainsKey(buttonID))
        {
            return VirtualButtons[buttonID].GetButton;
        }
        else
        {
            Debug.LogError("InputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
            return false;
        }
    }
    public bool GetButtonDown(eInputState buttonID)
    {
        if (VirtualButtons.ContainsKey(buttonID))
        {
            return VirtualButtons[buttonID].GetButtonDown;
        }
        else
        {
            Debug.LogError("InputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
            return false;
        }
    }
    public bool GetButtonUp(eInputState buttonID)
    {
        if (VirtualButtons.ContainsKey(buttonID))
        {
            return VirtualButtons[buttonID].GetButtonUp;
        }
        else
        {
            Debug.LogError("InputManager.GetButtonDown:: No button ID '" + buttonID + "'. Are you looking for an axis instead?");
            return false;
        }
    }

    /// <summary>
    /// Returns a cleaned version of the axis, yielding to deadzone value.
    /// </summary>
    /// <param name="axisID">The axis to check</param>
    /// <returns>The cleaned version of the axis value, adhering to deadzone values</returns>
    public float GetAxis(eInputState axisID)
    {
        if (VirtualAxes.ContainsKey(axisID))
        {
            return VirtualAxes[axisID].GetValue;
        }
        else
        {
            Debug.LogError("InputManager.GetAxis:: No axis ID '" + axisID + "'. Are you looking for a button instead?");
            return 0.0f;
        }
    }

    /// <summary>
    /// Returns the raw axis value, ignoring deadzone
    /// </summary>
    /// <param name="axisID">The axis to check</param>
    /// <returns>The raw axis value</returns>
    public float GetAxisRaw(eInputState axisID)
    {

        if (VirtualAxes.ContainsKey(axisID))
        {
            return VirtualAxes[axisID].GetValue;
        }
        else
        {
            Debug.LogError("InputManager.GetAxis:: No axis ID '" + axisID + "'. Are you looking for a button instead?");
            return 0.0f;
        }

    }

    /// <summary>
    /// This function flushes a subset of the input to ensure clean state is ready after operations like saving and loading a saved game, changing scene, etc.
    /// NOTE: Not all inputs are flushed. If you add custom buttons or axes, you should play test to check if flushing those inputs is required or not.
    /// </summary>
    public void FlushInputs()
    {
        // Prevent "sticky interact" on scene change, which required an extra key up and key down event before first interaction could happen
        VirtualButtons[eInputState.INPUT_INTERACT].Flush();
    }

    public class InputButton
    {
        public eInputState id { get; private set; }
        public string friendlyName { get; private set; }

        private int lastPressedFrame = -1;
        private int releasedFrame = -1;
        private bool pressed = false;

        public InputButton(eInputState id, string friendlyName)
        {
            this.id = id;
            this.friendlyName = friendlyName;
        }
        
        public void Pressed()
        {
            if (pressed)
                return;
            pressed = true;
            lastPressedFrame = Time.frameCount;
        }
        public void Released()
        {
            pressed = false;
            releasedFrame = Time.frameCount;
        }
        public bool GetButton
        {
            get{ return pressed; }
        }
        public bool GetButtonDown
        {
            get { return (lastPressedFrame - Time.frameCount) == -1; }
        }
        public bool GetButtonUp
        {
            get { return (releasedFrame - Time.frameCount) == -1; }
        }
        public void Flush()
        {
            pressed = false;
            releasedFrame = Time.frameCount;
        }
    }
    public class InputAxis
    {
        public eInputState id { get; private set; }
        public string name { get; private set; }
        private float value;

        public InputAxis(eInputState id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public float GetValue { get { return value; } }
        public float GetValueRaw { get { return value; } }

        public void Update(float latestValue)
        {
            value = latestValue;
        }
    }
}
