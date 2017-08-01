using UnityEngine;
using System.Collections;

public class BloodDecal : MonoBehaviour 
{
	public Vector3 endScale;
	public AnimationCurve scaleCurve;
	public AnimationCurve colorCurve;
	public Gradient colorGradient;
	public float duration;

	private void Start()
	{
		StartCoroutine (BloodEffectCoroutine ());
	}

	private IEnumerator BloodEffectCoroutine()
	{
		float timer = 0.0f;
		Vector3 startScale = this.transform.localScale;
		Color startColor = this.GetComponent<SpriteRenderer> ().color;

		while (timer <= duration) 
		{
			Debug.Log (timer);
			//if (GameManager.instance.IsPaused)
			//	yield return null;

			Vector3 newScale = Vector3.Lerp (startScale, endScale, scaleCurve.Evaluate (timer / duration));
			transform.localScale = newScale;

			Color newColor = colorGradient.Evaluate (colorCurve.Evaluate (timer / duration));
			this.GetComponent<SpriteRenderer> ().color = newColor;

			timer += Time.deltaTime;
			yield return null;
		}

		transform.localScale = endScale;
		this.GetComponent<SpriteRenderer> ().color = colorGradient.Evaluate (1.0f);
	}
}
