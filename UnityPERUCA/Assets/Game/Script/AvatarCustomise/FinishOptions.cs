using UnityEngine;
using UnityEngine.UI;

namespace AvatarLab {
    public class FinishOptions : MonoBehaviour
    {
        [SerializeField]
        private Button optionButton;

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
            if (!DialogueManager.instance.VerifyShouldShowActionsDialogue())
            {
                AvatarManager.instance.UpdateAvatarState(AvatarState.Game);
            }
        }
    }
}