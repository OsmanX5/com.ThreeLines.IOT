// ArduinoButton.cs
// This Unity component represents a digital button connected to an Arduino pin.
// It handles input from the Arduino and triggers UnityEvents based on button state changes.

using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using UnityEngine.UI;     // Required for Button component
using Sirenix.OdinInspector; // Required for Odin Inspector attributes
using System; // Required for Action (optional, could use UnityEvent directly for value changed)

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// Represents the current high-level state of the Arduino button.
    /// </summary>
    public enum ButtonState
    {
        /// <summary>
        /// The initial or inactive state of the button.
        /// </summary>
        None,
        /// <summary>
        /// The button has just been pressed.
        /// </summary>
        Pressed,
        /// <summary>
        /// The button is being held down.
        /// </summary>
        Held,
        /// <summary>
        /// The button has just been released.
        /// </summary>
        Released
    }

    /// <summary>
    /// Represents a digital button connected to an Arduino pin.
    /// This component listens for input from the specified pin and triggers UnityEvents
    /// when the button is clicked, released, or its value changes.
    /// It implements IArduinoInputHandler to receive data from the central dispatcher.
    /// </summary>
    public class ArduinoButton : MonoBehaviour, IArduinoInputHandler
    {
        [FoldoutGroup("Arduino Pin Configuration")]
        [Tooltip("The specific Arduino pin this button is connected to.")]
        public ArduinoPin targetPin;

        [FoldoutGroup("Arduino Pin Configuration")]
        [Tooltip("The pull-up/pull-down method configured for this pin on the Arduino.")]
        public PullMode pullMethod = PullMode.PullUp; // Default to PullUp as it's common for buttons

        [FoldoutGroup("Button State")]
        [Tooltip("The current normalized value received from the Arduino pin (0.0 to 1.0).")]
        [SerializeField] // Make it visible in the Inspector for debugging, but not directly editable
        private float currentValue = 0.0f;

        [FoldoutGroup("Button State")]
        [Tooltip("The previous normalized value received from the Arduino pin.")]
        [SerializeField]
        private float previousValue = 0.0f;

        // Removed rawOnOffState field, as OnOffState enum is being removed.
        // The raw state will now be determined directly from currentValue.

        [FoldoutGroup("Button State")]
        [Tooltip("The current high-level state of the button (None, Pressed, Held, Released).")]
        [SerializeField]
        [EnumToggleButtons] // Display as toggle buttons
        [ReadOnly]         // Make it read-only in the Inspector
        private ButtonState currentButtonState = ButtonState.None;

        [Header("Unity Events")]
        // Removed OnClick event. OnPressedUnityEvent will now be the primary event for a button press.
        // Removed OnRelease event. OnReleasedUnityEvent will now be the primary event for a button release.

        [Tooltip("Event triggered whenever the raw float value from the pin changes.")]
        public UnityEvent<float> OnValueChanged; // UnityEvent that passes a float argument

        [Tooltip("Event triggered once when the button is initially pressed (transition to Pressed state).")]
        public UnityEvent OnPressedUnityEvent;

        [Tooltip("Event triggered continuously while the button is held down (in Held state).")]
        public UnityEvent OnHoldUnityEvent;

        [Tooltip("Event triggered once when the button is released (transition to Released state).")]
        public UnityEvent OnReleasedUnityEvent;

        public event Action<float> OnValueChangedAction; // Optional Action for value change, can be used instead of UnityEvent
        public event Action OnPressed; // Optional Action for button press
        public event Action OnHold;    // Optional Action for button hold
        public event Action OnReleased; // Optional Action for button release


        [FoldoutGroup("Unity UI Integration")]
        [Tooltip("Enable this to link with a Unity UI Button component.")]
        public bool linkToUnityUIButton = false;

        [FoldoutGroup("Unity UI Integration")]
        [ShowIf("linkToUnityUIButton")] // Only show this field if linkToUnityUIButton is true
        [Tooltip("Optional: Drag a Unity UI Button component here. Its onClick event will be invoked when the Arduino button is clicked.")]
        public Button uiButtonToLink;

        public bool IsPressed
        {
            get { return currentButtonState == ButtonState.Pressed; }
        }
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Used to initialize UnityEvents to prevent NullReferenceExceptions if no listeners are assigned.
        /// </summary>
        private void Awake()
        {
            // Removed initialization for OnClick and OnRelease
            if (OnValueChanged == null) OnValueChanged = new UnityEvent<float>();
            if (OnPressedUnityEvent == null) OnPressedUnityEvent = new UnityEvent(); // Initialize new event
            if (OnHoldUnityEvent == null) OnHoldUnityEvent = new UnityEvent();       // Initialize new event
            if (OnReleasedUnityEvent == null) OnReleasedUnityEvent = new UnityEvent(); // Initialize new event
        }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Registers this component with the central Arduino input handler.
        /// </summary>
        private void OnEnable()
        {
            AllArduinoInputHandlers.RegisterHandler(this);
            Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} enabled and registered.");
            // Reset button state when enabled
            currentButtonState = ButtonState.None;
            // rawOnOffState removed
            currentValue = 0.0f;
            previousValue = 0.0f;
        }

        /// <summary>
        /// Called when the behaviour becomes disabled or inactive.
        /// Unregisters this component from the central Arduino input handler.
        /// </summary>
        private void OnDisable()
        {
            AllArduinoInputHandlers.UnregisterHandler(this);
            Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} disabled and unregistered.");
        }

        /// <summary>
        /// Processes incoming input data from the Arduino. This method is called by
        /// AllArduinoInputHandlers when data for any pin is received.
        /// </summary>
        /// <param name="pin">The ArduinoPin from which the input originated.</param>
        /// <param name="value">The float value received from the pin, normalized to 0.0-1.0.</param>
        public void ProcessInput(ArduinoPin pin, float value)
        {
            // Only process input if it's for the target pin of this button.
            if (pin != targetPin)
            {
                return;
            }

            previousValue = currentValue;
            currentValue = value;

            // Check for value change and invoke OnValueChanged event
            if (Mathf.Abs(currentValue - previousValue) > float.Epsilon) // Use Epsilon for float comparison
            {
                OnValueChanged?.Invoke(currentValue); // Null check added
            }

            // Determine the new raw binary state based on the normalized value.
            // Assuming 0.5 as a threshold for simplicity. For digital, it will be 0 or 1.
            bool newIsOn = (currentValue > 0.5f);
            bool wasOn = (previousValue > 0.5f); // Determine previous binary state based on previousValue

            // State machine for ButtonState
            if (newIsOn && !wasOn) // Button just pressed (transition from Off to On)
            {
                currentButtonState = ButtonState.Pressed;
                OnPressedUnityEvent?.Invoke(); // Null check added
                OnPressed?.Invoke(); // Optional Action for button press
                Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} pressed (state: {currentButtonState})!");

                // If linked, invoke the Unity UI Button's onClick event
                if (linkToUnityUIButton && uiButtonToLink != null)
                {
                    uiButtonToLink.onClick?.Invoke(); // Null check added for onClick
                    Debug.Log($"[ThreeLines.IOT.Arduino] Linked UI Button's onClick invoked for pin {targetPin}.");
                }
            }
            else if (!newIsOn && wasOn) // Button just released (transition from On to Off)
            {
                currentButtonState = ButtonState.Released;
                OnReleasedUnityEvent?.Invoke(); // Null check added
                OnReleased?.Invoke(); // Optional Action for button release
                Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} released (state: {currentButtonState})!");
            }
            else if (newIsOn && wasOn) // Button remains On (either Pressed or Held)
            {
                if (currentButtonState == ButtonState.Pressed)
                {
                    // If the button was just pressed and remains on, transition to Held
                    currentButtonState = ButtonState.Held;
                    Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} transitioned to held (state: {currentButtonState})!");
                }
                if (currentButtonState == ButtonState.Held)
                {
                    // If the button is held, continuously invoke OnHoldUnityEvent
                    OnHoldUnityEvent?.Invoke(); // Null check added
                    OnHold?.Invoke(); // Optional Action for button hold

                    // Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} held (state: {currentButtonState})!"); // Log less frequently for hold
                }
            }
            else // !newIsOn && !wasOn (Button remains Off)
            {
                currentButtonState = ButtonState.None; // Or keep as Released if it was just released and no new press
            }
        }

        /// <summary>
        /// Gets the current normalized value of the button pin.
        /// </summary>
        /// <returns>The current value (0.0 to 1.0).</returns>
        public float GetCurrentValue()
        {
            return currentValue;
        }

        /// <summary>
        /// Gets the current high-level ButtonState of the button.
        /// </summary>
        /// <returns>The current ButtonState (None, Pressed, Held, Released).</returns>
        public ButtonState GetCurrentButtonState()
        {
            return currentButtonState;
        }
    }
}
