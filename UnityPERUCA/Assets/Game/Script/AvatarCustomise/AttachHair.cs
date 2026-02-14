using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using AvatarLab;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization;

public class AttachHair : MonoBehaviour
{
    private AvatarCustom avatarCustomScript;
    private HairReset hairResetScript;

    void Start()
    {
        avatarCustomScript = GameObject.Find("MainCharacters").GetComponent<AvatarCustom>();
        hairResetScript = GameObject.Find("HairSet").GetComponent<HairReset>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag != "Hair") 
            return;
        
        avatarCustomScript.ActivateItemFrom(other.gameObject.name, AvatarElement.Hair);
        hairResetScript.Reset();
        other.gameObject.SetActive(false);
    }
}
