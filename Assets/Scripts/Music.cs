using DG.Tweening;
using UnityEngine;

public static class Music {

	public static float fadeOutDuration = .5f;
	public static float fadeInDuration = 1f;

	public static Lazy<AudioSource> source = new(() => {
		var go = new GameObject(nameof(Music));
		Object.DontDestroyOnLoad(go);
		var result = go.AddComponent<AudioSource>();
		result.loop = true;
		result.spatialize = false;
		result.volume = 0;
		result.Stop();
		return result;
	});

	public static Tween currentTween;

	public static Tween Play(AudioClip clip) {
		var sequence = DOTween.Sequence();
		sequence.Append(Stop());
		sequence.AppendCallback(() => {
			source.v.clip = clip;
			source.v.volume = 0;
			source.v.Play();
		});
		sequence.Append(source.v.DOFade(1, fadeInDuration));
		return sequence;
	}

	public static Tween Stop() {
		if (!source.v.isPlaying)
			return null;
		if (source.v.isPlaying && source.v.volume <= Mathf.Epsilon) {
			source.v.volume = 0;
			source.v.Stop();
			return null;
		}
		var sequence = DOTween.Sequence();
		sequence.Append(source.v.DOFade(0, fadeOutDuration));
		sequence.AppendCallback(() => {
			source.v.volume = 0;
			source.v.Stop();
		});
		return sequence;
	}
}