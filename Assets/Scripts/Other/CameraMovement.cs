using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement instance;

    public static Action<Vector3> onCameraRotate;

    private Camera cam;
    [SerializeField]
    private CameraMovementType cameraMovementType;


    [Header("Movement")]

    private bool lockCamera;

    [Header("Drag")]
    public Vector3 initialPosition;
    public float dragSpeed = 15f;
    public float smoothTime = 0.1f; // Smoothness factor
    private Vector3 targetPosition; // The position to move towards
    private Vector3 velocity = Vector3.zero; // Used for smoothing
    private Vector3 dragOrigin;

    [Header("Zoom")]
    public float initialZoom = 15; //Initial

    public Vector2 zoomLimits = new Vector2(5f, 25f);
    public float zoomSpeed = 10;
    public float zoomSmoothTime = 0.1f; // Smoothness factor
    public float lookAtZoom;

    private float zoomVelocity = 0f;
    private float targetZoom;

    [Header("Pan Like rts")]
    public float panSpeed = 10f;        // Movement speed
    public float edgeSize = 10f;        // Distance from screen edge in pixels
   
    public float rotateY;

    enum CameraMovementType{PAN_RTS, PAN_DRAG }

    
    private void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
    }
    void Start()
    {
        initialPosition = transform.position;
        targetPosition = transform.position; 
        initialZoom = cam.orthographicSize;
        targetZoom = cam.orthographicSize;
    }
    public void Restart() {
        transform.position = initialPosition;
        cam.orthographicSize = initialZoom;
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;
    }
    
    public void FreeMovement(Transform b) {
        lockCamera = false;
        StartCoroutine(CTZoom(1.5f, initialZoom));
    }
    public void LookAtTarget(Transform target, bool zoom = false, bool lockCam = false) {

        StopAllCoroutines();

        lockCamera = lockCam;

        var tp = target.position - transform.forward * 30;
        var duration = MathHelper.Map(Vector3.Distance(tp, transform.position), 0, 30, 0.5f, 1.5f);

        if(zoom)
            StartCoroutine(CTZoom(1.5f, lookAtZoom));

        StartCoroutine(CTLookAtTarget(duration, tp));
    }
    IEnumerator CTZoom(float time, float targetZoom) {
        float timer = 0;
        float startZoom = cam.orthographicSize;
        while (timer < time)
        {
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, timer / time);
            timer += Time.deltaTime;
            yield return null;
        }
        targetZoom = cam.orthographicSize;
        yield return null;
    }
    IEnumerator CTLookAtTarget(float time, Vector3 tp)
    {
        float timer = 0;
        Vector3 startingPos = transform.position;
        while (timer < time)
        {
            if (lockCamera)
            {
                transform.position = Vector3.Lerp(startingPos, tp, timer / time);
            }
            else {
                targetPosition = Vector3.Lerp(startingPos, tp, timer / time);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        targetPosition = transform.position;
        yield return null;
    }
  

    void Update()
    {
        if (lockCamera) return;
        //Zoom

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, zoomLimits.x, zoomLimits.y);
        }

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);

        if (cameraMovementType == CameraMovementType.PAN_DRAG)
        {
            if (Input.GetMouseButtonDown(0)) // LeftClick
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            if (Input.GetMouseButton(0)) // LeftClick holding
            {
                Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                Vector3 move = new Vector3(-difference.x * dragSpeed, 0, -difference.y * dragSpeed);

                targetPosition += move;
                dragOrigin = Input.mousePosition;
            }

            // Smoothly move camera toward the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else if(cameraMovementType == CameraMovementType.PAN_RTS) {

            if (Input.GetKeyUp(KeyCode.E)) {

                float d = 10;
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100,1<<15 )) {
                    d = Vector3.Distance(transform.position, hit.point);
                }

                pivot = transform.position + transform.forward * d; // a point in front of camera

                transform.RotateAround(pivot.WithY(transform.position.y), Vector3.up, 90f);
                onCameraRotate?.Invoke(transform.rotation.eulerAngles);
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                float d = 10;
                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, 1 << 15))
                {
                    d = Vector3.Distance(transform.position, hit.point);
                }
                pivot = transform.position + transform.forward * d; // a point in front of camera

                transform.RotateAround(pivot.WithY(transform.position.y), Vector3.up, -90f);

                onCameraRotate?.Invoke(transform.rotation.eulerAngles);

                targetPosition = transform.position;
            }
            
            Vector3 move = Vector3.zero;

            if (Input.mousePosition.x >= Screen.width - edgeSize)
                move.x += 1;
            if (Input.mousePosition.x <= edgeSize)
                move.x -= 1;
            if (Input.mousePosition.y >= Screen.height - edgeSize)
                move.z += 1;
            if (Input.mousePosition.y <= edgeSize)
                move.z -= 1;


            if (move.x != 0 || move.z != 0) {
                Vector3 up = (transform.up * move.z).WithY(0);
                targetPosition += (up + transform.right * move.x).normalized * panSpeed * Time.deltaTime;
                transform.position = targetPosition;
            }
        }
    }

    private Vector3 pivot;

    private void OnDrawGizmos()
    {
        Color color = Color.red;

        Gizmos.DrawCube(pivot, Vector3.one * 0.5f);
    }
}