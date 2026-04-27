using UnityEngine;

public class LevelCamera : MonoBehaviour
{
    [Header("Target")]
    public GridMovement player;

    [Header("Orbit")]
    public float yaw              = 45f;
    public float pitch            = 35f;
    public float pitchMin         = 10f;
    public float pitchMax         = 80f;
    public float orbitSensitivity = 3f;

    [Header("Zoom")]
    public float distance    = 12f;
    public float distanceMin = 4f;
    public float distanceMax = 25f;
    public float zoomSpeed   = 5f;

    [Header("2D Top-Down")]
    public float orthoSize = 6f;

    private Camera cam;
    private bool is2D;
    private Light[] sceneLights;
    private LightShadows[] originalShadows;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<GridMovement>();

        sceneLights     = FindObjectsByType<Light>(FindObjectsSortMode.None);
        originalShadows = new LightShadows[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
            originalShadows[i] = sceneLights[i].shadows;

        GameEvents.OnPerspectiveSwitch += HandlePerspectiveSwitch;
    }

    void OnDestroy()
    {
        GameEvents.OnPerspectiveSwitch -= HandlePerspectiveSwitch;
    }

    void HandlePerspectiveSwitch(bool entering2D)
    {
        is2D = entering2D;

        if (entering2D)
        {
            if (player != null)
                transform.SetPositionAndRotation(player.TargetPosition + Vector3.up * distance, Quaternion.Euler(90f, 0f, 0f));

            cam.orthographic     = true;
            cam.orthographicSize = orthoSize;

            foreach (var l in sceneLights)
                if (l != null) l.shadows = LightShadows.None;
        }
        else
        {
            cam.orthographic = false;

            for (int i = 0; i < sceneLights.Length; i++)
                if (sceneLights[i] != null) sceneLights[i].shadows = originalShadows[i];
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (is2D)
        {
            Vector3 pos = player.transform.position;
            transform.position = new Vector3(pos.x, pos.y + distance, pos.z);
            return;
        }

        if (InGameMenu.CameraInputBlocked || !GameManager.Instance.IsInLevel) return;

        if (Input.GetMouseButton(1))
        {
            yaw   += Input.GetAxis("Mouse X") * orbitSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * orbitSensitivity;
            pitch  = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, distanceMin, distanceMax);

        float p = pitch * Mathf.Deg2Rad;
        float y = yaw   * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Sin(y) * Mathf.Cos(p),
            Mathf.Sin(p),
            Mathf.Cos(y) * Mathf.Cos(p)
        ) * distance;

        transform.position = player.transform.position + offset;
        transform.LookAt(player.transform.position + Vector3.up * 0.5f);
    }
}
