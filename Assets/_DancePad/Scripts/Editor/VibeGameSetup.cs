using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace VibeArcade.Editor
{
    public static class VibeGameSetup
    {
        [MenuItem("Tools/Vibe Arcade/Интегрировать UI в GameScene")]
        public static void SetupUIInGameScene()
        {
            // 1. Deactivate old prototype canvases on Display 2
            string[] oldCanvasNames = new string[] {
                "ChooseLanguageCanvas",
                "ChooseMusicCanvas",
                "StartGameCanvas",
                "ResultCanvas",
                "Canvas"
            };

            foreach (var cName in oldCanvasNames)
            {
                var objs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var obj in objs)
                {
                    if (obj.name == cName && obj.transform.parent != null && obj.transform.parent.name == "Display2")
                    {
                        obj.SetActive(false);
                        Debug.Log($"[Vibe Arcade] Отключен старый плейсхолдер-канвас: {cName}");
                    }
                }
            }

            // 2. Find or create UI Manager Root
            GameObject uiRoot = GameObject.Find("Vibe_Arcade_UI_Manager");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("Vibe_Arcade_UI_Manager");
            }

            // 3. Tablet Document (Display 2 - Touchscreen)
            GameObject tabletObj = GameObject.Find("Tablet_UIDocument");
            if (tabletObj == null)
            {
                tabletObj = new GameObject("Tablet_UIDocument");
                tabletObj.transform.SetParent(uiRoot.transform, false);
            }
            UIDocument tabletDoc = tabletObj.GetComponent<UIDocument>();
            if (tabletDoc == null) tabletDoc = tabletObj.AddComponent<UIDocument>();

            var tabletUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VibeTabletScreen.uxml");
            if (tabletUxml != null) tabletDoc.visualTreeAsset = tabletUxml;

            var tabletSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/VibePanelSettings_Tablet.asset");
            if (tabletSettings == null)
            {
                tabletSettings = ScriptableObject.CreateInstance<PanelSettings>();
                tabletSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                tabletSettings.referenceResolution = new Vector2Int(1080, 1920);
                tabletSettings.match = 0.5f;
                tabletSettings.targetDisplay = 1; // Display 2 (Tablet)
                AssetDatabase.CreateAsset(tabletSettings, "Assets/UI/VibePanelSettings_Tablet.asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                tabletSettings.targetDisplay = 1; // Ensure Display 2
                EditorUtility.SetDirty(tabletSettings);
            }
            tabletDoc.panelSettings = tabletSettings;

            // 4. TV Document (Display 1 - TV Screen)
            GameObject tvObj = GameObject.Find("TV_UIDocument");
            if (tvObj == null)
            {
                tvObj = new GameObject("TV_UIDocument");
                tvObj.transform.SetParent(uiRoot.transform, false);
            }
            UIDocument tvDoc = tvObj.GetComponent<UIDocument>();
            if (tvDoc == null) tvDoc = tvObj.AddComponent<UIDocument>();

            var tvUxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VibeTVScreen.uxml");
            if (tvUxml != null) tvDoc.visualTreeAsset = tvUxml;

            var tvSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/VibePanelSettings_TV.asset");
            if (tvSettings == null)
            {
                tvSettings = ScriptableObject.CreateInstance<PanelSettings>();
                tvSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                tvSettings.referenceResolution = new Vector2Int(1920, 1080);
                tvSettings.match = 0.5f;
                tvSettings.targetDisplay = 0; // Display 1 (Main TV)
                AssetDatabase.CreateAsset(tvSettings, "Assets/UI/VibePanelSettings_TV.asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                tvSettings.targetDisplay = 0; // Ensure Display 1
                EditorUtility.SetDirty(tvSettings);
            }
            tvDoc.panelSettings = tvSettings;

            // 5. Controller
            VibeArcadeGameUI controller = uiRoot.GetComponent<VibeArcadeGameUI>();
            if (controller == null) controller = uiRoot.AddComponent<VibeArcadeGameUI>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("tabletDocument").objectReferenceValue = tabletDoc;
            so.FindProperty("tvDocument").objectReferenceValue = tvDoc;
            so.ApplyModifiedProperties();

            Selection.activeGameObject = uiRoot;
            EditorUtility.SetDirty(uiRoot);
            Debug.Log("<color=#00FF66><b>[Vibe Arcade]</b> Интерфейс планшета (Display 2) и ТВ (Display 1) успешно интегрирован в GameScene!</color>");
        }
    }
}
