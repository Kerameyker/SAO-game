using UnityEngine;

/// <summary>
/// Камера від третьої особи, що обертається навколо персонажа мишею —
/// такий самий базовий принцип, як у Muck.
/// Повісь на порожній GameObject "CameraRig", а Main Camera зроби його дочірнім об'єктом,
/// АБО повісь прямо на Main Camera і вкажи target.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Ціль")]
    public Transform target;              // перетягни сюди Player
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // точка над головою персонажа

    [Header("Орбіта")]
    public float distance = 6f;
    public float minDistance = 2.5f;
    public float maxDistance = 10f;
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float scrollSensitivity = 4f;

    private float yaw = 0f;
    private float pitch = 20f;
    private bool cursorLocked = true;

    void Start()
    {
        SetCursorLock(true);
    }

    void Update()
    {
        // Escape тимчасово вивільняє курсор (щоб зручно було натискати на UI)
        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLock(!cursorLocked);

        if (!cursorLocked) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focusPoint = target.position + targetOffset;
        Vector3 desiredPos = focusPoint - rot * Vector3.forward * distance;

        // проста перевірка, щоб камера не залазила крізь стіни/землю
        // (ігноруємо влучання в самого персонажа, інакше камера смикається)
        if (Physics.Linecast(focusPoint, desiredPos, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore)
            && hit.transform.root != target.root)
        {
            desiredPos = hit.point;
        }

        transform.position = desiredPos;
        transform.LookAt(focusPoint);
    }

    void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
