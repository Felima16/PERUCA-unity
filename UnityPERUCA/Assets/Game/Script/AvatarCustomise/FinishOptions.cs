using UnityEngine;
using UnityEngine.UI;

namespace AvatarLab {
    public class FinishOptions : MonoBehaviour
    {
        [SerializeField]
        private Button optionButton;

        private bool firstTime = true;

        void OnEnable()
        {
            optionButton.onClick.AddListener(Finish);
        }

        void OnDisable()
        {
            optionButton.onClick.RemoveListener(Finish);
        }

        private void Finish()
        {
            if (firstTime)
            {
                firstTime = false;
                DialogueManager.instance.SetDialogueScene(DialogueScene.Actions, true);
                return;
            }
            AvatarManager.instance.UpdateAvatarState(AvatarState.Game);
        }
    }
}