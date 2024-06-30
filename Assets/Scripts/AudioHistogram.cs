using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AudioSource))]
public class AudioHistogram : MonoBehaviour {
    public AudioSource audioSource;
    public float[] spectrum = new float[256];
    public Material material;

    public void Reset() {
        audioSource = GetComponent<AudioSource>();
        Assert.IsTrue(audioSource);
    }

    void Update() {
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (var i = 0; i < spectrum.Length; i++)
            spectrum[i] = Mathf.Log(spectrum[i] + 1, 2);

        material.SetFloatArray("_AudioSpectrum", spectrum);
    }
}