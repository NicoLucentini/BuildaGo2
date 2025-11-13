using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIReward : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;

    public void Set(string message, Color color, Vector3 position) {
        text.text = message;
        text.color = color;
        image.color = color;
        transform.position = position;
        StartCoroutine(Anim());
    }
    IEnumerator Anim() {
        float timer = 0;
        Vector3 initScale = transform.localScale;
        while (timer < .8f) { 
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initScale, Vector3.one * .1f, 1 -  timer / .8f);
            transform.position += Vector3.up * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
