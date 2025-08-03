// ArduinoDigitalButtonLogic.cs
// This class contains the core logic for processing digital button input from Arduino pins.
// It handles state transitions and value processing independently from Unity components.

using Sirenix.OdinInspector;
using System;

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
        /// The button has just been released.
        /// </summary>
        Released
    }

    /// <summary>
    /// Contains the result of processing button input, including what changed and current state.
    /// </summary>
    public struct ButtonProcessResult
    {
        /// <summary>
        /// True if the raw value changed during this processing cycle.
        /// </summary>
        public bool ValueChanged;

        /// <summary>
        /// True if the button state changed during this processing cycle.
        /// </summary>
        public bool StateChanged;

        /// <summary>
        /// True if the button was pressed during this processing cycle.
        /// </summary>
        public bool WasPressed;

        /// <summary>
        /// True if the button was released during this processing cycle.
        /// </summary>
        public bool WasReleased;

        /// <summary>
        /// The button state before processing.
        /// </summary>
        public ButtonState PreviousState;

        /// <summary>
        /// The button state after processing.
        /// </summary>
        public ButtonState CurrentState;

        /// <summary>
        /// The raw value before processing.
        /// </summary>
        public float PreviousValue;

        /// <summary>
        /// The raw value after processing.
        /// </summary>
        public float CurrentValue;
    }

    /// <summary>
    /// Contains the core logic for processing digital button input from Arduino pins.
    /// Handles state transitions, value processing, and provides events for state changes.
    /// This class is independent of Unity components and can be used in any C# context.
    /// </summary>
    [Serializable]
    public class ArduinoDigitalButtonLogic
    {
        #region Private Fields
        private float currentValue = 0.0f;
        private float previousValue = 0.0f;
        [ShowInInspector]
        [EnumToggleButtons]
        [ReadOnly]
        private ButtonState currentButtonState = ButtonState.None;
        private float pressThreshold = 0.5f; // Threshold for determining pressed state
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the current normalized value of the button pin (0.0 to 1.0).
        /// </summary>
        public float CurrentValue => currentValue;

        /// <summary>
        /// Gets the previous normalized value of the button pin (0.0 to 1.0).
        /// </summary>
        public float PreviousValue => previousValue;

        /// <summary>
        /// Gets the current high-level ButtonState of the button.
        /// </summary>
        public ButtonState CurrentButtonState => currentButtonState;

        /// <summary>
        /// Gets or sets the threshold value for determining when a button is considered pressed.
        /// Default is 0.5f. Values above this threshold are considered "pressed".
        /// </summary>
        public float PressThreshold
        {
            get => pressThreshold;
            set => pressThreshold = Math.Max(0.0f, Math.Min(1.0f, value)); // Clamp between 0 and 1
        }

        /// <summary>
        /// Returns true if the button is currently in the Pressed state.
        /// </summary>
        public bool IsPressed => currentButtonState == ButtonState.Pressed;
        #endregion



        #region Public Methods
        /// <summary>
        /// Processes incoming input data from the Arduino pin.
        /// Updates internal state and returns information about what changed.
        /// </summary>
        /// <param name="value">The float value received from the pin, normalized to 0.0-1.0.</param>
        /// <returns>A ButtonProcessResult containing information about state changes and value changes.</returns>
        public ButtonProcessResult ProcessInput(float value)
        {
            // Store previous values
            previousValue = currentValue;
            currentValue = value;
            ButtonState previousButtonState = currentButtonState;

            // Check for value change
            bool valueChanged = Math.Abs(currentValue - previousValue) > float.Epsilon;

            // Determine the binary states based on the threshold
            bool newIsOn = (currentValue > pressThreshold);
            bool wasOn = (previousValue > pressThreshold);

            // State machine for ButtonState
            bool stateChanged = false;
            bool wasPressed = false;
            bool wasReleased = false;

            if (newIsOn && !wasOn) // Button just pressed (transition from Off to On)
            {
                currentButtonState = ButtonState.Pressed;
                stateChanged = true;
                wasPressed = true;
            }
            else if (!newIsOn && wasOn) // Button just released (transition from On to Off)
            {
                currentButtonState = ButtonState.Released;
                stateChanged = true;
                wasReleased = true;
            }
            else // Button remains in same binary state (either On or Off)
            {
                // Keep current state as is - no transitions needed
                stateChanged = false;
            }

            return new ButtonProcessResult
            {
                ValueChanged = valueChanged,
                StateChanged = stateChanged,
                WasPressed = wasPressed,
                WasReleased = wasReleased,
                PreviousState = previousButtonState,
                CurrentState = currentButtonState,
                PreviousValue = previousValue,
                CurrentValue = currentValue
            };
        }

        /// <summary>
        /// Resets the button logic to its initial state.
        /// Clears all values and sets the state to None.
        /// </summary>
        /// <returns>A ButtonProcessResult indicating the reset operation.</returns>
        public ButtonProcessResult Reset()
        {
            ButtonState previousState = currentButtonState;
            float previousVal = currentValue;

            currentValue = 0.0f;
            previousValue = 0.0f;
            currentButtonState = ButtonState.None;

            return new ButtonProcessResult
            {
                ValueChanged = Math.Abs(previousVal) > float.Epsilon,
                StateChanged = previousState != ButtonState.None,
                WasPressed = false,
                WasReleased = false,
                PreviousState = previousState,
                CurrentState = currentButtonState,
                PreviousValue = previousVal,
                CurrentValue = currentValue
            };
        }

        /// <summary>
        /// Simulates a button press by setting the value above the press threshold.
        /// Useful for testing or programmatic button activation.
        /// </summary>
        /// <returns>A ButtonProcessResult containing the result of the simulated press.</returns>
        public ButtonProcessResult SimulatePress()
        {
            return ProcessInput(1.0f);
        }

        /// <summary>
        /// Simulates a button release by setting the value below the press threshold.
        /// Useful for testing or programmatic button deactivation.
        /// </summary>
        /// <returns>A ButtonProcessResult containing the result of the simulated release.</returns>
        public ButtonProcessResult SimulateRelease()
        {
            return ProcessInput(0.0f);
        }

        /// <summary>
        /// Gets a string representation of the current button state for debugging.
        /// </summary>
        /// <returns>A formatted string containing current state information.</returns>
        public override string ToString()
        {
            return $"ButtonLogic [State: {currentButtonState}, Value: {currentValue:F3}, PrevValue: {previousValue:F3}, Threshold: {pressThreshold:F3}]";
        }
        #endregion
    }
}