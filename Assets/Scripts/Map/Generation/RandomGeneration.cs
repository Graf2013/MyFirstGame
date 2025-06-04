using UnityEngine;

namespace Map.Generation
{
    public class RandomGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject[] objects;

        private void Start()
        {
            int rand = Random.Range(0, objects.Length);
            Instantiate(objects[rand], transform.position, Quaternion.identity);
        }
    }
}