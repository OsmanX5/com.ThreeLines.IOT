// ArduinoEnums.cs
// This script defines essential enums for interacting with Arduino pins and their modes.

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// Represents common Arduino digital and analog pin names.
    /// This enum can be extended to include more specific pins for different Arduino boards
    /// (e.g., Mega, ESP32) as the package evolves.
    /// </summary>
    public enum ArduinoPin
    {
        // Digital Pins
        D0 = 0,   // RX
        D1 = 1,   // TX
        D2 = 2,
        D3 = 3,   // PWM
        D4 = 4,
        D5 = 5,   // PWM
        D6 = 6,   // PWM
        D7 = 7,
        D8 = 8,
        D9 = 9,   // PWM
        D10 = 10, // PWM, SS
        D11 = 11, // PWM, MOSI
        D12 = 12, // MISO
        D13 = 13, // SCK, LED_BUILTIN

        // Analog Pins
        A0 = 14, // Corresponds to digital pin 14
        A1 = 15, // Corresponds to digital pin 15
        A2 = 16, // Corresponds to digital pin 16
        A3 = 17, // Corresponds to digital pin 17
        A4 = 18, // Corresponds to digital pin 18 (SDA)
        A5 = 19  // Corresponds to digital pin 19 (SCL)
    }

    /// <summary>
    /// Represents the state for digital inputs, particularly for pull-up/pull-down resistors.
    /// </summary>
    public enum PullMode
    {
        /// <summary>
        /// No pull-up or pull-down resistor is active.
        /// </summary>
        None,
        /// <summary>
        /// An internal pull-up resistor is enabled.
        /// </summary>
        PullUp,
        /// <summary>
        /// An internal pull-down resistor is enabled. (Note: Not all Arduino boards support internal pull-down).
        /// </summary>
        PullDown
    }

    /// <summary>
    /// A generic enum to represent an On/Off or Enabled/Disabled state.
    /// Can be used for various boolean-like settings.
    /// </summary>
    public enum OnOffState
    {
        Off,
        On
    }
}
