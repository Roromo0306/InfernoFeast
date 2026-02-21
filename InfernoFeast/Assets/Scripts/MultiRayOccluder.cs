using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        Reset();
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
                float nx = (axis == 1) ? 0f : ((x - half) / (float)half);
                float ny = (axis == 1) ? 0f : ((y - half) / (float)half);
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
                    Color c;
                    if (info.clone.HasProperty("_BaseColor"))
                        c = info.clone.GetColor("_BaseColor");
                    else
                        c = info.clone.color;

                    c.a = info.currentAlpha;

                    if (info.clone.HasProperty("_BaseColor"))
                        info.clone.SetColor("_BaseColor", c);
                    else
                        info.clone.color = c;
                }
                else
                {
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

    // ---------------------------
    // NUEVAS VERSIONES REEMPLAZADAS
    // ---------------------------

    bool TryMakeTransparent(Material m)
    {
        if (m == null) return false;

        // 1) BUILT-IN STANDARD
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

        // 2) URP Lit
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            if (m.HasProperty("_BaseColor"))
            {
                Color c = m.GetColor("_BaseColor");
                c.a = c.a;
                m.SetColor("_BaseColor", c);
            }
            else if (m.HasProperty("_Color"))
            {
                Color c = m.color;
                c.a = c.a;
                m.color = c;
            }
            return true;
        }

        // 3) HDRP
        if (m.HasProperty("_SurfaceType"))
        {
            m.SetFloat("_SurfaceType", 1f);
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (m.HasProperty("_BaseColor"))
            {
                Color c = m.GetColor("_BaseColor");
                m.SetColor("_BaseColor", c);
            }
            return true;
        }

        // 4) Generic alpha attempts
        if (m.HasProperty("_BaseColor"))
        {
            Color c = m.GetColor("_BaseColor");
            c.a = c.a;
            m.SetColor("_BaseColor", c);
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return true;
        }

        if (m.HasProperty("_Color"))
        {
            Color c = m.color;
            c.a = c.a;
            m.color = c;
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return true;
        }

        try
        {
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return true;
        }
        catch { }

        return false;
    }
    private void Reset()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
        }
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
                    if (clone.HasProperty("_BaseColor"))
                    {
                        Color cc = clone.GetColor("_BaseColor");
                        cc.a = info.currentAlpha;
                        clone.SetColor("_BaseColor", cc);
                    }
                    else if (clone.HasProperty("_Color"))
                    {
                        Color cc = clone.color;
                        cc.a = info.currentAlpha;
                        clone.color = cc;
                    }
                    info.clone = clone;
                    createdAnyClone = true;
                }
                else
                {
                    Destroy(clone);
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
        else
        {
            if (fallbackDisableRenderer)
                Debug.LogWarning($"MultiRayOccluder: no transparent clone could be created for Renderer '{r.name}'. Renderer left enabled (disable fallback if undesired).");
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

    void OnDisable()
    {
        var keys = new List<Renderer>(tracked.Keys);
        foreach (var r in keys) RestoreRenderer(r);
        tracked.Clear();
    }
}
