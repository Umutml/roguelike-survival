using UnityEngine;

namespace GameCore.Tutorial
{
    public class TutorialOutline : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject hereText;

        public void ToggleOutline(bool toggle)
        {
            spriteRenderer.enabled = toggle;
            hereText.SetActive(toggle);
        }
    }
}
