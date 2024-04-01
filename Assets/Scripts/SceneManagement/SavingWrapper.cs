using System;
using System.Collections;
using RPG.Saving;
using UnityEngine;

namespace RPG.SceneManagement {
    [RequireComponent(typeof(JsonSavingSystem))]
    public class SavingWrapper : MonoBehaviour {

        [SerializeField] private float fadeInTime = 0.2f;
        private JsonSavingSystem savingSystem;

        const string defaultSaveFile = "gameSave";

        void Awake() {
            StartCoroutine(LoadLastScene());
        }
        
        private IEnumerator LoadLastScene() {
            savingSystem = GetComponent<JsonSavingSystem>();
            yield return savingSystem.LoadLastScene(defaultSaveFile);

            // Make sure Awakes have happened from yield return LoadLastScene before trying to find the object
            SceneFader fader = FindObjectOfType<SceneFader>();            
            fader.FadeOutImmediate();
            yield return fader.FadeIn(fadeInTime);
        }

        void Update() {
            // Load save state with L key
            if (Input.GetKeyDown(KeyCode.L)) {
                Load();
            }

            // Save game state with S key
            if (Input.GetKeyDown(KeyCode.S)) {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.Delete)) {
                Delete();
            }
        }

        public void Save() {
            savingSystem.Save(defaultSaveFile);
        }

        public void Load() {
            savingSystem.Load(defaultSaveFile);
        }

        public void Delete() {
            savingSystem.Delete(defaultSaveFile);
        }
    }
}
