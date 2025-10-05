using UnityEngine;
using UnityEditor;
using ThreeLines.IOT.Arduino;

namespace ThreeLines.IOT.Editor
{
    /// <summary>
    /// Unity Editor script that adds ThreeLinesIOT creation options to the right-click context menu in the Hierarchy.
    /// Provides quick creation of GameObjects with Arduino components attached.
    /// </summary>
    public static class ThreeLinesIOTCreationEditor
    {
        #region Menu Item Constants
        private const string MENU_ROOT = "GameObject/ThreeLinesIOT/";
        private const int MENU_PRIORITY = 0;
        #endregion

        #region Arduino Button Creation
        /// <summary>
        /// Creates a new GameObject with an ArduinoButton component attached.
        /// Accessible via right-click context menu in Hierarchy: GameObject > ThreeLinesIOT > Create ArduinoButton
        /// </summary>
        [MenuItem(MENU_ROOT + "Create ArduinoButton", false, MENU_PRIORITY)]
        public static void CreateArduinoButton()
        {
            // Create new GameObject
            GameObject newGameObject = new GameObject("ArduinoButton");
            
            // Add ArduinoButton component
            ArduinoButton arduinoButton = newGameObject.AddComponent<ArduinoButton>();
            
            // Set default values for better usability
            arduinoButton.targetPin = ArduinoPin.D2; // Default to digital pin 2
            arduinoButton.pressThreshold = 0.5f;
            
            // Position in hierarchy
            SetupGameObjectInHierarchy(newGameObject, "ArduinoButton");
            
            // Log creation
            Debug.Log($"[ThreeLinesIOT] Created ArduinoButton GameObject: {newGameObject.name}");
        }

        /// <summary>
        /// Validates if the ArduinoButton creation menu item should be enabled.
        /// </summary>
        [MenuItem(MENU_ROOT + "Create ArduinoButton", true)]
        public static bool ValidateCreateArduinoButton()
        {
            // Always allow creation
            return true;
        }
        #endregion

        #region Arduino 4 Buttons Joystick Creation
        /// <summary>
        /// Creates a new GameObject with an Arduino4ButtonsJoystick component attached.
        /// Accessible via right-click context menu in Hierarchy: GameObject > ThreeLinesIOT > Create Arduino4ButtonsJoystick
        /// </summary>
        [MenuItem(MENU_ROOT + "Create Arduino4ButtonsJoystick", false, MENU_PRIORITY + 1)]
        public static void CreateArduino4ButtonsJoystick()
        {
            // Create new GameObject
            GameObject newGameObject = new GameObject("Arduino4ButtonsJoystick");
            
            // Add Arduino4ButtonsJoystick component
            Arduino4ButtonsJoystick joystick = newGameObject.AddComponent<Arduino4ButtonsJoystick>();
            
            // Note: Pin configuration will need to be set manually by the user
            // as the pins are private SerializeField in the original component
            
            // Position in hierarchy
            SetupGameObjectInHierarchy(newGameObject, "Arduino4ButtonsJoystick");
            
            // Log creation
            Debug.Log($"[ThreeLinesIOT] Created Arduino4ButtonsJoystick GameObject: {newGameObject.name}");
            Debug.Log($"[ThreeLinesIOT] Remember to configure the Up, Down, Left, and Right pins in the inspector.");
        }

        /// <summary>
        /// Validates if the Arduino4ButtonsJoystick creation menu item should be enabled.
        /// </summary>
        [MenuItem(MENU_ROOT + "Create Arduino4ButtonsJoystick", true)]
        public static bool ValidateCreateArduino4ButtonsJoystick()
        {
            // Always allow creation
            return true;
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Sets up the created GameObject in the hierarchy with proper selection and undo support.
        /// </summary>
        /// <param name="gameObject">The GameObject to setup</param>
        /// <param name="componentName">Name of the component for undo operation</param>
        private static void SetupGameObjectInHierarchy(GameObject gameObject, string componentName)
        {
            // Ensure the GameObject is placed in the currently selected parent (if any)
            Transform selectedTransform = Selection.activeTransform;
            if (selectedTransform != null)
            {
                gameObject.transform.SetParent(selectedTransform);
            }

            // Reset transform to default values
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            // Register undo operation
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {componentName}");
            
            // Select the new GameObject
            Selection.activeGameObject = gameObject;
            
            // Ensure the GameObject is visible in the Scene view
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }
        #endregion

        #region Menu Item Priorities
        // Unity menu priorities:
        // - Priority 0 places our items at the top of the GameObject menu
        // - Standard GameObjects (Cube, Sphere, etc.) use priority 10
        // - UI elements use priority 30
        // - We use priority 0 to place our ThreeLinesIOT items at the very top
        #endregion
    }
}