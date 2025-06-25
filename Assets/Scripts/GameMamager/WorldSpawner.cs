using System.Collections;
using System.Collections.Generic;
using Enemy;
using Map.Obstacle;
using UnityEngine;

namespace GameMamager
{
    public class WorldSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class Spawnable
        {
            public GameObject prefab;
            public float spawnChance;
        }

        [System.Serializable]
        public class Zone
        {
            public string zoneName;
            public Transform zoneCenter;
            public PolygonCollider2D zoneArea;
            public int maxEnemies;
            public int worldObjectsInZone;
            public Spawnable[] enemies;
            public Spawnable[] worldObjects;
            public float respawnDelay;
            public float playerDistanceThreshold ;
        }

        public Zone[] zones;
        public Transform player;
        public LayerMask groundLayer;

        private Dictionary<Zone, List<GameObject>> activeEnemies = new Dictionary<Zone, List<GameObject>>();
        private Dictionary<Zone, List<GameObject>> activeObjects = new Dictionary<Zone, List<GameObject>>();

        void Start()
        {
            foreach (Zone zone in zones)
            {
                activeEnemies.Add(zone, new List<GameObject>());
                activeObjects.Add(zone, new List<GameObject>());
            }
            
            SpawnWorldObjects();
            StartCoroutine(SpawnInitialEnemies());
        }

        void SpawnWorldObjects()
        {
            foreach (Zone zone in zones)
            {
                for (int i = 0; i < zone.worldObjectsInZone; i++)
                {
                    Vector2 spawnPos = GetRandomPointInZone(zone.zoneArea);
                    if (IsValidSpawnPosition(spawnPos))
                    {
                        SpawnInZone(zone.worldObjects, spawnPos, zone, false);
                    }
                }
            }
        }

        IEnumerator SpawnInitialEnemies()
        {
            foreach (Zone zone in zones)
            {
                for (int i = 0; i < zone.maxEnemies; i++)
                {
                    TrySpawnEnemy(zone);
                }
                yield return null; 
            }
        }

        public void OnEnemyDeath(GameObject enemy, Zone zone)
        {
            Debug.Log($"Enemy died in zone: {zone.zoneName}");
    
            if (activeEnemies.ContainsKey(zone) && activeEnemies[zone].Contains(enemy))
            {
                activeEnemies[zone].Remove(enemy);
                StartCoroutine(RespawnEnemy(zone));
            }
            else
            {
                Debug.LogWarning("Enemy not found in active enemies list");
            }
        }

        public void OnObjectDestroyed(GameObject destroyedObject, Zone zone)
        {
            if (activeObjects.ContainsKey(zone) && activeObjects[zone].Contains(destroyedObject))
            {
                activeObjects[zone].Remove(destroyedObject);
                StartCoroutine(RespawnObject(zone, destroyedObject.transform.position));
            }
        }

        public void RespawnEnemyInZone(Zone zone)
        {
            if (activeEnemies[zone].Count >= zone.maxEnemies) return;
    
            Vector2 spawnPos = GetRandomPointInZone(zone.zoneArea);
            if (IsValidSpawnPosition(spawnPos))
            {
                SpawnInZone(zone.enemies, spawnPos, zone, true);
            }
        }
        IEnumerator RespawnEnemy(Zone zone)
        {
            Debug.Log($"Starting respawn for zone: {zone.zoneName}, delay: {zone.respawnDelay}");
            yield return new WaitForSeconds(zone.respawnDelay);
    
            Debug.Log($"Checking player distance for zone: {zone.zoneName}");
            while (Vector2.Distance(player.position, zone.zoneCenter.position) <= zone.playerDistanceThreshold)
            {
                Debug.Log("Player too close, waiting...");
                yield return new WaitForSeconds(2f);
            }
    
            Debug.Log($"Respawning enemy in zone: {zone.zoneName}");
            TrySpawnEnemy(zone);
        }

        IEnumerator RespawnObject(Zone zone, Vector3 position)
        {
            yield return new WaitForSeconds(5f);
            Vector3 spawnPos = position + (Vector3)(Random.insideUnitCircle * 2f);
            if (IsValidSpawnPosition(spawnPos))
            {
                SpawnInZone(zone.worldObjects, spawnPos, zone, false);
            }
        }

        Vector2 GetRandomPointInZone(PolygonCollider2D collider)
        {
            Bounds bounds = collider.bounds;
            Vector2 randomPoint;
            int attempts = 30;
            
            do
            {
                randomPoint = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );
            } while (!collider.OverlapPoint(randomPoint) && --attempts > 0);
            
            return attempts > 0 ? randomPoint : collider.transform.position;
        }

        void TrySpawnEnemy(Zone zone)
        {
            Vector2 spawnPos = GetRandomPointInZone(zone.zoneArea);
            if (IsValidSpawnPosition(spawnPos))
            {
                SpawnInZone(zone.enemies, spawnPos, zone, true);
            }
        }

        void SpawnInZone(Spawnable[] items, Vector3 position, Zone zone, bool isEnemy)
        {
            if (items == null || items.Length == 0) return;

            float totalChance = 0f;
            foreach (Spawnable item in items)
            {
                totalChance += item.spawnChance;
            }

            float randomValue = Random.Range(0f, totalChance);
            float currentChance = 0f;

            foreach (Spawnable item in items)
            {
                currentChance += item.spawnChance;
                if (randomValue <= currentChance)
                {
                    GameObject spawned = Instantiate(item.prefab, position, Quaternion.identity);
                    
                    if (isEnemy)
                    {
                        activeEnemies[zone].Add(spawned);
                        EnemySpawner enemyComponent = spawned.GetComponent<EnemySpawner>();
                        if (enemyComponent != null)
                        {
                            enemyComponent.spawner = this;
                            enemyComponent.zone = zone;
                        }
                        EnemyAi2 enemyAi = spawned.GetComponent<EnemyAi2>();
                        if (enemyAi != null)
                        {
                            enemyAi.spawnZone = zone;
                        }
                    }
                    else
                    {
                        activeObjects[zone].Add(spawned);
                        Obstacle obstacleComponent = spawned.GetComponent<Obstacle>();
                        if (obstacleComponent != null)
                        {
                            obstacleComponent.spawner = this;
                            obstacleComponent.zone = zone;
                        }
                    }
                    return;
                }
            }
        }

        bool IsValidSpawnPosition(Vector2 position)
        {
            return Physics2D.OverlapCircle(position, 0.5f, groundLayer) != null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            if (zones == null) return;

            foreach (Zone zone in zones)
            {
                if (zone.zoneArea == null) continue;
                
                Vector3[] points = System.Array.ConvertAll(zone.zoneArea.points, p => (Vector3)p);
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 current = zone.zoneArea.transform.TransformPoint(points[i]);
                    Vector3 next = zone.zoneArea.transform.TransformPoint(points[(i + 1) % points.Length]);
                    Gizmos.DrawLine(current, next);
                }
            }
        }
    }
}