using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PlayerView2 : MonoBehaviour {

    public const int left = 0;
    public const int right = 1;
    public const int top = 1;
    public const int bottom = 0;
    public const int horizontal = 0;
    public const int vertical = 1;

    public static (Vector2 min, Vector2 max)[] anchors = {
        (new Vector2(0, 0), new Vector2(1, 0)),
        (new Vector2(0, 1), new Vector2(1, 1))
    };

    public static PlayerView2 GetPrefab(PersonName personName) {
        return "PlayerView2".LoadAs<PlayerView2>();
    }
    public static PlayerView2 DefaultPrefab => GetPrefab(PersonName.Natalie);

    public CreditsText creditsText;
    public PowerMeterStripe powerMeterStripe;
    public Image portrait;
    public Image portraitInsetImage;
    public Image background;
    public Image backgroundInsetImage;
    public float height = 150;

    public GameObject root;
    public RectTransform portraitPlaceholder;
    public RectTransform creditsTextPlaceholder;
    public RectTransform powerMeterStripePlaceholder;
    public RectTransform backgroundPlaceholder;
    public RectTransform placeholderRoot;
    public PersonName coName;

    [Command]
    public void SetCreditsAmount(int amount, bool animate = true) {
        creditsText.SetAmount(amount, animate);
    }
    [Command]
    public void SetPowerStripeMeter(int value, int max, bool animate = true, bool playSoundOnFull = true) {
        powerMeterStripe.SetProgress((float)value / max, animate, 
            value == max && playSoundOnFull ? () => Debug.Log("power meter full") : null);
    }

    public void Show(Vector2Int position, int credits, int abilityMeter, int maxAbilityMeter, Color color, PersonName coName) {
        
        root.SetActive(true);

        Assert.IsTrue(position[horizontal] is left or right);
        Assert.IsTrue(position[vertical] is top or bottom);

        var rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);

        rectTransform.pivot = new Vector2(.5f, position[vertical]);
        rectTransform.anchorMin = anchors[position[vertical]].min;
        rectTransform.anchorMax = anchors[position[vertical]].max;
        rectTransform.anchoredPosition = new Vector2(0, 0);

        placeholderRoot.localScale = new Vector3(position[horizontal] == left ? 1 : -1, 1, 1);

        creditsText.transform.position = creditsTextPlaceholder.position;
        powerMeterStripe.transform.position = powerMeterStripePlaceholder.position;
        portrait.rectTransform.position = portraitPlaceholder.position;
        background.rectTransform.position = backgroundPlaceholder.position;

        SetCreditsAmount(credits, false);
        SetPowerStripeMeter(abilityMeter, maxAbilityMeter, false);
        Color = color;
        this.coName = coName;
        Mood = Mood.Normal;
    }
    
    public void Hide() {
        root.SetActive(false);
    }

    [Command]
    public Color Color {
        set {
            backgroundInsetImage.color = value;
            var outline=portraitInsetImage.GetComponent<Outline>();
            if (outline)
                outline.effectColor = value;
        }
    }

    [Command]
    public Mood Mood {
        set => portraitInsetImage.sprite = Persons.TryGetPortrait(coName, value);
    }
}