using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrodyB
{
    /// <summary>
    /// An editor window with a collection of small, handy tools useful for most projects
    /// </summary>
    public class HandyToolsWindow : EditorWindow
    {
        #region Constants
        const string EDITORPREFS_DATA = "HandyToolsData";
        const int RECENT_SCENES_CAPACITY = 5;
        #endregion

        #region Data Classes
        [System.Serializable]
        public class SceneInfo
        {
            public string name;
            public string scenePath;
        }
        #endregion

        #region  Static Methods
        [MenuItem("Window/Handy Tools")]
        public static void Open ()
        {
            HandyToolsWindow window = GetWindow<HandyToolsWindow>();
		    window.titleContent = new GUIContent("Handy Tools");
        }
        #endregion

        #region Enums
        /// <summary>
        /// Enum for controlling which tool is visible in the window
        /// </summary>
        enum State
        {
            RecentScenes = 0
        }
        #endregion

        #region Members
        State currentState = State.RecentScenes;

        [SerializeField]
        List<SceneInfo> recentScenes = new List<SceneInfo>(RECENT_SCENES_CAPACITY);
        #endregion

        #region Editor Window Methods
        void OnEnable ()
        {
            LoadData();
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        void OnDisable ()
        {
            SaveData();
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        void OnGUI ()
        {
            DrawToolbar();

            switch (currentState)
            {
                case State.RecentScenes:
                    DrawRecentScenes();
                    break;
            }
        }

        /// <summary>
        /// Draws the top button bar that switches between available tools
        /// </summary>
        void DrawToolbar ()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = currentState != State.RecentScenes;
            if (GUILayout.Button("Recent Scenes"))
            {
                currentState = State.RecentScenes;
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        void SaveData ()
        {
            string data = JsonUtility.ToJson(this, true);
            EditorPrefs.SetString(EDITORPREFS_DATA, data);
        }

        void LoadData ()
        {
            JsonUtility.FromJsonOverwrite(
                EditorPrefs.GetString(EDITORPREFS_DATA, JsonUtility.ToJson(this, false)),
                this
            );
        }
        #endregion

        #region Recent Scenes Methods
        /// <summary>
        /// Draw the GUI for the Recent Scenes tool
        /// </summary>
        void DrawRecentScenes ()
        {
            foreach (SceneInfo scene in recentScenes)
            {
                if (scene == null)
                    continue;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(scene.scenePath, EditorStyles.boldLabel);
                
                if (GUILayout.Button("Open", GUILayout.Width(64f)))
                {
                    EditorSceneManager.OpenScene(scene.scenePath, OpenSceneMode.Single);
                    return;
                }

                if (GUILayout.Button("+", GUILayout.Width(24f)))
                {
                    EditorSceneManager.OpenScene(scene.scenePath, OpenSceneMode.Additive);
                    return;
                }

                EditorGUILayout.EndHorizontal();

            }
        }

        void OnSceneOpened (Scene scene, OpenSceneMode mode)
        {
            if (scene == null || recentScenes == null)
                return;

            SceneInfo sceneInfo = recentScenes.Find(x => x.scenePath == scene.path);

            if (sceneInfo != null)
            {
                recentScenes.Remove(sceneInfo);
            }
            else if (recentScenes.Count == RECENT_SCENES_CAPACITY)
            {
                recentScenes.RemoveAt(recentScenes.Count - 1);
            }

            recentScenes.Insert(0, new SceneInfo() {
                scenePath = scene.path,
                name = scene.name
            });

            SaveData();
        }
        #endregion
    }
}