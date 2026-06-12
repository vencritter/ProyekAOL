using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a GameObject's Z position via a UI Slider centred at 0.5.
/// Slider 0 = origin - rangeBack | Slider 0.5 = origin Z | Slider 1 = origin + rangeFront.
/// Set your Slider's Min=0, Max=1, Value=0.5 in the Inspector.
/// </summary>
public class ZPositionSlider : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Object to move along Z. Defaults to this GameObject.")]
    public GameObject targetObject;

    [Header("Slider")]
    [Tooltip("UI Slider with Min=0, Max=1, Value=0.5.")]
    public Slider slider;

    [Header("Movement Range")]
    [Tooltip("How far back (−Z) the object moves from its origin (slider at 0).")]
    public float rangeBack  = 5f;
    [Tooltip("How far forward (+Z) the object moves from its origin (slider at 1).")]
    public float rangeFront = 5f;

    [Header("Smoothing")]
    [Tooltip("Lerp the object to the target Z instead of snapping.")]
    public bool  smooth      = false;
    [Range(1f, 30f)]
    public float smoothSpeed = 10f;

    // ── Private ────────────────────────────────────────────────────────────────
    private Vector3 _originPosition;
    private float   _targetZ;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (targetObject == null)
            targetObject = gameObject;

        _originPosition = targetObject.transform.position;
        _targetZ        = _originPosition.z;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;

            slider.onValueChanged.AddListener(OnSliderChanged);
            OnSliderChanged(slider.value);   // apply current slider position immediately
        }
        else
        {
            Debug.LogWarning($"[ZPositionSlider] No Slider assigned on {gameObject.name}.");
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
        pos.z = Mathf.Lerp(pos.z, _targetZ, Time.deltaTime * smoothSpeed);
        targetObject.transform.position = pos;
    }

    // ── Callback ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Called automatically when the slider moves.
    /// 0 → origin − rangeBack | 0.5 → origin Z | 1 → origin + rangeFront
    /// </summary>
    public void OnSliderChanged(float sliderValue)
    {
        float offset;
        if (sliderValue <= 0.5f)
            offset = Mathf.Lerp(-rangeBack,  0f,         sliderValue / 0.5f);
        else
            offset = Mathf.Lerp( 0f,         rangeFront, (sliderValue - 0.5f) / 0.5f);

        _targetZ = _originPosition.z + offset;

        if (!smooth)
        {
            Vector3 pos = targetObject.transform.position;
            pos.z = _targetZ;
            targetObject.transform.position = pos;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Snap the slider and object back to centre (origin Z).</summary>
    public void ResetToCenter()
    {
        if (slider != null) slider.value = 0.5f;
    }

    /// <summary>Re-anchors the origin to the object's current position.</summary>
    public void BakeCurrentPositionAsOrigin()
    {
        _originPosition = targetObject.transform.position;
    }
}
