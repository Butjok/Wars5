using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSubscene : MonoBehaviour {

    public SubsceneRoot subsceneRootPrefab;
    public SubsceneRoot subsceneRoot;

    public void Awake() {
        subsceneRoot = Instantiate(subsceneRootPrefab);
        LightProbes.Tetrahedralize();
    }

    /*public string sceneName = "";
    public void Start() {
        Load();
    }
    [ContextMenu(nameof(Load))]
    public void Load() {
        var loading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loading.allowSceneActivation = true;
        loading.completed += _ => {
            var root = FindObjectOfType<SubsceneRoot>();
            LightProbes.Tetrahedralize();
        };
    }*/
}