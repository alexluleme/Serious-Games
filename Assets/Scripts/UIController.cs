using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour {

	public GameObject gameText;
	public GameObject countdown;
	public GameObject floatingText;

	private float gameTextDuration;
	private float countdownTimer;

	void Update() {
		//Update the game text
		if (gameTextDuration > 0) {
			CanvasGroup temp = gameText.GetComponent<CanvasGroup>();
			if(temp.alpha < 1) {
				temp.alpha += Time.deltaTime * 2;
			} else {
				gameTextDuration -= Time.deltaTime;
			}
		} else {
			CanvasGroup temp = gameText.GetComponent<CanvasGroup>();
			if(temp.alpha > 0) {
				temp.alpha -= Time.deltaTime * 2;
			}
		}

		//Update the countdown timer
		if (countdownTimer > 0) {
			countdown.GetComponentInChildren<Image>().fillAmount = (countdownTimer + 1) % Mathf.Floor(countdownTimer + 1);
			countdown.GetComponentInChildren<Text>().text = Mathf.Ceil(countdownTimer).ToString("F0");
			countdown.GetComponentInChildren<CanvasGroup>().alpha = 1;
			countdownTimer -= Time.deltaTime;
		} else {
			countdown.GetComponentInChildren<Image>().fillAmount = 0;
			countdown.GetComponentInChildren<Text>().text = "Start";
			CanvasGroup temp = countdown.GetComponentInChildren<CanvasGroup>();
			if(temp.alpha > 0) {
				temp.alpha -= Time.deltaTime * 2;
			}
		}
	}

	public void showGameText(string text, float duration) {
		gameText.GetComponentInChildren<Text> ().text = text;
		gameTextDuration = duration;
	}

	public void startCountdown(float timer) {
		countdownTimer = timer;
	}

	public IEnumerator showFloatingText(string text, Color color) {
		GameObject temp = Instantiate (floatingText) as GameObject;
		temp.GetComponent<RectTransform> ().SetParent (GetComponentInChildren<Canvas> ().transform);
		temp.GetComponent<RectTransform> ().localPosition = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0);
		temp.GetComponent<RectTransform> ().localScale = Vector3.one;
		temp.GetComponent<Text>().text = text;
		temp.GetComponent<Text>().color = color;

		float timer = 1;
		while (timer > 0) {
			timer -= Time.deltaTime;
			temp.GetComponent<Text>().rectTransform.Translate(Vector2.up * Time.deltaTime * 25);
			temp.GetComponent<CanvasGroup>().alpha = timer;
			yield return null;
		}

		Destroy (temp);
	}
}
