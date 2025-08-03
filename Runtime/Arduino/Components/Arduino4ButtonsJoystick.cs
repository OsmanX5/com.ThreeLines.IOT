using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



namespace ThreeLines.IOT.Arduino
{
    public class Arduino4ButtonsJoystick : MonoBehaviour , IArduinoInputHandler
    {
        [SerializeField]
        ArduinoPin upPin,
            downPin,
            leftPin,
            rightPin;

        [ShowInInspector]
        [ReadOnly]
        ArduinoDigitalButtonLogic upButtonLogic =new(),
            downButtonLogic = new(),
            leftButtonLogic = new(),
            rightButtonLogic = new();

        ButtonProcessResult upButtonResult,
            downButtonResult,
            leftButtonResult,
            rightButtonResult;

        Vector2 lastJoyStickValue;
        public UnityEvent<Vector2> OnJoystickMovedUnityEvent;

        private void OnEnable()
        {
            AllArduinoInputHandlers.RegisterHandler(this);
        }
        private void OnDisable()
        {
            AllArduinoInputHandlers.UnregisterHandler(this);
        }
        public void ProcessInput(ArduinoPin pin, float value)
        {
            Debug.Log($"Proocessing pin{pin} for joystick");
            switch (pin)
            {
                case var p when p == upPin:
                    upButtonResult = upButtonLogic.ProcessInput(value);
                    break;
                case var p when p == downPin:
                    downButtonResult = downButtonLogic.ProcessInput(value);
                    break;
                case var p when p == leftPin:
                    leftButtonResult =leftButtonLogic.ProcessInput(value);
                    break;
                case var p when p == rightPin:
                    rightButtonResult =rightButtonLogic.ProcessInput(value);
                    break;
            }
            var joyStickValue = GetJoystickDirection();
            if (joyStickValue != lastJoyStickValue)
            {
                lastJoyStickValue = joyStickValue;
                OnJoystickMovedUnityEvent?.Invoke(joyStickValue);
            }
        }
        public Vector2 GetJoystickDirection()
        {
            float x = 0.0f;
            float y = 0.0f;
            if (upButtonResult.WasPressed)
                y = 1.0f;
            else if (downButtonResult.WasPressed)
                y = -1.0f;
            if (leftButtonResult.WasPressed)
                x = -1.0f;
            else if (rightButtonResult.WasPressed)
                x = 1.0f;
            Vector2 joystickValue = new Vector2(x, y);

            return joystickValue;
        }

    }
}

