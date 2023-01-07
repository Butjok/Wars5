using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class DifferenceEffect : MonoBehaviour {
	public float intensity;
	public Material material;
	public bool negate;
	// Creates a private material used to the effect
	void Awake ()
	{
		//material = new Material( Shader.Find("Hidden/Difference") );
	}
    
	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Multiplier", negate ? -1 : 1);
		Graphics.Blit (source, destination, material);
	}
}