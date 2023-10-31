using UnityEngine;
using MelonLoader;
using static MelonLoader.MelonLogger;
using Il2CppDecaGames.RotMG.Managers;
using Il2Cpp;
using System.Collections;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Object = UnityEngine.Object;

namespace VisualiaMod
{
    public class visualia : MelonMod
    {
        public static visualia? instance;
        private bool isMain = false;

        private GameObject? GameControllerObj;
        private GameObject? FpsStatsObj;
        private ApplicationManager? ApplicationManagerObj;
        private GameObject? fpsDisplayObj;
        private float enableDelay = 0.1f;
        private float timeToEnableChild;

        private bool isCheckingTextComponent = false;

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
            if (GameControllerObj == null)
            {
                Msg("GameController not found.");
                return;
            }

            ApplicationManagerObj = GetAndLogComponent<ApplicationManager>(GameControllerObj, "ApplicationManager");
            if (ApplicationManagerObj == null)
            {
                Msg("ApplicationManager not found.");
                return;
            }

            FpsStatsObj = FindAndLog("Fps/Stats", GameControllerObj);
            if (FpsStatsObj == null)
            {
                Msg("Fps/Stats not found.");
                return;
            }

            SafeActivate(FpsStatsObj, "Fps/Stats");

            fpsDisplayObj = FindAndLog("FPS ---------------------------", FpsStatsObj);
            if (fpsDisplayObj == null)
            {
                Msg("FPS Display not found.");
                return;
            }

            timeToEnableChild = Time.time + enableDelay;

            string[] childrenNamesToDestroy = {
                "BG_Image_TEXT", "avg_fps_text", "avg_fps_value",
                "min_fps_text", "min_fps_value", "max_fps_text", "max_fps_value", "BG_Image_FULL", "BG_Image_BASIC", "ms_text", "ms_text_value"
            };

            foreach (string childName in childrenNamesToDestroy)
            {
                GameObject? childObj = FindAndLog(childName, fpsDisplayObj);
                if (childObj != null)
                {
                    UnityEngine.Object.Destroy(childObj);
                }
            }

            UpdateTextColorAndPosition();
            // Find the Game Overlay Canvas
            GameObject? gameOverlayCanvas = GameObject.Find("Game Overlay Canvas");
            if (gameOverlayCanvas == null)
            {
                Msg("Game Overlay Canvas not found.");
                return;
            }

            // Find and disable the Behaviour component in LowHP_Overlay
            GameObject? lowHPOverlay = gameOverlayCanvas.transform.Find("LowHP_Overlay")?.gameObject;
            if (lowHPOverlay == null)
            {
                Msg("LowHP_Overlay not found.");
                return;
            }

            Behaviour? behaviourComponentLowHP = GetAndLogComponent<Behaviour>(lowHPOverlay, "Behaviour");
            if (behaviourComponentLowHP != null)
            {
                behaviourComponentLowHP.enabled = false;
            }

            // Find and disable the Behaviour component in Underwater_Overlay
            GameObject? underwaterOverlay = gameOverlayCanvas.transform.Find("Underwater_Overlay")?.gameObject;
            if (underwaterOverlay == null)
            {
                Msg("Underwater_Overlay not found.");
                return;
            }

            Behaviour? behaviourComponentUnderwater = GetAndLogComponent<Behaviour>(underwaterOverlay, "Behaviour");
            if (behaviourComponentUnderwater != null)
            {
                behaviourComponentUnderwater.enabled = false;
            }
            // Find the Log_GUI GameObject
            GameObject? logGUIGameObject = GameObject.Find("Log_GUI");
            if (logGUIGameObject == null)
            {
                Msg("Log_GUI GameObject not found.");
            }
            else
            {
                // Find the Log GameObject inside Log_GUI
                GameObject? logGameObject = FindAndLog("Log", logGUIGameObject);
                if (logGameObject != null)
                {
                    // Find the ClockContainer GameObject inside Log
                    GameObject? clockContainer = FindAndLog("ClockContainer", logGameObject);
                    if (clockContainer != null)
                    {
                        // Set the ActiveSelf property of ClockContainer to false
                        clockContainer.SetActive(false);
                    }
                    GameObject? LogHeader = FindAndLog("Log Header", logGameObject);
                    if (LogHeader != null)
                    {
                        // Set the ActiveSelf property of ClockContainer to false
                        LogHeader.SetActive(false);
                    }
                }
            }

        }

        private void UpdateTextColorAndPosition()
        {
            if (!isCheckingTextComponent)
            {
                MelonCoroutines.Start(CheckTextComponentValue()); 
            }
        }
        private IEnumerator CheckTextComponentValue()
        {
            isCheckingTextComponent = true;
            while (true)
            {
                string[] childrenNames = { "fps_text", "fps_text_value"};
                Vector3[] positions =
                {
                    new Vector3(1892.63f, 1041.095f, 0f),
                    new Vector3(1784.126f, 1041.095f, 0f)
                };

                for (int i = 0; i < childrenNames.Length; i++)
                {
                    GameObject? childObj = FindAndLog(childrenNames[i], fpsDisplayObj);
                    if (childObj != null)
                    {
                        UnityEngine.UI.Text? textComponent = GetAndLogComponent<UnityEngine.UI.Text>(childObj, "Text");
                        if (textComponent != null)
                        {
                            textComponent.fontSize = 20;
                            textComponent.color = new Color(0, 1, 0, 0.6027f);

                            if (float.TryParse(textComponent.m_Text, out float floatValue))
                            {
                                if (IsWholeNumber(floatValue))
                                {
                                    Color textColor = GetColorForWholeNumber(floatValue);
                                    textComponent.color = textColor;
                                }
                                else
                                {
                                    Color textColor = GetColorForValueWithDecimal(floatValue);
                                    textComponent.color = textColor;
                                }
                            }
                        }
                        childObj.transform.position = positions[i];
                    }
                }
                yield return new WaitForSeconds(1.0f);
            }
        }


        private bool IsWholeNumber(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value));
        }

        private Color GetColorForWholeNumber(float value)
        {
            if (value >= 1 && value <= 20)
            {
                return new Color(1, 0, 0, 0.6027f);
            }
            else if (value > 20 && value <= 30)
            {
                return new Color(1, 0.6004f, 0, 0.6027f);
            }
            else if (value > 30 && value <= 50)
            {
                return new Color(1, 1, 0.1679f, 0.7238f);
            }
            else if (value > 50 && value <= 60)
            {
                return new Color(0, 1, 0, 0.6027f);
            }
            else
            {
                return new Color(1, 0, 0, 0.6027f);
            }
        }

        private Color GetColorForValueWithDecimal(float value)
        {
            if (value >= 1.0 && value <= 20.0)
            {
                return new Color(0, 1, 0, 0.6027f);
            }
            else if (value > 50.0 && value <= 100.0)
            {
                return new Color(1, 0.6004f, 0, 0.6027f);
            }
            else if (value > 20.0 && value <= 50.0)
            {
                return new Color(1, 1, 0.1679f, 0.7238f);
            }
            else if (value > 100.0)
            {
                return new Color(1, 0, 0, 0.6027f);
            }
            else
            {
                return new Color(1, 0, 0, 0.6027f);
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
