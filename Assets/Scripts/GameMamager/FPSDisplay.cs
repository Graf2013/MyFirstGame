using UnityEngine;

namespace GameMamager
{
    public class FPSDisplay : MonoBehaviour
    {
        [Header("Налаштування відображення")]
        [SerializeField] private int fontSize = 20;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private TextAnchor textAlignment = TextAnchor.UpperLeft;
        [SerializeField] private bool showMilliseconds = true;
        [SerializeField] private bool showAverage = true;
    
        [Header("Позиція на екрані")]
        [SerializeField] [Range(0, 1)] private float horizontalPosition = 0.01f;
        [SerializeField] [Range(0, 1)] private float verticalPosition = 0.01f;

        private float _deltaTime;
        private float _averageFps;
        private int _frameCount;
        private float _timeElapsed;
        private GUIStyle _style;

        private void Start()
        {
            // Створюємо стиль для тексту
            _style = new GUIStyle
            {
                alignment = textAlignment,
                fontSize = fontSize
            };
            _style.normal.textColor = textColor;
        }

        private void Update()
        {
            // Оновлюємо значення FPS
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        
            // Розраховуємо середній FPS
            _frameCount++;
            _timeElapsed += Time.unscaledDeltaTime;
        
            if (_timeElapsed >= 0.5f) // Оновлюємо кожні 0.5 секунд
            {
                _averageFps = _frameCount / _timeElapsed;
                _frameCount = 0;
                _timeElapsed = 0;
            }
        }

        private void OnGUI()
        {
            // Розраховуємо поточний FPS
            float fps = 1.0f / _deltaTime;
        
            // Форматуємо текст
            string fpsText = $"FPS: {fps:0.}";
        
            if (showMilliseconds)
            {
                float ms = _deltaTime * 1000.0f;
                fpsText += $"\nMS: {ms:0.0}";
            }
        
            if (showAverage)
            {
                fpsText += $"\nAvg: {_averageFps:0.}";
            }
        
            // Розраховуємо позицію
            float x = horizontalPosition * Screen.width;
            float y = verticalPosition * Screen.height;
            Rect rect = new Rect(x, y, Screen.width, Screen.height);
        
            // Відображаємо текст
            GUI.Label(rect, fpsText, _style);
        }
    }
}