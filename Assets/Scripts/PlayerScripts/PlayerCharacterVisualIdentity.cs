using UnityEngine;

public class PlayerCharacterVisualIdentity : MonoBehaviour
{
    private const string AuraName = "CharacterAura";
    private const string CrestName = "CharacterCrest";
    private const string CrestLabelName = "CharacterCrestLabel";
    private const string RoleMarkName = "CharacterRoleMark";

    private PlayableCharacterChoice _currentChoice;
    private SpriteRenderer _baseRenderer;
    private SpriteRenderer _auraRenderer;
    private SpriteRenderer _crestRenderer;
    private SpriteRenderer _roleMarkRenderer;
    private TextMesh _crestLabel;

    public void Apply(PlayableCharacterChoice choice)
    {
        _currentChoice = choice;
        EnsureObjects();
        ApplyColors();
        ApplyRoleShape();
    }

    private void EnsureObjects()
    {
        if (_baseRenderer == null)
            _baseRenderer = GetComponent<SpriteRenderer>();

        if (_baseRenderer == null)
            _baseRenderer = GetComponentInChildren<SpriteRenderer>();

        _auraRenderer = EnsureChildSprite(AuraName, Vector3.zero, sortingOffset: -1);
        _crestRenderer = EnsureChildSprite(CrestName, new Vector3(0f, 0.92f, 0f), sortingOffset: 15);
        _roleMarkRenderer = EnsureChildSprite(RoleMarkName, new Vector3(0f, -0.68f, 0f), sortingOffset: 14);

        Transform crest = transform.Find(CrestName);
        Transform labelTransform = crest != null ? crest.Find(CrestLabelName) : null;
        GameObject labelObject;

        if (labelTransform == null)
        {
            labelObject = new GameObject(CrestLabelName);
            labelObject.transform.SetParent(crest, false);
        }
        else
        {
            labelObject = labelTransform.gameObject;
        }

        labelObject.transform.localPosition = new Vector3(0f, -0.025f, -0.01f);
        labelObject.transform.localScale = Vector3.one;

        _crestLabel = labelObject.GetComponent<TextMesh>();
        if (_crestLabel == null)
            _crestLabel = labelObject.AddComponent<TextMesh>();

        _crestLabel.anchor = TextAnchor.MiddleCenter;
        _crestLabel.alignment = TextAlignment.Center;
        _crestLabel.fontSize = 54;
        _crestLabel.characterSize = 0.075f;

        MeshRenderer labelRenderer = labelObject.GetComponent<MeshRenderer>();
        if (labelRenderer != null)
            ApplySorting(labelRenderer, offset: 16);
    }

    private SpriteRenderer EnsureChildSprite(string childName, Vector3 localPosition, int sortingOffset)
    {
        Transform child = transform.Find(childName);
        GameObject childObject;

        if (child == null)
        {
            childObject = new GameObject(childName);
            childObject.transform.SetParent(transform, false);
        }
        else
        {
            childObject = child.gameObject;
        }

        childObject.transform.localPosition = localPosition;

        SpriteRenderer renderer = childObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = childObject.AddComponent<SpriteRenderer>();

        renderer.sprite = PickupSpriteFactory.CircleSprite;
        ApplySorting(renderer, sortingOffset);
        return renderer;
    }

    private void ApplyColors()
    {
        Color tint = RunLoadoutState.GetCharacterTint(_currentChoice);

        if (_baseRenderer != null)
            _baseRenderer.color = tint;

        if (_auraRenderer != null)
        {
            _auraRenderer.color = new Color(tint.r, tint.g, tint.b, 0.22f);
            _auraRenderer.transform.localScale = Vector3.one * GetAuraScale(_currentChoice);
        }

        if (_crestRenderer != null)
        {
            _crestRenderer.color = GetCrestColor(_currentChoice);
            _crestRenderer.transform.localScale = Vector3.one * 0.34f;
        }

        if (_roleMarkRenderer != null)
            _roleMarkRenderer.color = GetRoleMarkColor(_currentChoice);

        if (_crestLabel != null)
        {
            _crestLabel.text = GetCharacterGlyph(_currentChoice);
            _crestLabel.color = GetCrestLabelColor(_currentChoice);
        }
    }

    private void ApplyRoleShape()
    {
        if (_roleMarkRenderer == null)
            return;

        switch (_currentChoice)
        {
            case PlayableCharacterChoice.HumanRanger:
                _roleMarkRenderer.transform.localPosition = new Vector3(0f, -0.72f, 0f);
                _roleMarkRenderer.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
                break;

            case PlayableCharacterChoice.HumanArcanist:
                _roleMarkRenderer.transform.localPosition = new Vector3(0f, -0.72f, 0f);
                _roleMarkRenderer.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
                break;

            case PlayableCharacterChoice.HumanVanguard:
            default:
                _roleMarkRenderer.transform.localPosition = new Vector3(0f, -0.67f, 0f);
                _roleMarkRenderer.transform.localScale = new Vector3(0.32f, 0.2f, 1f);
                break;
        }
    }

    private void ApplySorting(Renderer renderer, int offset)
    {
        if (_baseRenderer != null && renderer != _baseRenderer)
        {
            renderer.sortingLayerID = _baseRenderer.sortingLayerID;
            renderer.sortingOrder = _baseRenderer.sortingOrder + offset;
            return;
        }

        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = offset;
    }

    private static float GetAuraScale(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return 1.12f;
            case PlayableCharacterChoice.HumanArcanist:
                return 1.26f;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return 1.42f;
        }
    }

    private static string GetCharacterGlyph(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "R";
            case PlayableCharacterChoice.HumanArcanist:
                return "A";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "V";
        }
    }

    private static Color GetCrestColor(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return new Color(0.05f, 0.32f, 0.08f, 0.92f);
            case PlayableCharacterChoice.HumanArcanist:
                return new Color(0.08f, 0.22f, 0.42f, 0.92f);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return new Color(0.52f, 0.36f, 0.08f, 0.92f);
        }
    }

    private static Color GetRoleMarkColor(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return new Color(0.38f, 1f, 0.34f, 0.9f);
            case PlayableCharacterChoice.HumanArcanist:
                return new Color(0.42f, 0.9f, 1f, 0.9f);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return new Color(1f, 0.78f, 0.25f, 0.9f);
        }
    }

    private static Color GetCrestLabelColor(PlayableCharacterChoice choice)
    {
        return choice == PlayableCharacterChoice.HumanVanguard ? Color.black : Color.white;
    }
}
