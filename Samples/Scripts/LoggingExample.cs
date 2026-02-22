using UnityEngine;
using UnityEngine.InputSystem;
using static IdaelDev.AdvancedLogger.Log;

namespace IdaelDev.AdvancedLogger.Samples
{
    /// <summary>
    /// Exemple d'utilisation du système de logging amélioré
    /// </summary>
    public class LoggingExample : MonoBehaviour
    {
        private void Start()
        {
            // Logs de différents niveaux
            Debug("This is a debug message");
            Info("This is an info message");
            Warning("This is a warning message");
            Error("This is an error message");

            // Test avec différents types
            Info($"Player health: {100}");
            Info($"Player position: {transform.position}");

            // Simulation d'erreur
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

            // Log en continu (pour tester le filtrage)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Info("Space key pressed!");
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Error("Error key pressed!");
            }
        }

        private void TestMethod()
        {
            // Le nom de la classe (LoggingExample) sera automatiquement ajouté en préfixe
            Info("This message will be prefixed with [LoggingExample]");
        }

        private void OnDestroy()
        {
            Warning("LoggingExample component destroyed");
        }
    }
}
