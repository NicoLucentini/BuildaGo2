using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public void Start()
    {
        //transform.rotation = Camera.main.transform.rotation;
    }
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform.position.WithY(transform.position.y));
    }
}
