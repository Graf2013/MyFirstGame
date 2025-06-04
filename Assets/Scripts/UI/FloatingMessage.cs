using TMPro;
using UnityEngine;

namespace UI
{
    public class FloatingMassega : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private TextMeshPro _damageValue;

        [SerializeField] private float initialYVelocity = 7f;
        [SerializeField] private float initialXVelocityRange = 3f;
        [SerializeField] private float lifeTime = 0.8f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _damageValue = GetComponentInChildren<TextMeshPro>();
        }
    
        void Start()
        {
            _rb.linearVelocity = new Vector2(Random.Range(-initialXVelocityRange, initialXVelocityRange), initialYVelocity);
            Destroy(gameObject, lifeTime);
        }

        public void SetMessage(string message)
        {
            _damageValue.SetText(message);
        }
    }
}
