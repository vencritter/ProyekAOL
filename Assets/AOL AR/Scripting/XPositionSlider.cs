using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a GameObject's X position via a UI Slider.
/// Set your Slider's min=0, max=1, value=0.5 in the Inspector —
/// the middle position will map to the object's original X (zero offset).
/// </summary>
public class XPositionSlider : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Object to move along X. Defaults to this GameObject.")]
    public GameObject targetObject;

    [Header("Slider")]
    [Tooltip("The UI Slider. Should have Min=0, Max=1, Value=0.5 in the Inspector.")]
    public Slider slider;

    [Header("Movement Range")]
    [Tooltip("How far left  the object moves from its origin (slider at 0).")]
    public float rangeLeft  = 5f;
    [Tooltip("How far right the object moves from its origin (slider at 1).")]
    public float rangeRight = 5f;

    [Header("Smoothing")]
    [Tooltip("Lerp the object to the target X instead of snapping.")]
    public bool  smooth     = false;
    [Range(1f, 30f)]
    public float smoothSpeed = 10f;

    // ── Private ────────────────────────────────────────────────────────────────
    private Vector3 _originPosition;   // world position when the script starts
    private float   _targetX;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (targetObject == null)
            targetObject = gameObject;

        _originPosition = targetObject.transform.position;
        _targetX        = _originPosition.x;

        if (slider != null)
        {
            // Enforce 0-1 range; leave value as-is so mid-slider = origin
            slider.minValue = 0f;
            slider.maxValue = 1f;

            slider.onValueChanged.AddListener(OnSliderChanged);

            // Apply the slider's current value immediately
            OnSliderChanged(slider.value);
        }
        else
        {
            Debug.LogWarning($"[XPositionSlider] No Slider assigned on {gameObject.name}.");
        }
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void Update()
    {
        if (!smooth) return;

        Vector3 pos = targetObject.transform.position;
        pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * smoothSpeed);
        targetObject.transform.position = pos;
    }

    // ── Callback ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Called automatically when the slider moves.
    /// Slider value 0.5 → origin X. 0 → origin - rangeLeft. 1 → origin + rangeRight.
    /// </summary>
    public void OnSliderChanged(float sliderValue)
    {
        // Remap 0..0.5..1  →  -rangeLeft..0..+rangeRight
        float offset;
        if (sliderValue <= 0.5f)
            offset = Mathf.Lerp(-rangeLeft, 0f, sliderValue / 0.5f);
        else
            offset = Mathf.Lerp(0f, rangeRight, (sliderValue - 0.5f) / 0.5f);

        _targetX = _originPosition.x + offset;

        if (!smooth)
        {
            Vector3 pos = targetObject.transform.position;
            pos.x = _targetX;
            targetObject.transform.position = pos;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Snap the slider and object back to centre (origin X).</summary>
    public void ResetToCenter()
    {
        if (slider != null) slider.value = 0.5f;
        // OnSliderChanged fires automatically from the above line
    }

    /// <summary>Move the object's origin reference to its current position.</summary>
    public void BakeCurrentPositionAsOrigin()
    {
        _originPosition = targetObject.transform.position;
    }
}
