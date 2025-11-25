using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool incremental = false;
    public bool doesMove = true;
    public Vector3 offset = Vector3.zero;
    public void Start()
    {
        //transform.rotation = Camera.main.transform.rotation;
        if (!doesMove) {
            if (incremental)
            {
                transform.forward = Camera.main.transform.forward;
            }
            else
            {
                transform.LookAt(Camera.main.transform.position.WithY(transform.position.y));
            }
        }
    }
    private void LateUpdate()
    {
        if (!doesMove) return;

        if (incremental)
        {
            transform.forward = Camera.main.transform.forward;
        }
        else {
            //transform.LookAt(Camera.main.transform.position.WithY(transform.position.y));
            transform.forward = Camera.main.transform.forward;
        }
    }
}
