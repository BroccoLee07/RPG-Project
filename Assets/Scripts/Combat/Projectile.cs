using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using UnityEngine;

namespace RPG.Combat {
    public class Projectile : MonoBehaviour {
        [SerializeField] private float speed = 5f;
        // [SerializeField] private float damage = 1f;
        private Health target;
        private float weaponDamage = 0;

        public void Initialize(Health targetHealth, float weaponDamage) {
            target = targetHealth;
            this.weaponDamage = weaponDamage;
        }

        void Update() {
            if (target == null) return;

            Vector3 targetPos = GetAimLocation(target);
            // Debug.Log($"targetPos: {targetPos}");

            transform.LookAt(targetPos);
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private Vector3 GetAimLocation(Health target) {
            CapsuleCollider targetCapsuleCollider = target.GetComponent<CapsuleCollider>();
            if (targetCapsuleCollider == null) {
                return target.transform.position;
            }

            // Get position of target and random y position of target's upper half and below top of head
            return target.transform.position + Vector3.up * ((targetCapsuleCollider.height / 2) + Random.Range(0, targetCapsuleCollider.height / 3));
        }

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponent<Health>() != target) return;

            // Damage will only be based on weapon's damage for now
            // In this case, the projectile contains the weapon data including damage
            // target.TakeDamage(weaponDamage + damage);
            target.TakeDamage(weaponDamage);
            Destroy(gameObject);
        }
    }
}

