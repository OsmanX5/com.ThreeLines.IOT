// ArduinoButton.cs
// This Unity component represents a digital button connected to an Arduino pin.
// It handles input from the Arduino and triggers UnityEvents based on button state changes.
// Uses ArduinoDigitalButtonLogic for core button processing logic.

using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent
using UnityEngine.UI;     // Required for Button component
using Sirenix.OdinInspector; // Required for Odin Inspector attributes
using System; // Required for Action

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// Represents a digital button connected to an Arduino pin.
    /// This component listens for input from the specified pin and triggers UnityEvents
    /// when the button is clicked, released, or its value changes.
    /// It implements IArduinoInputHandler to receive data from the central dispatcher.
    /// </summary>
    public class ArduinoButton : MonoBehaviour, IArduinoInputHandler
    {
        #region Arduino Pin Configuration
        [FoldoutGroup("Arduino Pin Configuration")]
        [Tooltip("The specific Arduino pin this button is connected to.")]
        public ArduinoPin targetPin;

        [FoldoutGroup("Arduino Pin Configuration")]
        [Tooltip("The pull-up/pull-down method configured for this pin on the Arduino.")]
        public PullMode pullMethod = PullMode.PullUp; // Default to PullUp as it's common for buttons

        [FoldoutGroup("Arduino Pin Configuration")]
        [Tooltip("Threshold value for determining when the button is considered pressed. Values above this are 'pressed'.")]
        [Range(0.0f, 1.0f)]
        public float pressThreshold = 0.5f;
        #endregion

        #region Button Logic
        [FoldoutGroup("Button State")]
        [Tooltip("The core logic handler for this button.")]
        [SerializeField]
        [ReadOnly]
        private ArduinoDigitalButtonLogic buttonLogic;
        #endregion

        #region Unity Events
        [Header("Unity Events")]
        [Tooltip("Event triggered whenever the raw float value from the pin changes.")]
        public UnityEvent<float> OnValueChanged;

        [Tooltip("Event triggered once when the button is initially pressed (transition to Pressed state).")]
        public UnityEvent OnPressedUnityEvent;

        [Tooltip("Event triggered once when the button is released (transition to Released state).")]
        public UnityEvent OnReleasedUnityEvent;

        [Tooltip("Event triggered whenever the button state changes.")]
        public UnityEvent<ButtonState, ButtonState> OnStateChangedUnityEvent;
        #endregion

        #region Unity UI Integration
        [FoldoutGroup("Unity UI Integration")]
        [Tooltip("Enable this to link with a Unity UI Button component.")]
        public bool linkToUnityUIButton = false;

        [FoldoutGroup("Unity UI Integration")]
        [ShowIf("linkToUnityUIButton")]
        [Tooltip("Optional: Drag a Unity UI Button component here. Its onClick event will be invoked when the Arduino button is clicked.")]
        public Button uiButtonToLink;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns true if the button is currently in the Pressed state.
        /// </summary>
        public bool IsPressed => buttonLogic?.IsPressed ?? false;

        /// <summary>
        /// Gets the current normalized value of the button pin.
        /// </summary>
        public float CurrentValue => buttonLogic?.CurrentValue ?? 0.0f;

        /// <summary>
        /// Gets the current high-level ButtonState of the button.
        /// </summary>
        public ButtonState CurrentButtonState => buttonLogic?.CurrentButtonState ?? ButtonState.None;

        /// <summary>
        /// Gets the button logic instance for advanced access.
        /// </summary>
        public ArduinoDigitalButtonLogic ButtonLogic => buttonLogic;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the button logic and UnityEvents.
        /// </summary>
        private void Awake()
        {
            // Initialize button logic
            buttonLogic = new ArduinoDigitalButtonLogic();
            buttonLogic.PressThreshold = pressThreshold;

            // Initialize UnityEvents to prevent NullReferenceExceptions
            if (OnValueChanged == null) OnValueChanged = new UnityEvent<float>();
            if (OnPressedUnityEvent == null) OnPressedUnityEvent = new UnityEvent();
            if (OnReleasedUnityEvent == null) OnReleasedUnityEvent = new UnityEvent();
            if (OnStateChangedUnityEvent == null) OnStateChangedUnityEvent = new UnityEvent<ButtonState, ButtonState>();
        }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Registers this component with the central Arduino input handler.
        /// </summary>
        private void OnEnable()
        {
            AllArduinoInputHandlers.RegisterHandler(this);
            Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} enabled and registered.");

            // Reset button logic when enabled
            if (buttonLogic != null)
            {
                var resetResult = buttonLogic.Reset();
                ProcessButtonResult(resetResult);
            }
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
        /// Called when the component is destroyed.
        /// No cleanup needed since we removed event subscriptions.
        /// </summary>
        private void OnDestroy()
        {
            // No event cleanup needed since ArduinoDigitalButtonLogic no longer has events
        }

        /// <summary>
        /// Called every frame to update the press threshold if it changed in the inspector.
        /// </summary>
        private void Update()
        {
            // Update press threshold if it changed in the inspector
            if (buttonLogic != null && Math.Abs(buttonLogic.PressThreshold - pressThreshold) > float.Epsilon)
            {
                buttonLogic.PressThreshold = pressThreshold;
            }
        }
        #endregion

        #region IArduinoInputHandler Implementation
        /// <summary>
        /// Processes incoming input data from the Arduino. This method is called by
        /// AllArduinoInputHandlers when data for any pin is received.
        /// </summary>
        /// <param name="pin">The ArduinoPin from which the input originated.</param>
        /// <param name="value">The float value received from the pin, normalized to 0.0-1.0.</param>
        public void ProcessInput(ArduinoPin pin, float value)
        {
            // Only process input if it's for the target pin of this button
            if (pin != targetPin || buttonLogic == null)
            {
                return;
            }

            // Delegate processing to the button logic and handle the result
            var result = buttonLogic.ProcessInput(value);
            ProcessButtonResult(result);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Processes the result from the button logic and triggers appropriate Unity events.
        /// </summary>
        /// <param name="result">The result from processing button input.</param>
        private void ProcessButtonResult(ButtonProcessResult result)
        {
            // Handle value change
            if (result.ValueChanged)
            {
                OnValueChanged?.Invoke(result.CurrentValue);
            }

            // Handle state change
            if (result.StateChanged)
            {
                OnStateChangedUnityEvent?.Invoke(result.PreviousState, result.CurrentState);
                Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin}: {buttonLogic}");
            }

            // Handle press event
            if (result.WasPressed)
            {
                OnPressedUnityEvent?.Invoke();
                Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} pressed!");

                // If linked, invoke the Unity UI Button's onClick event
                if (linkToUnityUIButton && uiButtonToLink != null)
                {
                    uiButtonToLink.onClick?.Invoke();
                    Debug.Log($"[ThreeLines.IOT.Arduino] Linked UI Button's onClick invoked for pin {targetPin}.");
                }
            }

            // Handle release event
            if (result.WasReleased)
            {
                OnReleasedUnityEvent?.Invoke();
                Debug.Log($"[ThreeLines.IOT.Arduino] ArduinoButton on pin {targetPin} released!");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Simulates a button press programmatically.
        /// </summary>
        public void SimulatePress()
        {
            if (buttonLogic != null)
            {
                var result = buttonLogic.SimulatePress();
                ProcessButtonResult(result);
            }
        }

        /// <summary>
        /// Simulates a button release programmatically.
        /// </summary>
        public void SimulateRelease()
        {
            if (buttonLogic != null)
            {
                var result = buttonLogic.SimulateRelease();
                ProcessButtonResult(result);
            }
        }

        /// <summary>
        /// Resets the button to its initial state.
        /// </summary>
        public void ResetButton()
        {
            if (buttonLogic != null)
            {
                var result = buttonLogic.Reset();
                ProcessButtonResult(result);
            }
        }
        #endregion
    }
}