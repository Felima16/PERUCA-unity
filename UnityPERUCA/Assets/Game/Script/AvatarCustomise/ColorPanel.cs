using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AvatarLab;
using Unity.VisualScripting;

public enum TypeColor { Eye, Skin, Hair }
public class ColorPanel : MonoBehaviour
{
    [SerializeField]
    List<GameObject> colorsButton = new List<GameObject>();
    private TypeColor typeColor;
    AvatarCustom avatarCustomScript;
    private float greyR = 0.299f, greyG = 0.587f, greyB = 0.114f, greyA = 0.1f;
    Color[] primary = { new Color(0.2862745f, 0.4f, 0.4941177f), new Color(0.4392157f, 0.1960784f, 0.172549f), new Color(0.3529412f, 0.3803922f, 0.2705882f), new Color(0.682353f, 0.4392157f, 0.2196079f), new Color(0.4313726f, 0.2313726f, 0.2705882f), new Color(0.5921569f, 0.4941177f, 0.2588235f), new Color(0.482353f, 0.4156863f, 0.3529412f), new Color(0.2352941f, 0.2352941f, 0.2352941f), new Color(0.2313726f, 0.4313726f, 0.4156863f) };
    Color[] hair = { new Color(0.3098039f, 0.254902f, 0.1764706f), new Color(0.2196079f, 0.2196079f, 0.2196079f), new Color(0.8313726f, 0.6235294f, 0.3607843f), new Color(0.8901961f, 0.7803922f, 0.5490196f), new Color(0.8000001f, 0.8196079f, 0.8078432f), new Color(0.6862745f, 0.4f, 0.2352941f), new Color(0.5450981f, 0.427451f, 0.2156863f), new Color(0.8470589f, 0.4666667f, 0.2470588f) };
    Color[] skin = { new Color(1f, 0.8000001f, 0.682353f), new Color(0.8196079f, 0.6352941f, 0.4588236f), new Color(0.5647059f, 0.4078432f, 0.3137255f), new Color(0.9607844f, 0.7843138f, 0.7294118f) };
    // Start is called before the first frame update
    void Start()
    {
        avatarCustomScript = GameObject.Find("MainCharacters").GetComponent<AvatarCustom>();
        typeColor = TypeColor.Skin;
        CreateEvent();
        CreateColorPallet();
    }

    // Update is called once per frame
    void OnDestroy()
    {
        for (int i = 0; i < colorsButton.Count; i++)
        {
            int closureIndex = i ;
            Button button = colorsButton[i].GetComponent<Button>();
            button.onClick.RemoveListener(() => ApplyColorInAvatar(closureIndex));
        }
    }

    private void CreateEvent()
    {
        for (int i = 0; i < colorsButton.Count; i++)
        {
            int closureIndex = i ;
            Button button = colorsButton[i].GetComponent<Button>();
            button.onClick.AddListener(() => ApplyColorInAvatar(closureIndex));
        }
    }

    private void CreateColorPallet() {
        for (int i = 0; i < colorsButton.Count; i++)
        {
            Button button = colorsButton[i].GetComponent<Button>();
            button.interactable = false;
        }

        switch (typeColor)
        {
            case TypeColor.Eye:
                for (int i = 0; i < primary.Length; i++)
                {
                    Button button = colorsButton[i].GetComponent<Button>();

                    if (button.transition != Selectable.Transition.ColorTint)
                        continue;

                    var theColor = button.colors;
                    theColor.normalColor = primary[i];
                    theColor.selectedColor = new Color(primary[i].r * greyR, primary[i].g * greyG, primary[i].b * greyB);
                    button.colors = theColor;
                    button.interactable = true;
                }
                break;
            case TypeColor.Skin:
                for (int i = 0; i < skin.Length; i++)
                {
                    Button button = colorsButton[i].GetComponent<Button>();

                    if (button.transition != Selectable.Transition.ColorTint)
                        continue;
                    
                    var theColor = button.colors;
                    theColor.normalColor = skin[i];
                    button.colors = theColor;
                    button.interactable = true;
                }
                break;
            case TypeColor.Hair:
                for (int i = 0; i < hair.Length; i++)
                {
                    Button button = colorsButton[i].GetComponent<Button>();

                    if (button.transition != Selectable.Transition.ColorTint)
                        continue;

                    var theColor = button.colors;
                    theColor.normalColor = hair[i];
                    button.colors = theColor;
                    button.interactable = true;
                }
                break;
        }
    }

    private void ApplyColorInAvatar(int color)
    {
        switch (typeColor)
        {
            case TypeColor.Eye:
                if(color < primary.Length)
                    avatarCustomScript.ColorApply(primary[color], "Eye");
                break;
            case TypeColor.Skin:
                if(color < skin.Length)
                    avatarCustomScript.ColorApply(skin[color], "Skin");
                break;
            case TypeColor.Hair:
                if(color < hair.Length)
                    avatarCustomScript.ColorApply(hair[color], "Hair");
                break;
        }
    }

    public void SetColorPanel(TypeColor typeColor)
    {
        this.typeColor = typeColor;
        CreateColorPallet();
    }
}