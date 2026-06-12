using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a GameObject's opacity via a UI Slider centred at 0.5.
/// Slider 0 = fully transparent | Slider 0.5 = mid opacity | Slider 1 = fully opaque.
/// Set your Slider's Min=0, Max=1, Value=0.5 in the Inspector.
/// </summary>
public class XOpacitySlider : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Object to fade. Defaults to this GameObject.")]
    public GameObject targetObject;

    [Header("Slider")]
    [Tooltip("UI Slider with Min=0, Max=1, Value=0.5.")]
    public Slider slider;

    [Header("Opacity Range")]
    [Tooltip("Alpha when slider is at 0 (left).")]
    [Range(0f, 1f)]
    public float minOpacity = 0f;

    [Tooltip("Alpha when slider is at 0.5 (centre). Object starts here.")]
    [Range(0f, 1f)]
    public float midOpacity = 0.5f;

    [Tooltip("Alpha when slider is at 1 (right).")]
    [Range(0f, 1f)]
    public float maxOpacity = 1f;

    [Header("Smoothing")]
    public bool smooth = false;
    [Range(1f, 30f)]
    public float smoothSpeed = 10f;

    // ── Private ────────────────────────────────────────────────────────────────
    private Material[] _materials;
    private float      _targetAlpha;
    private float      _currentAlpha;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────
    void Awake()
    {
        if (targetObject == null)
            targetObject = gameObject;

        CacheRenderers();

        _targetAlpha  = midOpacity;
        _currentAlpha = midOpacity;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.onValueChanged.AddListener(OnSliderChanged);
            OnSliderChanged(slider.value);   // apply immediately
        }
        else
        {
            Debug.LogWarning($"[XOpacitySlider] No Slider assigned on {gameObject.name}.");
            ApplyAlpha(midOpacity);
        }
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);

        if (_materials != null)
            foreach (var mat in _materials)
                if (mat != null) Destroy(mat);
    }

    void Update()
    {
        if (!smooth) return;

        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * smoothSpeed);
        ApplyAlpha(_currentAlpha);
    }

    // ── Callback ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Called automatically when the slider moves.
    /// 0→minOpacity | 0.5→midOpacity | 1→maxOpacity
    /// </summary>
    public void OnSliderChanged(float sliderValue)
    {
        float alpha;
        if (sliderValue <= 0.5f)
            alpha = Mathf.Lerp(minOpacity, midOpacity, sliderValue / 0.5f);
        else
            alpha = Mathf.Lerp(midOpacity, maxOpacity, (sliderValue - 0.5f) / 0.5f);

        _targetAlpha = alpha;

        if (!smooth)
        {
            _currentAlpha = alpha;
            ApplyAlpha(alpha);
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Snap slider and opacity back to the centre value.</summary>
    public void ResetToCenter()
    {
        if (slider != null) slider.value = 0.5f;
    }

    /// <summary>Set opacity directly from code (0 = invisible, 1 = fully opaque).</summary>
    public void SetOpacity(float alpha)
    {
        _targetAlpha = Mathf.Clamp01(alpha);
        if (!smooth)
        {
            _currentAlpha = _targetAlpha;
            ApplyAlpha(_currentAlpha);
        }
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    void CacheRenderers()
    {
        var renderers = targetObject.GetComponentsInChildren<Renderer>(includeInactive: true);
        var matList   = new System.Collections.Generic.List<Material>();

        foreach (var r in renderers)
        {
            var instanced = r.materials;   // returns instanced copies
            r.materials = instanced;
            matList.AddRange(instanced);
        }

        _materials = matList.ToArray();
    }

    void ApplyAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        foreach (var mat in _materials)
        {
            if (mat == null) continue;
            SetMaterialTransparent(mat, alpha);
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
    }

    static void SetMaterialTransparent(Material mat, float alpha)
    {
        // ── URP Lit ────────────────────────────────────────────────
        if (mat.HasProperty("_Surface"))
        {
            if (alpha < 1f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend",   0f);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite",  0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetFloat("_ZWrite",  1f);
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
            return;
        }

        // ── Built-in RP Standard ───────────────────────────────────
        if (mat.HasProperty("_Mode"))
        {
            if (alpha < 1f)
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite",  0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                mat.SetFloat("_Mode", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite",  1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        }
    }
}
