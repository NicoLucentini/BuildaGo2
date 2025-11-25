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
        transform.localScale *= 1.3f;
        StartCoroutine(Anim());
    }
    IEnumerator Anim() {
        float timer = 0;
        Vector3 initScale = transform.localScale;
        while (timer < 3f) { 
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(initScale, initScale * .7f, timer / 3f);
            transform.position += Vector3.up * Time.deltaTime * 0.2f;
            yield return null;
        }
        Destroy(gameObject);
    }
}
