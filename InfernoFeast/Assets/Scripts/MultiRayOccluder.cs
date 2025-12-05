using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MultiRayOccluder : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Raycast settings")]
    [Tooltip("Número de rayos en la rejilla (ej. 1 = centro, 9 = 3x3)")]
    [Range(1, 25)]
    public int raysPerAxis = 3;
    [Tooltip("Radio virtual en metros para esparcir los rayos alrededor del centro")]
    public float sampleRadius = 0.3f;
    [Tooltip("Distancia máxima de comprobación (0 = usar distancia cámara->target)")]
    public float maxDistance = 0f;
    public LayerMask occluderMask = ~0; // por defecto todo
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Fade/Material")]
    [Range(0f, 1f)] public float targetAlpha = 0.2f;
    public float fadeSpeed = 8f;
    public bool fallbackDisableRenderer = true;

    [Header("Debug")]
    public bool debugDraw = false;
    public Color debugHitColor = Color.red;
    public Color debugMissColor = Color.green;

    // Internals
    class MatInfo { public Material original; public Material clone; public float currentAlpha; }
    Dictionary<Renderer, MatInfo[]> tracked = new Dictionary<Renderer, MatInfo[]>();

    void Update()
    {
        if (target == null) return;

        Vector3 camPos = transform.position;
        Vector3 targetPos = target.position;
        Vector3 dir = targetPos - camPos;
        float dist = dir.magnitude;
        if (dist < 0.001f) return;
        dir /= dist;

        float checkDist = (maxDistance > 0f) ? Mathf.Min(maxDistance, dist) : dist;

        // build a set of renderers hit by any ray
        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        // camera basis for offsets
        Vector3 right = transform.right;
        Vector3 up = transform.up;

        int axis = Mathf.Max(1, raysPerAxis);
        int half = axis / 2;

        for (int x = 0; x < axis; x++)
        {
            for (int y = 0; y < axis; y++)
            {
                // compute normalized grid coordinates in [-1,1]
                float nx = (axis == 1) ? 0f : ((x - half) / (float)half);
                float ny = (axis == 1) ? 0f : ((y - half) / (float)half);
                // if axis even, scale down to keep sampling centered
                if (axis % 2 == 0) { nx *= (half / (float)(half + 0.5f)); ny *= (half / (float)(half + 0.5f)); }

                Vector3 origin = camPos + right * nx * sampleRadius + up * ny * sampleRadius;

                RaycastHit hit;
                bool isHit = Physics.Raycast(origin, dir, out hit, checkDist, occluderMask, triggerInteraction);

                if (debugDraw)
                {
                    Debug.DrawLine(origin, origin + dir * checkDist, isHit ? debugHitColor : debugMissColor, 0.1f);
                }

                if (isHit)
                {
                    // get renderer from hit collider (handle child colliders)
                    Renderer r = hit.collider.GetComponent<Renderer>();
                    if (r == null)
                        r = hit.collider.GetComponentInParent<Renderer>();

                    if (r != null && !IsPartOfTarget(r))
                    {
                        hitRenderers.Add(r);
                    }
                }
            }
        }

        // Add new hit renderers to tracking
        foreach (var r in hitRenderers)
        {
            if (!tracked.ContainsKey(r))
                SetupRenderer(r);
        }

        // Update tracked renderers (fade toward targetAlpha or restore)
        List<Renderer> toRemove = new List<Renderer>();
        foreach (var kv in tracked)
        {
            var r = kv.Key;
            bool isHitNow = hitRenderers.Contains(r);
            var infos = kv.Value;

            bool allRestored = true;
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                float target = isHitNow ? targetAlpha : (info.original != null ? info.original.color.a : 1f);
                info.currentAlpha = Mathf.MoveTowards(info.currentAlpha, target, fadeSpeed * Time.deltaTime);

                if (info.clone != null)
                {
                    Color c = info.clone.color;
                    c.a = info.currentAlpha;
                    info.clone.color = c;
                }
                else
                {
                    // fallback: enable/disable renderer
                    if (fallbackDisableRenderer)
                        r.enabled = isHitNow;
                }

                if (Mathf.Abs(info.currentAlpha - target) > 0.001f) allRestored = false;
            }

            if (!isHitNow && allRestored)
            {
                RestoreRenderer(r);
                toRemove.Add(r);
            }
        }

        foreach (var r in toRemove) tracked.Remove(r);
    }

    bool IsPartOfTarget(Renderer r)
    {
        if (target == null) return false;
        return r.transform.IsChildOf(target);
    }

    void SetupRenderer(Renderer r)
    {
        Material[] shared = r.sharedMaterials;
        MatInfo[] infos = new MatInfo[shared.Length];

        bool createdAnyClone = false;
        for (int i = 0; i < shared.Length; i++)
        {
            var orig = shared[i];
            var info = new MatInfo();
            info.original = orig;
            info.currentAlpha = (orig != null && orig.HasProperty("_Color")) ? orig.color.a : 1f;
            info.clone = null;

            if (orig != null)
            {
                Material clone = new Material(orig);
                if (TryMakeTransparent(clone))
                {
                    Color cc = clone.color;
                    cc.a = info.currentAlpha;
                    clone.color = cc;
                    info.clone = clone;
                    createdAnyClone = true;
                }
                else
                {
                    // if shader doesn't support transparency, leave clone null and fallback to disable renderer
                    info.clone = null;
                }
            }
            infos[i] = info;
        }

        if (createdAnyClone)
        {
            Material[] mats = new Material[infos.Length];
            for (int i = 0; i < infos.Length; i++) mats[i] = infos[i].clone ?? shared[i];
            r.materials = mats;
        }
        else if (fallbackDisableRenderer)
        {
            r.enabled = false;
        }

        tracked.Add(r, infos);
    }

    void RestoreRenderer(Renderer r)
    {
        if (!tracked.ContainsKey(r)) return;
        var infos = tracked[r];
        bool hadClones = false;
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].clone != null)
            {
                hadClones = true;
                Destroy(infos[i].clone);
            }
        }

        if (hadClones)
        {
            Material[] origs = new Material[infos.Length];
            for (int i = 0; i < infos.Length; i++) origs[i] = infos[i].original;
            r.materials = origs;
        }
        r.enabled = true;
    }

    bool TryMakeTransparent(Material m)
    {
        if (m == null) return false;
        // Standard shader (has _Mode)
        if (m.HasProperty("_Mode"))
        {
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return true;
        }
        // URP/HDRP shaders: try color alpha if available (may not work)
        if (m.HasProperty("_BaseColor")) // URP Lit uses _BaseColor
        {
            Color c = m.GetColor("_BaseColor");
            c.a = 1f;
            m.SetColor("_BaseColor", c);
            return true;
        }
        if (m.HasProperty("_Color"))
        {
            Color c = m.color;
            c.a = 1f;
            m.color = c;
            return true;
        }
        return false;
    }

    void OnDisable()
    {
        // restore everything
        var keys = new List<Renderer>(tracked.Keys);
        foreach (var r in keys) RestoreRenderer(r);
        tracked.Clear();
    }
}
