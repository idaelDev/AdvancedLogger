using UnityEngine;
using UnityEngine.InputSystem;
using static IdaelDev.AdvancedLogger.Log;

namespace IdaelDev.AdvancedLogger.Samples
{
    public class LoggingExample : MonoBehaviour
    {
        private void Start()
        {
            Debug("This is a debug message");
            Info("This is an info message");
            Warning("This is a warning message");
            Error("This is an error message");

            Info($"Player health: {100}");
            Info($"Player position: {transform.position}");

            // Exception Simulation
            try
            {
                throw new System.Exception("Something went wrong!");
            }
            catch (System.Exception ex)
            {
                Exception(ex);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Info("Space key pressed!");
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Error("Error key pressed!");
            }
        }

        private void OnDestroy()
        {
            Warning("LoggingExample component destroyed");
        }
    }
}
