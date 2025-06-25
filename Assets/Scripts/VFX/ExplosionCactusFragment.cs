using UnityEngine;

namespace VFX
{
    public class ExplosionCactusFragment : MonoBehaviour
    {
        [SerializeField] private GameObject[] fragments;
        [SerializeField] private float forceVectorX, forceVectorY;
        private Vector2 _rand;
        private void Start()
        {
            Explosion();
        }
        public void Explosion()
        {
            //Для кожного фрагмента генеруємо випадковий вектор переміщення якому далі задаємо силу
            foreach (var fragment in fragments)
            {
                _rand = new Vector2(Random.Range(-forceVectorX, forceVectorX), Random.Range(-forceVectorY, forceVectorY));
                Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
                rb.AddForce(_rand, ForceMode2D.Impulse);
                rb.angularDamping = 1;
                rb.linearDamping = 1;
            }
        }

    }
}
