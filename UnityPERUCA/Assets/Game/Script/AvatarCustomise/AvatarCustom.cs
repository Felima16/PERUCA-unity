using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public enum AvatarElement
{
    Head,
    Eyebrow,
    FacialHair,
    Hair
    // Torso,
    // Arm_Upper_Right,
    // Arm_Upper_Left,
    // Arm_Lower_Right,
    // Arm_Lower_Left,
    // Hand_Right,
    // Hand_Left,
    // Hips,
    // Leg_Right,
    // Leg_Left
}

namespace AvatarLab {
    public enum Gender { Male, Female }
    public enum Race { Human, Elf }
    public enum SkinColor { White, Brown, Black, Elf }
    public enum Elements {  Yes, No }
    public enum HeadCovering { HeadCoverings_Base_Hair, HeadCoverings_No_FacialHair, HeadCoverings_No_Hair }
    public enum FacialHair { Yes, No }

    public class AvatarCustom : MonoBehaviour {
        [SerializeField]
        Text textDebbug;
        [Header("Material")]
        public Material mat;

         // list of enabed objects on character
        [HideInInspector]
        public List<GameObject> enabledObjects = new List<GameObject>();
        
        // character object lists
        // male list
        [HideInInspector]
        public CharacterObjectGroups male;

        // female list
        [HideInInspector]
        public CharacterObjectGroups female;

        // universal list
        [HideInInspector]
        public CharacterObjectListsAllGender allGender;

        //Avater Gender
        private Gender gender = Gender.Female;

        //Avatar parts
        GameObject hair;
        GameObject head;
        GameObject eyebrows;
        GameObject facialHair;
        // Start is called before the first frame update
        void Start()
        {
            // rebuild all lists
            BuildLists();

            AvatarApplyElement();
        }

        public void SelectGender(Gender selectedGender) 
        {
            gender = selectedGender;
            AvatarApplyElement();
        }

        public void ActivateItemFrom(string name, AvatarElement type)
        {
            DebugManager.instance.MyLOG("#### ActivateItemFrom" + name);
            switch (gender)
            {
                case Gender.Female:
                    ActivateFemaleItemFrom(name, type);
                    break;
                case Gender.Male:
                    ActivateMaleItemFrom(name, type);
                    break;
                default:
                    print ("Incorrect intelligence level.");
                    break;
            }
        }

        public void ColorApply(Color color, string name)
        {
            if (name == "Hair")
            {
                mat.SetColor("_Color_Hair", color);
            }
            else if (name == "Eye")
            {
                mat.SetColor("_Color_Eyes", color);
            }
            else if (name == "Skin")
            {
                mat.SetColor("_Color_Skin", color);
            }
        }

        // enable game object and add it to the enabled objects list
        private void ActivateMaleItemFrom(string name, AvatarElement type)
        {
            DebugManager.instance.MyLOG("#### Activate Male Item Attached: " + name);
            switch (type)
            {
                case AvatarElement.Head:
                    head = male.headAllElements.Find(x => x.name == name);
                    break;
                case AvatarElement.Eyebrow:
                    eyebrows = male.eyebrow.Find(x => x.name == name);
                    break;
                case AvatarElement.FacialHair:
                    facialHair = male.facialHair.Find(x => x.name == name);
                    break;
                case AvatarElement.Hair:
                    hair = allGender.all_Hair.Find(x => x.name == name);
                    break;
                default:
                    print ("Incorrect intelligence level.");
                    break;
            }

            AvatarApplyElement();
        }

        private void ActivateFemaleItemFrom(string name, AvatarElement type)
        {
            DebugManager.instance.MyLOG("#### Activate Female Item Attached: " + name);
            switch (type)
            {
                case AvatarElement.Head:
                    head = female.headAllElements.Find(x => x.name == name);
                    break;
                case AvatarElement.Eyebrow:
                    eyebrows = female.eyebrow.Find(x => x.name == name);
                    break;
                case AvatarElement.Hair:
                    hair = allGender.all_Hair.Find(x => x.name == name);
                    break;
                default:
                    print ("Incorrect intelligence level.");
                    break;
            }

            AvatarApplyElement();
        }

        void AvatarApplyElement() {
            // disable all enabled objects
            foreach (GameObject go in enabledObjects)
            {
                go.SetActive(false);
            }

            // clear enabled objects list
            enabledObjects.Clear();

            // enable the selected object
            if (head != null)
                ActivateItem(head);

            if (eyebrows != null)
                ActivateItem(eyebrows);

            if (facialHair != null)
                ActivateItem(facialHair);

            if (hair != null)
                ActivateItem(hair);

            switch(gender)
            {
                case Gender.Female:
                    ActiveStandardFemaleCharacter();
                break;

                case Gender.Male:
                    ActivateStandardMaleCharacter();
                break;  
            }
        }

        void ActivateItem(GameObject go)
        {
            // enable item
            go.SetActive(true);

            // add item to the enabled items list
            enabledObjects.Add(go);
        }

