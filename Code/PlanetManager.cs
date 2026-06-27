using System;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

namespace ModernBox
{
    public class PlanetManager : MonoBehaviour
    {
        public static PlanetManager instance;
        private string currentPlanetName;
        private string currentPlanetType;
        private const string planetFileName = "currentPlanet.txt";
        private const string planetFileNameTypeWHyAmIStoringItLikeThis = "currentPlanetType.txt";
        private string planetFilePath;
        private string planetTypeFilePath;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                planetFilePath = Path.Combine(Application.persistentDataPath, "modernbox", planetFileName);
                planetTypeFilePath = Path.Combine(Application.persistentDataPath, "modernbox", planetFileNameTypeWHyAmIStoringItLikeThis);
                LoadPlanetName();
                LoadPlanetType();
            }
            else
            {
                Destroy(gameObject);
            }
            if (string.IsNullOrEmpty(currentPlanetName))
            {
          //      SpaceManager.startup = true;
          //      SpaceManager.EnableSpace();
            }
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(currentPlanetName))
            {
                StartCoroutine(WaitForPlanetName());
            }
            if (!string.IsNullOrEmpty(currentPlanetType))
            {
                StartCoroutine(WaitForPlanetType());
            }
        }
		
		public string FindParentStar()
        {
            string galaxyPath = Path.Combine(Application.persistentDataPath, "modernbox");
            string foundStar = null;

            if (!Directory.Exists(galaxyPath))
            {
                Debug.LogError("Galaxy path not found.");
                return null;
            }

            foreach (var galaxyDir in Directory.GetDirectories(galaxyPath))
            {
                string galaxiesFolderPath = Path.Combine(galaxyDir, "Galaxies");

                if (!Directory.Exists(galaxiesFolderPath))
                    continue;

                foreach (var galaxyFolder in Directory.GetDirectories(galaxiesFolderPath))
                {
                    foreach (var starFolder in Directory.GetDirectories(galaxyFolder))
                    {
                        string starJsonPath = Path.Combine(starFolder, "star.json");

                        if (File.Exists(starJsonPath))
                        {
                            string starJsonContent = File.ReadAllText(starJsonPath);

                            if (starJsonContent.Contains(currentPlanetName))
                            {
                                foundStar = Path.GetFileName(starFolder);
                                Debug.Log("Found parent star: " + foundStar);
                                return foundStar;
                            }
                        }
                    }
                }
            }

            if (foundStar == null)
            {
                Debug.LogError("Parent star not found for planet: " + currentPlanetName);
            }

            return foundStar;
        }

        private void LoadPlanetName()
        {
            if (File.Exists(planetFilePath))
            {
                currentPlanetName = File.ReadAllText(planetFilePath);
                Debug.Log("Loaded current planet: " + currentPlanetName);
            }
        }

        private void LoadPlanetType()
        {
            if (File.Exists(planetTypeFilePath))
            {
                currentPlanetType = File.ReadAllText(planetTypeFilePath);
                Debug.Log("Loaded current planet type: " + currentPlanetType);
            }
        }

        private void SavePlanetName(string planetName)
        {
            File.WriteAllText(planetFilePath, planetName);
            currentPlanetName = planetName;
            Debug.Log("Saved current planet: " + currentPlanetName);
        }

        private void SavePlanetType(string planetType)
        {
            File.WriteAllText(planetTypeFilePath, planetType);
            currentPlanetType = planetType;
            Debug.Log("Saved current planet type: " + currentPlanetType);
        }

        private IEnumerator WaitForPlanetName()
        {
            while (string.IsNullOrEmpty(currentPlanetName))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator WaitForPlanetType()
        {
            while (string.IsNullOrEmpty(currentPlanetType))
            {
                yield return new WaitForSeconds(1f);
            }
        }

        public void SetCurrentPlanet(string planetName)
        {
            SavePlanetName(planetName);
        }

        public string GetCurrentPlanet()
        {
            return currentPlanetName;
        }

        public string GetCurrentPlanetType()
        {
            return currentPlanetType;
        }
        public void SetCurrentPlanetType(string planetType)
        {
            SavePlanetType(planetType);
        }
        public int getplanettotalcount()
        {
	     string planetCountFilePath = Path.Combine(Application.persistentDataPath, "ModernBox", "PlanetCount.txt");

            int planetCount = 1;
            if (File.Exists(planetCountFilePath))
            {
                string countText = File.ReadAllText(planetCountFilePath);
                if (int.TryParse(countText, out int count))
                {
                    planetCount = count;
                }
            }
            return planetCount;
        }
    }
}
