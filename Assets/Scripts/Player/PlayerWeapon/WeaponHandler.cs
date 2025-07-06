using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.PlayerWeapon
{
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Shooting Settings")] public GameObject bulletPrefab;
        public Transform firePoint;
        public float bulletSpeed = 10f;
        public float fireRate = 0.5f;
        public LayerMask enemyLayer = -1;

        [Header("Auto Aim Settings")] public float aimRadius = 5f;
        public float aimAngle = 45f;
        public GameObject crosshairPrefab;

        [Header("Camera Shake")] public Camera playerCamera;
        public float shakeIntensity = 0.1f;
        public float shakeDuration = 0.2f;

        public int maxAmmo;
        public int currentAmmo;
        public float reloadTime;
        private bool isReloading;
        public bool isControlledByPlayer = true;


        private InputAction attackAction;
        private Vector2 attackDirection;
        private float lastFireTime;
        private GameObject currentCrosshair;
        private Transform currentTarget;
        private Vector3 originalCameraPosition;

        private void Start()
        {
            attackAction = InputSystem.actions.FindAction("Attack");

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (playerCamera != null)
                originalCameraPosition = playerCamera.transform.localPosition;

            currentAmmo = maxAmmo;
        }

        private void Update()
        {
            attackDirection = attackAction.ReadValue<Vector2>();

            UpdateAiming();

            if (isControlledByPlayer)
            {
                if (currentAmmo > 0 && !isReloading)
                {
                    if (attackDirection.magnitude > 0.1f && Time.time >= lastFireTime + fireRate)
                    {
                        Shoot();
                        currentAmmo--;
                        lastFireTime = Time.time;
                    }
                }
                else if (currentAmmo == 0 && !isReloading)
                {
                    StartCoroutine(Reload());
                }
            }
        }

        private void UpdateAiming()
        {
            if (attackDirection.magnitude > 0.1f)
            {
                Transform target = FindNearestEnemyInDirection(attackDirection);

                if (target != currentTarget)
                {
                    currentTarget = target;
                    UpdateCrosshair();
                }
            }
            else
            {
                if (currentCrosshair != null)
                {
                    Destroy(currentCrosshair);
                    currentCrosshair = null;
                }

                currentTarget = null;
            }
        }

        private Transform FindNearestEnemyInDirection(Vector2 direction)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, aimRadius, enemyLayer);

            if (enemies.Length == 0) return null;

            Vector2 playerPos = transform.position;
            Transform bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var enemy in enemies)
            {
                if (enemy.GetComponent<Test.TestEnemy>() == null &&
                    enemy.GetComponent<Enemy.EnemyHealthSystem>() == null)
                    continue;

                Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;

                float angle = Vector2.Angle(direction, dirToEnemy);
                if (angle <= aimAngle / 2f)
                {
                    float distance = Vector2.Distance(playerPos, enemy.transform.position);

                    float angleScore = (aimAngle / 2f - angle) / (aimAngle / 2f);
                    float distanceScore = (aimRadius - distance) / aimRadius;

                    float totalScore = angleScore * 2f + distanceScore;

                    if (totalScore > bestScore)
                    {
                        bestScore = totalScore;
                        bestTarget = enemy.transform;
                    }
                }
            }

            return bestTarget;
        }

        private void UpdateCrosshair()
        {
            if (currentCrosshair != null)
            {
                Destroy(currentCrosshair);
                currentCrosshair = null;
            }

            if (currentTarget != null && crosshairPrefab != null)
            {
                currentCrosshair = Instantiate(crosshairPrefab, currentTarget.position, Quaternion.identity);
                currentCrosshair.transform.SetParent(currentTarget);
                currentCrosshair.transform.localPosition = Vector3.zero + new Vector3(0, -0.3f, 0);
            }
        }

        private void Shoot()
        {
            Vector2 shootDirection;

            if (currentTarget != null)
            {
                shootDirection = (currentTarget.position - firePoint.position).normalized;
            }
            else
            {
                shootDirection = attackDirection.normalized;
            }

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = Vector2.zero;

                float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
                bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            StartCoroutine(CameraShake());
        }

        private IEnumerator CameraShake()
        {
            if (playerCamera == null) yield break;

            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-shakeIntensity, shakeIntensity);
                float y = Random.Range(-shakeIntensity, shakeIntensity);

                playerCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            playerCamera.transform.localPosition = originalCameraPosition;
        }

        private IEnumerator Reload()
        {
            isReloading = true;
            yield return new WaitForSeconds(reloadTime);
            lastFireTime = Time.time;
            currentAmmo = maxAmmo;
            isReloading = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aimRadius);

            if (attackDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.red;
                Vector3 direction = attackDirection.normalized;
                Vector3 leftBoundary = Quaternion.AngleAxis(-aimAngle / 2f, Vector3.forward) * direction;
                Vector3 rightBoundary = Quaternion.AngleAxis(aimAngle / 2f, Vector3.forward) * direction;

                Gizmos.DrawRay(transform.position, leftBoundary * aimRadius);
                Gizmos.DrawRay(transform.position, rightBoundary * aimRadius);
            }
        }
    }
}