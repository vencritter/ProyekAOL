using UnityEngine;

public class ARGestureManipulator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.5f;

    [Header("Scale Settings")]
    public float scaleSpeed = 0.001f;
    public float minScale = 0.05f;
    public float maxScale = 1.0f;

    [Header("Move Settings")]
    public float moveSpeed = 0.001f;

    void Update()
{
    // MOBILE
    if (Input.touchCount == 1)
    {
        HandleMove();
    }
    else if (Input.touchCount == 2)
    {
        HandleScale();
        HandleRotation();
    }

    // PC TESTING
    #if UNITY_EDITOR || UNITY_STANDALONE
    HandleMouse();
    #endif
}

    void HandleMove()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Moved)
        {
            Vector3 move = new Vector3(
                touch.deltaPosition.x * moveSpeed,
                0,
                touch.deltaPosition.y * moveSpeed
            );

            transform.Translate(move, Space.World);
        }
    }

    void HandleScale()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
        float touchDeltaMag = (touch0.position - touch1.position).magnitude;

        float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

        Vector3 newScale = transform.localScale + Vector3.one * deltaMagnitudeDiff * scaleSpeed;

        float clampedScale = Mathf.Clamp(newScale.x, minScale, maxScale);
        transform.localScale = Vector3.one * clampedScale;
    }

    void HandleRotation()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        Vector2 prevDir = touch0PrevPos - touch1PrevPos;
        Vector2 currDir = touch0.position - touch1.position;

        float angleDelta = Vector2.SignedAngle(prevDir, currDir);

        transform.Rotate(Vector3.up, -angleDelta * rotationSpeed, Space.World);
    }

    void HandleMouse()
{
    // LEFT CLICK → MOVE
    if (Input.GetMouseButton(0))
    {
        float moveX = Input.GetAxis("Mouse X") * moveSpeed * 100;
        float moveZ = Input.GetAxis("Mouse Y") * moveSpeed * 100;

        transform.Translate(new Vector3(moveX, 0, moveZ), Space.World);
    }

    // RIGHT CLICK → ROTATE
    if (Input.GetMouseButton(1))
    {
        float rot = Input.GetAxis("Mouse X") * rotationSpeed * 100;
        transform.Rotate(Vector3.up, -rot, Space.World);
    }

    // SCROLL → SCALE
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (scroll != 0)
    {
        Vector3 newScale = transform.localScale + Vector3.one * scroll;

        float clamped = Mathf.Clamp(newScale.x, minScale, maxScale);
        transform.localScale = Vector3.one * clamped;
    }
}
}