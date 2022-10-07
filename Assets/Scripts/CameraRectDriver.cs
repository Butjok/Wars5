using Butjok.CommandLine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraRectDriver : MonoBehaviour {

    public Camera camera;

    public Rect cameraOffscreenRect = new(0, .2f, 0, .6f);
    public Rect cameraOnscreenRect = new(0, .2f, .5f, .6f);
    public Tweener cameraRectTweener;
    public float rectDuration = .5f;
    public Ease rectEase = Ease.OutSine;
    public float cameraMinimalRectSize = .01f;

    [Range(-1, 1)] public int side = -1;

    [ContextMenu(nameof(Initialize))]
    private void Initialize() {
        camera = GetComponentInChildren<Camera>();
        Assert.IsTrue(camera);
    }
    
    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        Initialize();
    }
    
    private void Awake() {
        EnsureInitialized();
    }

    private Rect TransformRect(Rect rect) {

        if (side <= 0)
            return rect;

        var flip = Matrix4x4.Translate(new Vector2(1, 0)) * Matrix4x4.Scale(new Vector2(-1, 1));
        var a = flip.MultiplyPoint(rect.min);
        var b = flip.MultiplyPoint(rect.max);
        var minX = Mathf.Min(a.x, b.x);
        var maxX = a.x + b.x - minX;
        var minY = Mathf.Min(a.y, b.y);
        var maxY = a.y + b.y - minY;
        rect = new Rect(minX, minY, maxX - minX, maxY - minY);

        return rect;
    }

    private void Update() {
        camera.enabled = camera.rect.width > cameraMinimalRectSize && camera.rect.height > cameraMinimalRectSize;
    }

    [ContextMenu(nameof(Show))]
    public void Show() {
        EnsureInitialized();
        
        cameraRectTweener?.Kill();
        camera.rect = TransformRect(cameraOffscreenRect);
        cameraRectTweener = camera.DORect(TransformRect(cameraOnscreenRect), rectDuration).SetEase(rectEase);
    }

    [ContextMenu(nameof(Hide))]
    public void Hide() {
        EnsureInitialized();
        
        cameraRectTweener?.Kill();
        camera.rect = TransformRect(cameraOffscreenRect);
    }
}