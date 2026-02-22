using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IdaelDev.AdvancedLogger
{
    /// <summary>
    /// Console in-game pour visualiser les logs
    /// </summary>
    public class InGameConsole : MonoBehaviour
    {

        [Header("Configuration")]
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // Touche ~ (backtick)
        [SerializeField] private bool startVisible;

        [Header("Apparence")]
        [SerializeField] private int maxVisibleLogs = 100;
        [SerializeField] private int fontSize = 14;

        private bool _isVisible;
        private Vector2 _scrollPosition;
        private ELogLevel _filterLevel = ELogLevel.Debug;
        private string _searchFilter = "";
        private GUIStyle _backgroundStyle;
        private GUIStyle _logStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private bool _stylesInitialized;

        private void Awake()
        {
            _isVisible = startVisible;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (IsTogglePressed())
            {
                _isVisible = !_isVisible;
            }
        }


        private void OnGUI()
        {
            if (!_isVisible) return;

            InitializeStyles();
            DrawConsole();
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _backgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0, 0, 0, 0.9f)) }
            };

            _logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(5, 5, 2, 2)
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize + 2,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                padding = new RectOffset(10, 10, 5, 5)
            };

            _stylesInitialized = true;
        }

        private void DrawConsole()
        {
            var width = Screen.width * 0.8f;
            var height = Screen.height * 0.6f;
            var x = (Screen.width - width) / 2;
            var y = (Screen.height - height) / 2;

            GUILayout.BeginArea(new Rect(x, y, width, height), _backgroundStyle);

            DrawHeader();
            DrawFilters();
            DrawLogsList();

            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Game Console", _headerStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button($"Close ({toggleKey})", _buttonStyle, GUILayout.Width(120)))
            {
                _isVisible = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void DrawFilters()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            // Ligne 1 : Recherche
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(60));
            _searchFilter = GUILayout.TextField(_searchFilter);
            if (GUILayout.Button("Clear Search", GUILayout.Width(100)))
            {
                _searchFilter = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Ligne 2 : Filtres de niveau
            GUILayout.BeginHorizontal();
            GUILayout.Label("Level:", GUILayout.Width(60));

            if (GUILayout.Toggle(_filterLevel == ELogLevel.Debug, "Debug", _buttonStyle))
                _filterLevel = ELogLevel.Debug;
            if (GUILayout.Toggle(_filterLevel == ELogLevel.Info, "Info", _buttonStyle))
                _filterLevel = ELogLevel.Info;
            if (GUILayout.Toggle(_filterLevel == ELogLevel.Warning, "Warning", _buttonStyle))
                _filterLevel = ELogLevel.Warning;
            if (GUILayout.Toggle(_filterLevel == ELogLevel.Error, "Error", _buttonStyle))
                _filterLevel = ELogLevel.Error;
            if (GUILayout.Toggle(_filterLevel == ELogLevel.Fatal, "Fatal", _buttonStyle))
                _filterLevel = ELogLevel.Fatal;

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Ligne 3 : Actions
            GUILayout.BeginHorizontal();

            var logs = GetFilteredLogs();
            GUILayout.Label($"Total: {logs.Count} logs", GUILayout.Width(150));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear Console", _buttonStyle, GUILayout.Width(120)))
            {
                Log.Clear();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawLogsList()
        {
            var logs = GetFilteredLogs();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.ExpandHeight(true));

            if (logs.Count == 0)
            {
                GUILayout.Label("No logs to display", _logStyle);
            }
            else
            {
                // Limiter le nombre de logs affichés pour les performances
                var visibleLogs = logs.Skip(Mathf.Max(0, logs.Count - maxVisibleLogs)).ToList();

                foreach (var log in visibleLogs)
                {
                    DrawLogEntry(log);
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawLogEntry(LogEntry log)
        {
            var color = log.GetColor();
            var colorHex = ColorUtility.ToHtmlStringRGB(color);

            var timeStr = log.Timestamp.ToString("HH:mm:ss");
            var levelStr = log.Level.ToString().ToUpper();
            var message = $"<color=#{colorHex}>[{timeStr}] [{levelStr}] {log.GetFormattedMessage()}</color>";

            GUILayout.Label(message, _logStyle);

            // Si on clique sur un log, afficher les détails
            if (GUILayout.Button("Details", GUILayout.Width(80)))
            {
                Debug.Log(log.GetDetailedMessage());
                if (!string.IsNullOrEmpty(log.StackTrace))
                {
                    Debug.Log(log.StackTrace);
                }
            }

            GUILayout.Space(2);
        }

        private List<LogEntry> GetFilteredLogs()
        {
            var logs = Log.History.ToList();

            // Filtrer par niveau (afficher ce niveau et tous ceux au-dessus)
            logs = logs.Where(l => l.Level >= _filterLevel).ToList();

            // Filtrer par recherche
            if (!string.IsNullOrEmpty(_searchFilter))
            {
                logs = logs.Where(l =>
                    l.Message.ToLower().Contains(_searchFilter.ToLower()) ||
                    l.CallerClass.ToLower().Contains(_searchFilter.ToLower())
                ).ToList();
            }

            return logs;
        }

        private Texture2D MakeTex(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }


        private bool IsTogglePressed()
        {
            #if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
                return false;

            // Conversion KeyCode → Key
            if (System.Enum.TryParse(toggleKey.ToString(), out Key newKey))
            {
                return Keyboard.current[newKey].wasPressedThisFrame;
            }

            return false;

            #elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(toggleKey);

            #else
            return false;
            #endif
        }
    }
}
