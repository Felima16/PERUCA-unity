using UnityEngine;
using System.Collections.Generic;
using System;

public class SceneManager : MonoBehaviour
{
    private static SceneManager instance;
    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = GameObject.Find("SceneManager");
                if (go == null)
                    go = new GameObject("SceneManager");

                instance = go.GetComponent<SceneManager>();
                if (instance == null)
                    instance = go.AddComponent<SceneManager>();

                DontDestroyOnLoad(go);
                instance.Init();
            }

            return instance;
        }
    }

    private String initialLog = "";
    private Dictionary<DialogueScene, List<DialogueCase>> scenesDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        if (scenesDictionary == null)
            scenesDictionary = CreateScenes();
    }

    public String GetInitialLog()
    {
        return initialLog;
    }

    public Dictionary<DialogueScene, List<DialogueCase>> GetScenes()
    {
        return scenesDictionary;
    }

    private Dictionary<DialogueScene, List<DialogueCase>> CreateScenes()
    {
        SetLog("Creating dialogue scenes...");
        Dictionary<DialogueScene, List<DialogueCase>> scenes = new Dictionary<DialogueScene, List<DialogueCase>>();

        // Load dialogue cases for each scene from JSON files
        scenes[DialogueScene.AvatarEditor] = GetSceneFromJSON("DialogueSystem/Cases/EditCase");
        scenes[DialogueScene.Actions] = GetSceneFromJSON("DialogueSystem/Cases/ActionsCase");
        scenes[DialogueScene.OrganiseGame] = GetSceneFromJSON("DialogueSystem/Cases/OrganiseGameCase");

        return scenes;
    }

    private List<DialogueCase> GetSceneFromJSON(String resourcePath)
    {
        SetLog("Loading dialogue cases from Resources at path: " + resourcePath);
        
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
            if (jsonFile == null)
            {
                SetLog("File not found in Resources at path: " + resourcePath);
                Debug.LogError("File not found in Resources at path: " + resourcePath);
                return new List<DialogueCase>();
            }

            string json = jsonFile.text;
            
            if (string.IsNullOrEmpty(json))
            {
                SetLog("JSON file is empty at path: " + resourcePath);
                Debug.LogError("JSON file is empty at path: " + resourcePath);
                return new List<DialogueCase>();
            }

            DialogueCaseWrapper wrapper = JsonUtility.FromJson<DialogueCaseWrapper>(json);
            if (wrapper == null || wrapper.cases == null)
            {
                SetLog("Failed to parse JSON or cases array is null at path: " + resourcePath);
                Debug.LogError("Failed to parse JSON or cases array is null at path: " + resourcePath);
                return new List<DialogueCase>();
            }
            
            SetLog("Loaded " + wrapper.cases.Length + " dialogue cases from JSON at path: " + resourcePath);
            return new List<DialogueCase>(wrapper.cases);
        }
        catch (System.Exception ex)
        {
            SetLog("Error loading from Resources: " + ex.Message);
            Debug.LogError("Error loading from Resources at path: " + resourcePath + " - " + ex.Message);
            return new List<DialogueCase>();
        }
    }

    private void SetLog(String log)
    {
        initialLog = initialLog + log + "\n";
    }
}