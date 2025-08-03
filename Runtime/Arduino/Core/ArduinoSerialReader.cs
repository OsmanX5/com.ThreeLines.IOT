// ArduinoSerialReader.cs
// This Unity component processes raw serial line data received from an Arduino,
// parses it into pin and value, and dispatches it to registered input handlers.

using UnityEngine;
using System; // Required for Exception, Enum
using System.Globalization; // Required for CultureInfo.InvariantCulture for robust float parsing
// Removed Sirenix.OdinInspector as it's not used in this specific script's public API now.

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// A MonoBehaviour component responsible for processing raw serial data lines
    /// received from an Arduino. It parses the line into an ArduinoPin and a
    /// normalized float value, then dispatches this information to all
    /// registered IArduinoInputHandler instances via AllArduinoInputHandlers.
    /// </summary>
    /// <remarks>
    /// This component requires a 'Serial' component on the same GameObject to function.
    /// The 'Serial' component is responsible for the actual low-level serial port communication.
    /// </remarks>
    [RequireComponent(typeof(Serial))] // Ensures a Serial component is present on the GameObject
    public class ArduinoSerialReader : MonoBehaviour
    {
        [Tooltip("If true, debug messages will be logged to the console.")]
        public bool enableDebugLogs = true;

        // Reference to the Serial component on this GameObject.
        private Serial serialReader;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the reference to the Serial component and sets up line notification.
        /// </summary>
        private void Start()
        {
            serialReader = GetComponent<Serial>();
            if (serialReader == null)
            {
                Debug.LogError("[ThreeLines.IOT.Arduino] ArduinoSerialReader requires a Serial component to function. Please add a Serial component to this GameObject.");
                // Optionally disable this component if the dependency is missing
                enabled = false;
                return;
            }
            // Instruct the Serial component to notify this script whenever a complete line is received.
            // The Serial component should internally call OnSerialLine(string line) when a newline character is encountered.
            serialReader.NotifyLines = true;

            if (enableDebugLogs) Debug.Log("[ThreeLines.IOT.Arduino] ArduinoSerialReader initialized and linked with Serial component.");
        }

        /// <summary>
        /// This function is called by the underlying serial communication logic (the Serial component)
        /// whenever a complete line of data is received from the Arduino.
        /// It parses the line and dispatches the data to all registered handlers.
        /// Expected format: "PIN_NUMBER#VALUE" (e.g., "2#1.0" or "14#0.5")
        /// </summary>
        /// <param name="line">The raw string line received from the Arduino.</param>
        public void OnSerialLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (enableDebugLogs) Debug.LogWarning("[ThreeLines.IOT.Arduino] Received empty or whitespace serial line.");
                return;
            }

            // Trim any leading/trailing whitespace or newline characters
            string trimmedLine = line.Trim();

            if (enableDebugLogs) Debug.Log($"[ThreeLines.IOT.Arduino] Raw serial line received: '{trimmedLine}'");

            // Split the line using '#' as the separator, as per the defined schema.
            string[] parts = trimmedLine.Split('#');

            // Now expecting 2 parts: Pin and Value.
            if (parts.Length != 2)
            {
                if (enableDebugLogs) Debug.LogError($"[ThreeLines.IOT.Arduino] Invalid serial line format: '{trimmedLine}'. Expected 'PIN_NUMBER#VALUE'.");
                return;
            }

            int pinNumber;
            float value;

            // Try parsing the pin number.
            if (!int.TryParse(parts[0], out pinNumber))
            {
                if (enableDebugLogs) Debug.LogError($"[ThreeLines.IOT.Arduino] Failed to parse pin number from '{parts[0]}' in line: '{trimmedLine}'.");
                return;
            }

            // Try parsing the value, using InvariantCulture for consistent float parsing across locales.
            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                if (enableDebugLogs) Debug.LogError($"[ThreeLines.IOT.Arduino] Failed to parse value from '{parts[1]}' in line: '{trimmedLine}'.");
                return;
            }

            // Validate value range (ensure it's between 0.0 and 1.0).
            value = Mathf.Clamp01(value);

            // Convert integer pin number to ArduinoPin enum.
            ArduinoPin arduinoPin;
            try
            {
                arduinoPin = (ArduinoPin)pinNumber;
                // Optional: Add a check if the parsed pinNumber is a valid enum value.
                if (!Enum.IsDefined(typeof(ArduinoPin), pinNumber))
                {
                    if (enableDebugLogs) Debug.LogWarning($"[ThreeLines.IOT.Arduino] Received data for unknown ArduinoPin number: {pinNumber} in line: '{trimmedLine}'. Skipping dispatch.");
                    return;
                }
            }
            catch (Exception ex)
            {
                if (enableDebugLogs) Debug.LogError($"[ThreeLines.IOT.Arduino] Error converting pin number {pinNumber} to ArduinoPin enum: {ex.Message}");
                return;
            }

            // Dispatch the processed input to all registered handlers.
            AllArduinoInputHandlers.ProcessAllInputs(arduinoPin, value);

            if (enableDebugLogs) Debug.Log($"[ThreeLines.IOT.Arduino] Dispatched data: Pin={arduinoPin}, Value={value}");
        }
    }
}
