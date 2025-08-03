// AllArduinoInputHandlers.cs
// This static class manages a collection of all active IArduinoInputHandler implementations
// within the Unity application, allowing for centralized dispatch of Arduino input data.

using System.Collections.Generic; // Required for List<T>
using UnityEngine; // Required for Debug.Log, GameObject, FindObjectOfType, AddComponent

namespace ThreeLines.IOT.Arduino
{
    /// <summary>
    /// A static class responsible for managing all active implementations of the
    /// IArduinoInputHandler interface. This provides a centralized point for
    /// registering and unregistering components that need to process data
    /// received from an Arduino, and for dispatching incoming data to them.
    /// It also ensures that an ArduinoSerialReader component exists in the scene
    /// to process incoming serial data.
    /// </summary>
    public static class AllArduinoInputHandlers
    {
        // A private static list to hold all registered IArduinoInputHandler instances.
        private static readonly List<IArduinoInputHandler> _handlers = new List<IArduinoInputHandler>();

        // A reference to the ArduinoSerialReader in the scene.
        private static ArduinoSerialReader _serialReaderInstance;

        /// <summary>
        /// Registers an IArduinoInputHandler instance.
        /// Components that implement IArduinoInputHandler should call this method
        /// typically in their OnEnable() or Start() method to register themselves.
        /// This method also ensures that an ArduinoSerialReader exists in the scene.
        /// </summary>
        /// <param name="handler">The instance of the handler to register.</param>
        public static void RegisterHandler(IArduinoInputHandler handler)
        {
            // Check if the handler is already in the list to prevent duplicate registrations.
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
                Debug.Log($"[ThreeLines.IOT.Arduino] Handler registered: {handler.GetType().Name}");

                // Ensure an ArduinoSerialReader exists in the scene
                EnsureSerialReaderExists();
            }
            else
            {
                Debug.LogWarning($"[ThreeLines.IOT.Arduino] Attempted to register handler that is already registered: {handler.GetType().Name}");
            }
        }

        /// <summary>
        /// Unregisters an IArduinoInputHandler instance.
        /// Components should call this method typically in their OnDisable() or OnDestroy()
        /// method to unregister themselves when they are no longer active.
        /// </summary>
        /// <param name="handler">The instance of the handler to unregister.</param>
        public static void UnregisterHandler(IArduinoInputHandler handler)
        {
            if (_handlers.Remove(handler))
            {
                Debug.Log($"[ThreeLines.IOT.Arduino] Handler unregistered: {handler.GetType().Name}");
            }
            else
            {
                Debug.LogWarning($"[ThreeLines.IOT.Arduino] Attempted to unregister handler that was not registered: {handler.GetType().Name}");
            }
        }

        /// <summary>
        /// Dispatches incoming Arduino input data to all currently registered handlers.
        /// This method would typically be called by the serial communication manager
        /// whenever a new data point is received from the Arduino.
        /// </summary>
        /// <param name="pin">The ArduinoPin from which the input originated.</param>
        /// <param name="value">The float value received from the pin, normalized to 0.0-1.0.</param>
        public static void ProcessAllInputs(ArduinoPin pin, float value)
        {
            // Iterate through a copy of the list to avoid issues if handlers unregister themselves
            // during the iteration (e.g., if a handler destroys itself).
            foreach (var handler in new List<IArduinoInputHandler>(_handlers))
            {
                // Ensure the handler is still valid (not null or destroyed if it's a MonoBehaviour)
                if (handler != null)
                {
                    handler.ProcessInput(pin, value);
                }
                else
                {
                    Debug.LogWarning("[ThreeLines.IOT.Arduino] Null handler found in list. It may have been destroyed without unregistering.");
                    // In a more robust system, you might want to remove null entries here
                    // _handlers.Remove(handler); // This would require careful handling of the iteration
                }
            }
        }

        /// <summary>
        /// Ensures that an ArduinoSerialReader component exists in the current scene.
        /// If one is not found, a new GameObject is created and the component is added to it.
        /// </summary>
        private static void EnsureSerialReaderExists()
        {
            // Try to find an existing instance in the scene
            if (_serialReaderInstance == null)
            {
                _serialReaderInstance = GameObject.FindObjectOfType<ArduinoSerialReader>();
            }

            // If no instance is found, create a new GameObject and add the component
            if (_serialReaderInstance == null)
            {
                GameObject managerObject = new GameObject("ArduinoSerialManager");
                _serialReaderInstance = managerObject.AddComponent<ArduinoSerialReader>();
                Debug.Log("[ThreeLines.IOT.Arduino] No ArduinoSerialReader found. Created a new GameObject 'ArduinoSerialManager' and added ArduinoSerialReader component.");
            }
        }
    }
}
