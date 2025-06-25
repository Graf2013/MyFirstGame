using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class SceneController : MonoBehaviour
    {
        public string sceneName;
        
        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
