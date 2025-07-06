using UnityEngine;

namespace GameMamager
{
    public class DestroyObject : MonoBehaviour
    {
        void Start()
        {
            GameObject.Destroy(gameObject, 5);
        }
    
    }
}
