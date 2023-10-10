using UnityEngine;
using MelonLoader;
using static MelonLoader.MelonLogger;
using Il2CppDecaGames.RotMG.Managers;

namespace FpsStatsMod
{
    public class ShowFPS : MelonMod
    {
        public static ShowFPS? instance;
        private bool isMain = false;
        private GameObject? GameControllerObj;
        private GameObject? FpsStatsObj;
        private ApplicationManager? ApplicationManagerObj;
        private GameObject? fpsDisplayObj;
        private float enableDelay = 0.1f;
        private float timeToEnableChild;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            isMain = sceneName == "Main";
            if (isMain)
            {
                Setup();
            }
        }

        private void Setup()
        {
            GameControllerObj = FindAndLog("GameController");
            if (GameControllerObj == null) return;

            ApplicationManagerObj = GetAndLogComponent<ApplicationManager>(GameControllerObj, "ApplicationManager");
            if (ApplicationManagerObj == null) return;

            FpsStatsObj = FindAndLog("Fps/Stats", GameControllerObj);
            if (FpsStatsObj == null) return;

            SafeActivate(FpsStatsObj, "Fps/Stats");

            fpsDisplayObj = FindAndLog("FPS ---------------------------", FpsStatsObj);
            if (fpsDisplayObj == null) return;

            timeToEnableChild = Time.time + enableDelay;

            string[] childrenNamesToDestroy = {
                "BG_Image_TEXT", "avg_fps_text", "avg_fps_value",
                "min_fps_text", "min_fps_value", "max_fps_text", "max_fps_value"
            };

            foreach (string childName in childrenNamesToDestroy)
            {
                GameObject? childObj = FindAndLog(childName, fpsDisplayObj);
                if (childObj != null)
                {
                    UnityEngine.Object.Destroy(childObj);
                }
            }

            // Update text color, position, and font size for specified children
            UpdateTextColorAndPosition();
        }

        private void UpdateTextColorAndPosition()
        {
            string[] childrenNames = { "fps_text", "fps_text_value", "ms_text", "ms_text_value" };
            // Color and opacity of the text
            Color newColor = new Color(1, 1, 1, 0.5027f);
            // Position of the text
            Vector3[] positions =
            {
                new Vector3(1892.63f, 1041.095f, 0f),
                new Vector3(1784.126f, 1041.095f, 0f),
                new Vector3(1884.052f, 1021.87f, 0f),
                new Vector3(1778.906f, 1022.269f, 0f)
            };
            int newFontSize = 20;

            for (int i = 0; i < childrenNames.Length; i++)
            {
                GameObject? childObj = FindAndLog(childrenNames[i], fpsDisplayObj);
                if (childObj != null)
                {
                    UnityEngine.UI.Text? textComponent = GetAndLogComponent<UnityEngine.UI.Text>(childObj, "Text");
                    if (textComponent != null)
                    {
                        textComponent.color = newColor;
                        textComponent.fontSize = newFontSize;
                    }
                    childObj.transform.position = positions[i];
                }
            }
        }

        private GameObject? FindAndLog(string objectName, GameObject? parent = null)
        {
            GameObject? foundObj = parent == null ? GameObject.Find(objectName) : FindObject(parent, objectName);
            return foundObj;
        }

        private T? GetAndLogComponent<T>(GameObject parent, string componentName) where T : Component
        {
            T? component = parent.GetComponent<T>();
            return component;
        }

        private void SafeActivate(GameObject obj, string objName)
        {
            try
            {
                obj.SetActive(true);
            }
            catch (Exception e)
            {
                Msg($"ERROR: Exception while trying to activate {objName} object: {e}");
            }
        }

        public override void OnUpdate()
        {
            if (fpsDisplayObj != null)
            {
                if (!fpsDisplayObj.activeInHierarchy)
                {
                    fpsDisplayObj.SetActive(true);
                }
            }
            else if (isMain && Time.time >= timeToEnableChild)
            {
                fpsDisplayObj = FindAndLog("FPS ---------------------------", FpsStatsObj);
                if (fpsDisplayObj != null)
                {
                    fpsDisplayObj.SetActive(true);
                }
            }
        }

        public static GameObject? FindObject(GameObject parent, string name)
        {
            Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trs)
            {
                if (t.name == name) { return t.gameObject; }
            }
            return null;
        }
    }
}