        //####### creating objects for the character
        private void ActivateStandardMaleCharacter()
        {
            ActivateItem(male.headAllElements[0]);
            ActivateItem(male.eyebrow[0]);
            ActivateItem(male.torso[3]);
            ActivateItem(male.arm_Upper_Right[0]);
            ActivateItem(male.arm_Upper_Left[0]);
            ActivateItem(male.arm_Lower_Right[0]);
            ActivateItem(male.arm_Lower_Left[0]);
            ActivateItem(male.hand_Right[0]);
            ActivateItem(male.hand_Left[0]);
            ActivateItem(male.hips[6]);
            ActivateItem(male.leg_Right[17]);
            ActivateItem(male.leg_Left[17]);
        }
        private void ActiveStandardFemaleCharacter()
        {
            ActivateItem(female.headAllElements[0]);
            ActivateItem(female.eyebrow[0]);
            ActivateItem(female.torso[3]);
            ActivateItem(female.arm_Upper_Right[0]);
            ActivateItem(female.arm_Upper_Left[0]);
            ActivateItem(female.arm_Lower_Right[0]);
            ActivateItem(female.arm_Lower_Left[0]);
            ActivateItem(female.hand_Right[0]);
            ActivateItem(female.hand_Left[0]);
            ActivateItem(female.hips[6]);
            ActivateItem(female.leg_Right[17]);
            ActivateItem(female.leg_Left[17]);
        }

        // build all item lists for use in randomization
        private void BuildLists()
        {
            //build out male lists
            BuildList(male.headAllElements, "Male_Head_All_Elements");
            BuildList(male.eyebrow, "Male_01_Eyebrows");
            BuildList(male.facialHair, "Male_02_FacialHair");
            BuildList(male.torso, "Male_03_Torso");
            BuildList(male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
            BuildList(male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
            BuildList(male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
            BuildList(male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
            BuildList(male.hand_Right, "Male_08_Hand_Right");
            BuildList(male.hand_Left, "Male_09_Hand_Left");
            BuildList(male.hips, "Male_10_Hips");
            BuildList(male.leg_Right, "Male_11_Leg_Right");
            BuildList(male.leg_Left, "Male_12_Leg_Left");

            //build out female lists
            BuildList(female.headAllElements, "Female_Head_All_Elements");
            BuildList(female.eyebrow, "Female_01_Eyebrows");
            BuildList(female.facialHair, "Female_02_FacialHair");
            BuildList(female.torso, "Female_03_Torso");
            BuildList(female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
            BuildList(female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
            BuildList(female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
            BuildList(female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
            BuildList(female.hand_Right, "Female_08_Hand_Right");
            BuildList(female.hand_Left, "Female_09_Hand_Left");
            BuildList(female.hips, "Female_10_Hips");
            BuildList(female.leg_Right, "Female_11_Leg_Right");
            BuildList(female.leg_Left, "Female_12_Leg_Left");

            // build out all gender lists
            BuildList(allGender.all_Hair, "All_01_Hair");
            BuildList(allGender.all_Head_Attachment, "All_02_Head_Attachment");
            BuildList(allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
            hair = allGender.all_Hair[0];
        }

        // called from the BuildLists method
        void BuildList(List<GameObject> targetList, string characterPart)
        {
            Transform[] rootTransform = gameObject.GetComponentsInChildren<Transform>();

            // declare target root transform
            Transform targetRoot = null;

            // find character parts parent object in the scene
            foreach (Transform t in rootTransform)
            {
                if (t.gameObject.name == characterPart)
                {
                    targetRoot = t;
                    break;
                }
            }

            // clears targeted list of all objects
            targetList.Clear();

            // cycle through all child objects of the parent object
            for (int i = 0; i < targetRoot.childCount; i++)
            {
                // get child gameobject index i
                GameObject go = targetRoot.GetChild(i).gameObject;

                // disable child object
                go.SetActive(false);

                // add object to the targeted object list
                targetList.Add(go);

                // collect the material for the random character, only if null in the inspector;
                if (!mat)
                {
                    if (go.GetComponent<SkinnedMeshRenderer>())
                        mat = go.GetComponent<SkinnedMeshRenderer>().material;
                }
            }
        }
    }

    // classe for keeping the lists organized, allows for simple switching from male/female objects
    [System.Serializable]
    public class CharacterObjectGroups
    {
        public List<GameObject> headAllElements;
        public List<GameObject> headNoElements;
        public List<GameObject> eyebrow;
        public List<GameObject> facialHair;
        public List<GameObject> torso;
        public List<GameObject> arm_Upper_Right;
        public List<GameObject> arm_Upper_Left;
        public List<GameObject> arm_Lower_Right;
        public List<GameObject> arm_Lower_Left;
        public List<GameObject> hand_Right;
        public List<GameObject> hand_Left;
        public List<GameObject> hips;
        public List<GameObject> leg_Right;
        public List<GameObject> leg_Left;
    }

    // classe for keeping the lists organized, allows for organization of the all gender items
    [System.Serializable]
    public class CharacterObjectListsAllGender
    {
        public List<GameObject> headCoverings_Base_Hair;
        public List<GameObject> all_Hair;
        public List<GameObject> all_Head_Attachment;
    }
}