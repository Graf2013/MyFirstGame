using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    void Update()
    {
        GameObject.Destroy(gameObject, 5);
    }
    
}
