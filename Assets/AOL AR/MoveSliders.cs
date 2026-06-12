using UnityEngine;
using UnityEngine.UI;

public class MoveSliders : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Sliders")]
    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    [Header("Ranges")]
    public float xRange = 0.5f;
    public float yRange = 0.5f;
    public float zRange = 0.5f;

    [Header("Smoothing")]
    public bool smooth = true;
    public float smoothSpeed = 10f;

    // Use LocalPosition to avoid fighting with the AR Session/Camera
    private Vector3 _startLocalPos;
    private Vector3 _targetLocalPos;
    private float _xOff, _yOff, _zOff;
    private bool _hasInitialized = false;

    void Awake()
    {
        if (targetObject == null) targetObject = gameObject;

        // Initialize sliders to middle
        SetupSlider(xSlider, OnXChanged);
        SetupSlider(ySlider, OnYChanged);
        SetupSlider(zSlider, OnZChanged);
    }

    void OnEnable()
    {
        // Don't bake here! AR might not have placed the object yet.
        _hasInitialized = false; 
    }

    // --- Core Logic ---

    void InitializeIfNeeded()
    {
        if (_hasInitialized) return;

        // Capture the position AFTER it has been placed in AR
        _startLocalPos = targetObject.transform.localPosition;
        _targetLocalPos = _startLocalPos;
        _hasInitialized = true;

        // Force sliders to center visually without triggering movement yet
        if(xSlider) xSlider.SetValueWithoutNotify(0.5f);
        if(ySlider) ySlider.SetValueWithoutNotify(0.5f);
        if(zSlider) zSlider.SetValueWithoutNotify(0.5f);
    }

    void Update()
    {
        if (!_hasInitialized) return;

        if (smooth)
        {
            targetObject.transform.localPosition = Vector3.Lerp(
                targetObject.transform.localPosition,
                _targetLocalPos,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    void Apply()
    {
        InitializeIfNeeded();

        _targetLocalPos = _startLocalPos + new Vector3(_xOff, _yOff, _zOff);

        if (!smooth)
            targetObject.transform.localPosition = _targetLocalPos;
    }

    // --- Callbacks ---

    public void OnXChanged(float v) { _xOff = (v - 0.5f) * 2f * xRange; Apply(); }
    public void OnYChanged(float v) { _yOff = (v - 0.5f) * 2f * yRange; Apply(); }
    public void OnZChanged(float v) { _zOff = (v - 0.5f) * 2f * zRange; Apply(); }

    void SetupSlider(Slider s, UnityEngine.Events.UnityAction<float> call)
    {
        if (!s) return;
        s.minValue = 0;
        s.maxValue = 1;
        s.value = 0.5f;
        s.onValueChanged.AddListener(call);
    }

    // Call this from your AR Placement script specifically 
    // once the user taps the screen to place the object.
    public void FinalizePlacement()
    {
        _hasInitialized = false;
        InitializeIfNeeded();
    }
}