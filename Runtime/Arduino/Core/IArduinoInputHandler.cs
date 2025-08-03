// IArduinoInputHandler.cs
// This interface defines a contract for classes that can process input data received from an Arduino.

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// Defines a contract for classes that are capable of processing input data
    /// received from an Arduino board. This allows for flexible handling of sensor
    /// readings or other data points based on the pin they originate from.
    /// </summary>
    public interface IArduinoInputHandler
    {
        /// <summary>
        /// Processes an input value received from a specific Arduino pin.
        /// </summary>
        /// <param name="pin">The pin number (digital or analog) from which the input originated.</param>
        /// <param name="value">The float value received from the pin (e.g., 0.0 or 1.0 for digital, 0.0-1.0 or 0-1023 normalized for analog).</param>
        void ProcessInput(ArduinoPin pin, float value);
    }
}
