using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace GameMamager
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private readonly List<EnemyAi1> _leaders = new List<EnemyAi1>();

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go); // Зберігаємо між сценами
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterLeader(EnemyAi1 leader)
        {
            if (leader != null && !_leaders.Contains(leader) && leader.gameObject.activeInHierarchy)
            {
                _leaders.Add(leader);
            }
        }

        public void UnregisterLeader(EnemyAi1 leader)
        {
            if (leader != null)
            {
                _leaders.Remove(leader);
            }
        }

        public EnemyAi1 FindBestLeader(Vector2 position, float radius)
        {
            EnemyAi1 bestLeader = null;
            int maxFollowers = -1;
            foreach (var leader in _leaders)
            {
                if (leader != null && leader.gameObject.activeInHierarchy && leader.Followers.Count < 9 &&
                    Vector2.Distance(position, leader.transform.position) <= radius)
                {
                    if (leader.Followers.Count > maxFollowers)
                    {
                        maxFollowers = leader.Followers.Count;
                        bestLeader = leader;
                    }
                }
            }
            return bestLeader;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _leaders.Clear();
            }
        }
    }
}