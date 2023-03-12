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

    private static Dictionary<Vector2Int, PlayerView2> playerViews = new() {
        [new Vector2Int(left, top)] = null,
        [new Vector2Int(right, top)] = null,
        [new Vector2Int(left, bottom)] = null,
        [new Vector2Int(right, bottom)] = null,
    };

    public static PlayerView2 GetPrefab(PersonName personName) {
        return "PlayerView2".LoadAs<PlayerView2>();
    }

    public Player player;
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

    [Command]
    public void SetCreditsAmount(int amount, bool animate = true) {
        creditsText.SetAmount(amount, animate);
    }
    [Command]
    public void SetPowerStripeMeter(int value, bool animate = true, bool playSoundOnFull = true) {
        powerMeterStripe.SetProgress((float)value / Rules.MaxAbilityMeter(player), animate, 
            playSoundOnFull ? () => Debug.Log("power meter full") : null);
    }

    public void Show() {
        
        root.SetActive(true);

        Assert.IsTrue(player.uiPosition[horizontal] is left or right);
        Assert.IsTrue(player.uiPosition[vertical] is top or bottom);
        Assert.IsNull(playerViews[player.uiPosition]);

        playerViews[player.uiPosition] = this;

        var rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);

        rectTransform.pivot = new Vector2(.5f, player.uiPosition[vertical]);
        rectTransform.anchorMin = anchors[player.uiPosition[vertical]].min;
        rectTransform.anchorMax = anchors[player.uiPosition[vertical]].max;
        rectTransform.anchoredPosition = new Vector2(0, 0);

        placeholderRoot.localScale = new Vector3(player.uiPosition[horizontal] == left ? 1 : -1, 1, 1);

        creditsText.transform.position = creditsTextPlaceholder.position;
        powerMeterStripe.transform.position = powerMeterStripePlaceholder.position;
        portrait.rectTransform.position = portraitPlaceholder.position;
        background.rectTransform.position = backgroundPlaceholder.position;

        SetCreditsAmount(player.Credits, false);
        SetPowerStripeMeter(player.AbilityMeter, false);
        Color = player.Color;
        Mood = Mood.Normal;
    }
    
    public void Hide() {
        playerViews[player.uiPosition] = null;
        root.SetActive(false);
    }

    [Command]
    public Color Color {
        set => backgroundInsetImage.color = value;
    }

    [Command]
    public Mood Mood {
        set => portraitInsetImage.sprite = People.TryGetPortrait(player.coName, value);
    }
}