using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LoadingSpinner : MonoBehaviour {

    public Image image;
    public float speed = .5f;
    public float spin;
    public bool invert = true;
    
    private void Start() {
        image = GetComponentInChildren<Image>();
        Assert.IsTrue(image);
    }
    private void Update() {
        spin += Time.deltaTime * speed;
        image.fillClockwise = Mathf.FloorToInt(spin) % 2 == 0;
        image.fillAmount = spin % 1;
        if (image.fillClockwise)
            image.fillAmount = 1 - image.fillAmount;
        if (invert)
            image.fillAmount = 1 - image.fillAmount;
    }
}