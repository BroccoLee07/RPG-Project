using GameDevTV.Utils;
using Newtonsoft.Json.Linq;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Attributes {
    [RequireComponent(typeof(ActionScheduler))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(BaseStats))]
    public class Health : MonoBehaviour, IJsonSaveable {

        [Tooltip("How much health is regenerated on level up")]
        [SerializeField] private float levelUpRegenerationPercentage = 15;
        [SerializeField] private LazyValue<float> healthPoints;
        [SerializeField] private UnityEvent<float> takeDamageEvent;
        [SerializeField] private UnityEvent<bool> postDeathAction;
        [SerializeField] private UnityEvent onDeath;
        [SerializeField] private UnityEvent onRevive;

        private bool isDead = false;
        private BaseStats baseStats;

        private const string ANIMATOR_DIE_TRIGGER = "die";

        // Property
        public bool IsDead { get => isDead; }
        public float CurrentHealthPoints { get => healthPoints.value; }
        public float MaxHealthPoints { get => baseStats.GetStat(Stat.Health); }

        void Awake() {
            baseStats = GetComponent<BaseStats>();
            // Makes sure correct value is retrieved
            healthPoints = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth() {
            return baseStats.GetStat(Stat.Health);
        }

        void Start() {
            // If health was not initialized prior to this point, make sure it is initialized
            healthPoints.ForceInit();
            Debug.Log($"{gameObject.name} health after force init: {healthPoints.value}");
            Revive();
            postDeathAction.Invoke(false);
        }

        private void OnEnable() {
            if (baseStats != null) {
                // Subscribe to onLevelUp event of BaseStats to health regen code
                baseStats.onLevelUp += LevelUpRegenerateHealth;
            }
        }

        private void OnDisable() {
            if(baseStats != null) {
                // Subscribe to onLevelUp event of BaseStats to health regen code
                baseStats.onLevelUp -= LevelUpRegenerateHealth;
            }
        }

        public void TakeDamage(GameObject attackInitiator, float damage) {
            if (IsDead) return;

            // To avoid the health going below 0
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            takeDamageEvent.Invoke(damage);

            UpdateHealthState();

            if (isDead) {
                onDeath.Invoke();
                AwardExperience(attackInitiator);
                postDeathAction.Invoke(true);
            }
        }

        public void Heal(float healValue) {
            healthPoints.value = Mathf.Min(healthPoints.value + healValue, MaxHealthPoints);
        }

        public float GetPercentage() {
            return 100 * GetFraction();
        }

        public float GetFraction() {
            return healthPoints.value / baseStats.GetStat(Stat.Health);
        }

        private void LevelUpRegenerateHealth() {
            // Heal up a percentage of new max health on level up
            healthPoints.value += baseStats.GetStat(Stat.Health) * (levelUpRegenerationPercentage / 100);
        }

        private void UpdateHealthState() {
            if (healthPoints.value <= 0) {
                isDead = true;
                Die();                
            }
        }

        private void AwardExperience(GameObject attackInitiator) {
            Experience exp = attackInitiator.GetComponent<Experience>();
            if (exp == null) return;

            exp.GainExperience(baseStats.GetStat(Stat.ExperienceReward));
        }

        private void Die() {
            GetComponent<Animator>().SetTrigger(ANIMATOR_DIE_TRIGGER);
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void Revive() {
            isDead = false;
            Animator animator = GetComponent<Animator>();
            animator.Rebind();
            animator.Update(0f);

            onRevive.Invoke();
        }

        public JToken CaptureAsJToken() {
            return JToken.FromObject(healthPoints.value);
        }

        public void RestoreFromJToken(JToken state) {
            healthPoints.value = state.ToObject<float>();
            Debug.Log($"{gameObject.name} health on load: {healthPoints.value}");
            UpdateHealthState();
            Revive();
            postDeathAction.Invoke(false);            
        }

    }
}

