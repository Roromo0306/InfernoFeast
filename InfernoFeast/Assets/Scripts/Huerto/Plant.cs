using UnityEngine;

public class Plant : MonoBehaviour
{
    public string plantName;
    public int growthDays = 3;
    public int currentDay = 0;

    public bool wateredToday = false;
    public static bool hasGrown = false;

    [Header("Crecimiento visual")]
    public GameObject[] growthStages; // prefabs/modelos: semilla, brote, planta madura
    private int currentStage = 0;

    private Renderer soilRenderer;

    void Awake()
    {
        soilRenderer = transform.parent.GetComponent<Renderer>();
    }

    void OnEnable()
    {
        Calendar.OnDayChanged += AdvanceDay;
    }

    void OnDisable()
    {
        Calendar.OnDayChanged -= AdvanceDay;
    }

    public void Water()
    {
        if (!wateredToday)
        {
            wateredToday = true;
            if (soilRenderer != null) soilRenderer.material.color = Color.green;
        }
    }

    public void AdvanceDay()
    {
        if (wateredToday && currentDay < growthDays)
        {
            currentDay++;
            wateredToday = false;
            UpdateGrowthVisual();

            if (currentDay >= growthDays)
            {
                hasGrown = true;
            }
        }
        else
        {
            wateredToday = false;
            if (soilRenderer != null) soilRenderer.material.color = Color.gray;
        }
    }

    private void UpdateGrowthVisual()
    {
        int stage = Mathf.FloorToInt(((float)currentDay / growthDays) * growthStages.Length);
        stage = Mathf.Clamp(stage, 0, growthStages.Length - 1);

        if (stage != currentStage)
        {
            for (int i = 0; i < growthStages.Length; i++)
                growthStages[i].SetActive(i == stage);

            currentStage = stage;
        }
    }

    public float GetGrowthProgress() => (float)currentDay / growthDays;
}