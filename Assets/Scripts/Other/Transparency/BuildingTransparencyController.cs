using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingTransparencyController : MonoBehaviour
{
    [SerializeField] private LayerMask buildingLayer;

    private List<MakeTransparent> objectsToTransparent = new();

    void Update()
    {
        HandleTransparency();
    }

    void HandleTransparency()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 1000.0F, buildingLayer);

        List<MakeTransparent> temp = new List<MakeTransparent>();
        foreach (var hit in hits.ToList())
        { 
            if (hit.collider.TryGetComponent(out MakeTransparent mt)) {

                if (!objectsToTransparent.Contains(mt))
                    objectsToTransparent.Add(mt);

                mt.MakeTransparents();
                temp.Add(mt);
            }
        }

        for(int i = objectsToTransparent.Count - 1; i >= 0; i--)
        {
            if (!temp.Contains(objectsToTransparent[i])) {
                objectsToTransparent[i].RestoreCurrent();
                objectsToTransparent.Remove(objectsToTransparent[i]);
            }
        }
    }
}
