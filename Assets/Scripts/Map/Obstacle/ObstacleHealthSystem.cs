using UnityEngine;

namespace Map.Obstacle
{
    class ObstacleHealthSystem : MonoBehaviour
    {
        private GameObject _healthBarPrefab;
        public Vector3 offset = new Vector3(0f, 0.5f, 0f);
        public Vector3 scale = new Vector3(4f, 3f, 1f);

    
        private void Awake()
        {
            _healthBarPrefab = Resources.Load<GameObject>("Prefabs/ObstacleHealthBar");
            _healthBarPrefab.transform.localScale = scale;

            Instantiate(_healthBarPrefab, transform.position + offset, Quaternion.identity, this.transform);
        }



    }
}