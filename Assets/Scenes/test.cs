using UnityEngine;

public class test : MonoBehaviour
{
    float timer = 0;
    void Start()
    {
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer <= 5 )
        {
            Debug.Log("true");
            
        } else {
            Debug.Log("false");
            timer = 0;
        }
    }
}
