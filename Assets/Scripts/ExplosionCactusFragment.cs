using UnityEngine;

public class ExplosionCactusFragment : MonoBehaviour
{
    [SerializeField] private GameObject[] fragments;
    [SerializeField] private float forceVectorX, forceVectorY;
    Vector2 _rand;
    void Start()
    {
        Explosion();
    }
    public void Explosion()
    {
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
