using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RenderBoundsHeight : MonoBehaviour
{
    private string heightPropertyName = "_maxH";
    private string currentHeightProperyName = "_CurrentCompletition";

    public MaterialPropertyBlock mpb;
    public Renderer rend;

    //I dont think this is needed
    private float currentCompletition = 1;

    public float objectHeight = 0;

    Coroutine cr;

    public bool isSet = false;



    private void OnValidate()
    {
        Debug.Log($"{gameObject.name} h: " + GetComponent<Renderer>().bounds.size.y);
        
        if(!isSet)
        { 
            rend = GetComponent<Renderer>();
            Init();
            isSet = true;
        }
        UpdateCurrentCompletition(currentCompletition);
    }
    public void Init()
    {
        mpb = new MaterialPropertyBlock();
        float mult = 1 / transform.localScale.x;
        objectHeight = rend.bounds.size.y * mult;

        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(currentHeightProperyName, mult);
        mpb.SetFloat(heightPropertyName, objectHeight);
        rend.SetPropertyBlock(mpb);
    }
    public void Build(Texture texture)
    {
        Shader shader = Shader.Find("Shader Graphs/CompletitionShader");
        Material material = new Material(shader);
        material.SetTexture("_BaseMap", texture);
        material.SetFloat("_Alpha", 0.8f);
        material.SetColor("_IncompleteColor", Color.red);

        if (rend == null) rend = GetComponent<Renderer>();

        Init();
        UpdateCurrentCompletition(currentCompletition);
    }
    public float GetObjectHeight() => objectHeight; 

    public void UpdateCurrentCompletition(float value) {
        currentCompletition = value;
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat(currentHeightProperyName, value);
        rend.SetPropertyBlock(mpb);
    }
    public void UpdateCurrentCompletitionProgressive(float initValue, float targetValue, float duration) {

        if (cr != null) StopCoroutine(cr);

        cr = StartCoroutine(UpdateCompletition(initValue, targetValue, duration));
    }
    IEnumerator UpdateCompletition(float initValue, float targetValue, float duration) {
        float timer = 0;
        while (timer < duration)
        {
            UpdateCurrentCompletition(Mathf.Lerp(initValue, targetValue, timer));
            timer += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }
}
