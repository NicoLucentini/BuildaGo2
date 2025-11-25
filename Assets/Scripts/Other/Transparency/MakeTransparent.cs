using System.Collections.Generic;
using UnityEngine;

public class MakeTransparent :MonoBehaviour{
    [SerializeField]private Renderer[] currentRenderer;

    [SerializeField]private Dictionary<Renderer, Material[]> originalMaterials = new();
    [SerializeField]private Dictionary<Renderer, Material[]> transparentMaterials = new();

    [SerializeField] private float transparentAlpha = 0.3f;


    void Start() { 
        for(int i = 0; i< currentRenderer.Length; i++)
        {
            originalMaterials.Add(currentRenderer[i], currentRenderer[i].materials);
        }
    }

    public void RestoreCurrent()
    {
        foreach (var k in originalMaterials) {
            k.Key.materials = originalMaterials[k.Key];
        }
    }
    public void MakeTransparents()
    {
        Debug.Log("Make Transparents");
        foreach (var k in originalMaterials)
        {
            if(!transparentMaterials.ContainsKey(k.Key))
            { 
                var transparentsMats = new Material[originalMaterials[k.Key].Length];

                for (int i = 0; i < transparentsMats.Length; i++)
                {
                    var originalMaterial = originalMaterials[k.Key][i];
                    Material newMat = new Material(originalMaterial);
                    newMat.name = originalMaterial.name + " Transparent";


                    EnableTransparencyMode(newMat);
                    SetMaterialAlpha(newMat, transparentAlpha);

                    transparentsMats[i] = newMat;
                }

                transparentMaterials.Add(k.Key, transparentsMats);
            }
            k.Key.materials = transparentMaterials[k.Key];
        }

    }
    void SetMaterialAlpha(Material mat, float alpha)
    {
        Color c = mat.GetColor("_BaseColor");
        c.a = alpha;

        // Apply
        mat.SetColor("_BaseColor", c);

    }
    void EnableTransparencyMode(Material mat)
    {
        mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        mat.SetOverrideTag("RenderType", "Transparent");

        // Proper blending
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        // Disable ZWrite for transparent
        mat.SetInt("_ZWrite", 0);

        // Transparent render queue
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Shader keywords
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHATEST_ON");
    }
}
