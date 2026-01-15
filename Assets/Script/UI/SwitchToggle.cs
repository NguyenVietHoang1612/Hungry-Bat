using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class SwitchToggle : MonoBehaviour
    {
        [SerializeField] RectTransform uiHandleRectTransform;
        [SerializeField] Color backgroundActiveColor;
        [SerializeField] Color handleActiveColor;

        private Image backgroundImage, handleImage;

        private Color backgroundDefaultColor, handleDefaultColor;

        private Toggle toggle;

        private Vector2 handlePosition;

        void Awake()
        {
            toggle = GetComponent<Toggle>();

            handlePosition = uiHandleRectTransform.anchoredPosition;

            backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
            handleImage = uiHandleRectTransform.GetComponent<Image>();

            backgroundDefaultColor = backgroundImage.color;
            handleDefaultColor = handleImage.color;

            toggle.onValueChanged.AddListener(OnSwitch);
            toggle.isOn = GameManager.Instance.Effects;

            if (toggle.isOn)
            {
                OnSwitch(true);
            }    
                
        }

        void OnSwitch(bool on)
        {
            uiHandleRectTransform.anchoredPosition = on ? handlePosition * -1 : handlePosition ; 

            backgroundImage.color = on ? backgroundActiveColor : backgroundDefaultColor ; 

            handleImage.color = on ? handleActiveColor : handleDefaultColor ; 

            if (on)
            {
                GameManager.Instance.Effects = true;
            }
            else
            {
                GameManager.Instance.Effects = false;
            }

            GameManager.Instance.SaveSettings();
        }

        void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnSwitch);
        }
    }
}
