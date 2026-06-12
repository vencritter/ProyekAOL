using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to any GameObject. Assign the Left and Right buttons —
/// hold either button to continuously rotate the target on the Y axis.
/// </summary>
public class RotateButtons : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Object to rotate. Defaults to this GameObject.")]
    public GameObject targetObject;

    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second while button is held.")]
    public float rotateSpeed = 45f;

    [Tooltip("Lerp the rotation for a smoother feel.")]
    public bool smooth = false;
    [Range(1f, 30f)]
    public float smoothSpeed = 10f;

    // ── Private ────────────────────────────────────────────────────────────────
    private bool  _rotatingLeft;
    private bool  _rotatingRight;
    private float _targetY;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (targetObject == null)
            targetObject = gameObject;

        _targetY = targetObject.transform.eulerAngles.y;

        RegisterButton(leftButton,  onDown: () => _rotatingLeft  = true,
                                    onUp:   () => _rotatingLeft  = false);

        RegisterButton(rightButton, onDown: () => _rotatingRight = true,
                                    onUp:   () => _rotatingRight = false);
    }

    void Update()
    {
        int direction = 0;
        if (_rotatingLeft)  direction -= 1;
        if (_rotatingRight) direction += 1;

        if (direction == 0 && !smooth) return;

        _targetY += direction * rotateSpeed * Time.deltaTime;

        Vector3 euler = targetObject.transform.eulerAngles;

        if (smooth)
            euler.y = Mathf.LerpAngle(euler.y, _targetY, Time.deltaTime * smoothSpeed);
        else
            euler.y = _targetY;

        targetObject.transform.eulerAngles = euler;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    void RegisterButton(Button btn, System.Action onDown, System.Action onUp)
    {
        if (btn == null)
        {
            Debug.LogWarning($"[RotateButtons] A button is not assigned on {gameObject.name}.");
            return;
        }

        var trigger = btn.gameObject.GetComponent<EventTrigger>()
                   ?? btn.gameObject.AddComponent<EventTrigger>();

        AddTriggerEntry(trigger, EventTriggerType.PointerDown, onDown);
        AddTriggerEntry(trigger, EventTriggerType.PointerUp,   onUp);
        AddTriggerEntry(trigger, EventTriggerType.PointerExit, onUp); // release if finger slides off
    }

    static void AddTriggerEntry(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Snap the Y rotation back to zero.</summary>
    public void ResetRotation()
    {
        _targetY = 0f;
        Vector3 euler = targetObject.transform.eulerAngles;
        euler.y = _targetY;
        targetObject.transform.eulerAngles = euler;
    }
}
