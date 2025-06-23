using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FollowMainCameraInWorld : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private bool follow = true;

    [SerializeField]
    private bool isUi = true;

    private Transform t;
    private Vector3 screenPos;
    public Transform target;



    private void Awake()
    {
        if (isUi)
        {
            t = GetComponent<RectTransform>();
        }
        else {
            t = transform;
        }
    }
   
    private void LateUpdate()
    {
        if(follow)
        { 
            screenPos = Camera.main.WorldToScreenPoint(target.position);
            t.position = screenPos + offset;
        }
    }
}
