using System;
using UnityEngine;

[System.Serializable]
public class DialogueCase
{
    public String id; // Unique identifier for the dialogue case
    public String text;
    public String image; // Optional image associated with the dialogue case
    public DialogueOption[] options;
}

[System.Serializable]
public class DialogueOption
{
    public String title;
    public String nextSceneIndex;

    public DialogueOption(String title, String nextSceneIndex)
    {
        this.title = title;
        this.nextSceneIndex = nextSceneIndex;
    }
}

[System.Serializable]
public class DialogueCaseWrapper
{
    public DialogueCase[] cases;
}