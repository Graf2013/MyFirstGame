using UnityEngine;

namespace GameMamager
{
    public class DestroyObject : MonoBehaviour
    {
        void Update()
        {
            GameObject.Destroy(gameObject, 5);
        }
    
    }
}
