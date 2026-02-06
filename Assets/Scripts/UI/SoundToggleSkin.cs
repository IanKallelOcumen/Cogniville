using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SoundToggleSkin : MonoBehaviour
{
    public Toggle toggle;
    public Image targetGraphic;

    [Header("Sprites")]
    public Sprite onSpriteBlue;
    public Sprite offSpriteRed;
    public Sprite pressedSpriteGreen;

    [Header("Options")]
    public bool forceColorMultiplier1 = true;
    public bool lockSizeToInitial = true;

    RectTransform _rt;
    Vector2 _initialSize;
    LayoutElement _le;

    void Reset()
    {
        toggle = GetComponent<Toggle>();
        if (toggle != null && targetGraphic == null)
            targetGraphic = toggle.targetGraphic as Image;
    }

    void OnEnable()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        if (!targetGraphic) targetGraphic = toggle.targetGraphic as Image;
        _rt = targetGraphic ? targetGraphic.rectTransform : null;

        if (_rt) _initialSize = _rt.sizeDelta;

        _le = GetComponent<LayoutElement>();
        if (!_le) _le = gameObject.AddComponent<LayoutElement>();
        if (lockSizeToInitial && _rt)
        {
            _le.preferredWidth  = _initialSize.x;
            _le.preferredHeight = _initialSize.y;
        }

        ApplySettings();
        SyncVisual();
        toggle.onValueChanged.AddListener(OnToggled);
    }

    void OnDisable()
    {
        if (toggle != null) toggle.onValueChanged.RemoveListener(OnToggled);
    }

    void OnToggled(bool isOn) => SyncVisual();

    void ApplySettings()
    {
        if (!toggle || !targetGraphic) return;
        toggle.targetGraphic = targetGraphic;
        toggle.transition = Selectable.Transition.SpriteSwap;

        if (forceColorMultiplier1)
        {
            var colors = toggle.colors;
            colors.colorMultiplier = 1f;
            toggle.colors = colors;
        }

        targetGraphic.preserveAspect = false;

        var st = toggle.spriteState;
        st.highlightedSprite = null;
        st.selectedSprite = null;
        st.disabledSprite = null;
        st.pressedSprite = pressedSpriteGreen;
        toggle.spriteState = st;
    }

    public void SyncVisual()
    {
        if (!toggle || !targetGraphic) return;
        targetGraphic.sprite = toggle.isOn ? onSpriteBlue : offSpriteRed;
    }
}
