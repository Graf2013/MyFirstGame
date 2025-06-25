using UnityEngine;
using UnityEngine.UI;

namespace GameMamager
{
    public class DynamicResolution : MonoBehaviour
    {
        public Text screenText;
        [Range(0f, 1f)]
        public float resolutionScale = 0.7f;
        
        private static Vector2 originalResolution;
        private static bool isInitialized = false;

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
            
            if (!isInitialized)
            {
                originalResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
                isInitialized = true;
            }
            
            Vector2 scaledRes = originalResolution * resolutionScale;
            Screen.SetResolution((int)scaledRes.x, (int)scaledRes.y, true);
        }

        void Update()
        {
            screenText.text = $"Resolution: {Screen.width} x {Screen.height} | Scale: {resolutionScale}";
        }
    }
}