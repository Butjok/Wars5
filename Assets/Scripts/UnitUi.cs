using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class UnitUi : MonoBehaviour {

    public static UnitUi Prefab => Resources.Load<UnitUi>("UnitUi2");

    public RectTransform root;
    public UiLabel hpLabel;
    public TMP_Text hpText;
    public Image lowFuelIcon;
    public Image lowAmmoIcon;
    public Image cargoIcon;

    public Camera camera;
    public Transform target;
    public float radius = 1;
    public Vector3 offset = new();
    public float ratio = 1;

    public Image circle;
    public float circlePadding = 10;

    public RectTransform rectTransform;

    private void Reset() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void LateUpdate() {
        if (camera.TryCalculateScreenCircle(target.position + offset, radius, out var center, out float halfSize)) {
            rectTransform.EncapsulateScreenRect(center, new Vector2(halfSize, halfSize * ratio));
            if (circle) {
                circle.rectTransform.EncapsulateScreenRect(center, new Vector2(halfSize, halfSize) + new Vector2(circlePadding, circlePadding));
                circle.materialForRendering.SetFloat("_Size", (halfSize + circlePadding) * 2);
            }
        }
    }

    public bool ShouldBeActive => hpLabel.gameObject.activeSelf || ShowCargoIcon || ShowLowAmmoIcon || ShowLowFuelIcon;

    [Command]
    public void SetHp(int hp, int maxHp = 10) {
        if (!hpText)
            return;
        if (hp == maxHp) {
            hpLabel.gameObject.SetActive(false);
        }
        else {
            hpLabel.gameObject.SetActive(true);
            hpLabel.text.text = hp.ToString();
        }
        gameObject.SetActive(ShouldBeActive);
    }

    [Command]
    public bool ShowLowFuelIcon {
        get => lowFuelIcon && lowFuelIcon.gameObject.activeSelf;
        set {
            if (lowFuelIcon)
                lowFuelIcon.gameObject.SetActive(value);
            gameObject.SetActive(ShouldBeActive);
        }
    }
    [Command]
    public bool ShowLowAmmoIcon {
        get => lowAmmoIcon && lowAmmoIcon.gameObject.activeSelf;
        set {
            if (lowAmmoIcon)
                lowAmmoIcon.gameObject.SetActive(value);
            gameObject.SetActive(ShouldBeActive);
        }
    }
    [Command]
    public bool ShowCargoIcon {
        get => cargoIcon && cargoIcon.gameObject.activeSelf;
        set {
            if (cargoIcon)
                cargoIcon.gameObject.SetActive(value);
            gameObject.SetActive(ShouldBeActive);
        }
    }
}