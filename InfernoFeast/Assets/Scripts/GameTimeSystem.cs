using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimeSystem : MonoBehaviour
{
    public static GameTimeSystem Instance { get; private set; }

    [Header("Duracion de los dias y semana")]
    public float secondsPerGameDay = 480f;
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    [Header("Configuracion Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;
    public bool useUnscaledTime = false;

    [Header("UI")]
    public GameObject panelFinalDia;
    public TMP_Text horaText;
    public TMP_Text diaText;
    public GameObject Stats;
    public TMP_Text StatDinero;
    public TMP_Text StatReputacion;

    [Header("Manager fin del dia")]
    public GameObject Managerfindia;
    public GameObject Cama;

    [Header("Configuracion nuevo dia")]
    public string restaurantSceneName = "Restaurant";
    public float startHour = 6f;
    public float automaticEndDayHour = 2f;

    private const float SecondsPerRealDay = 24f * 60f * 60f;

    private float gameTimeInSeconds = 0f;
    private float lastGameTimeInSeconds = 0f;
    private int currentDayIndex = 0;
    private bool esperandoNuevoDia = false;
    private bool isTransitioningDay = false;
    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.CopySceneReferencesFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureValidDays();
        InitializeTimeIfNeeded();
    }

    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CacheSceneReferences();
        InitializeSceneUI();
        UpdateClockUI();
    }

    private void Update()
    {
        if (esperandoNuevoDia || isTransitioningDay)
            return;

        float previousTime = gameTimeInSeconds;
        AdvanceGameTime();

        if (HasCrossedHour(previousTime, gameTimeInSeconds, automaticEndDayHour))
        {
            ComenzarNuevoDia();
            return;
        }

        lastGameTimeInSeconds = gameTimeInSeconds;
        UpdateClockUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this)
            return;

        CacheSceneReferences();

        if (!esperandoNuevoDia && !isTransitioningDay)
            InitializeSceneUI();

        UpdateClockUI();
    }

    private void CopySceneReferencesFrom(GameTimeSystem other)
    {
        if (other == null)
            return;

        if (other.secondsPerGameDay > 0f)
            secondsPerGameDay = other.secondsPerGameDay;

        if (other.days != null && other.days.Length > 0)
            days = other.days;

        if (other.fadeCanvasGroup != null)
            fadeCanvasGroup = other.fadeCanvasGroup;

        if (other.fadeDuration > 0f)
            fadeDuration = other.fadeDuration;

        if (other.panelFinalDia != null)
            panelFinalDia = other.panelFinalDia;

        if (other.horaText != null)
            horaText = other.horaText;

        if (other.diaText != null)
            diaText = other.diaText;

        if (other.Stats != null)
            Stats = other.Stats;

        if (other.StatDinero != null)
            StatDinero = other.StatDinero;

        if (other.StatReputacion != null)
            StatReputacion = other.StatReputacion;

        if (other.Managerfindia != null)
            Managerfindia = other.Managerfindia;

        if (other.Cama != null)
            Cama = other.Cama;

        if (!string.IsNullOrEmpty(other.restaurantSceneName))
            restaurantSceneName = other.restaurantSceneName;

        if (other.startHour >= 0f && other.startHour < 24f)
            startHour = other.startHour;

        if (other.automaticEndDayHour >= 0f && other.automaticEndDayHour < 24f)
            automaticEndDayHour = other.automaticEndDayHour;

        CacheSceneReferences();
        UpdateClockUI();
    }

    private void InitializeTimeIfNeeded()
    {
        if (initialized)
            return;

        gameTimeInSeconds = HourToSeconds(startHour);
        lastGameTimeInSeconds = gameTimeInSeconds;
        initialized = true;
    }

    private void AdvanceGameTime()
    {
        float safeSecondsPerGameDay = Mathf.Max(1f, secondsPerGameDay);
        gameTimeInSeconds += Time.deltaTime * (SecondsPerRealDay / safeSecondsPerGameDay);

        while (gameTimeInSeconds >= SecondsPerRealDay)
            gameTimeInSeconds -= SecondsPerRealDay;
    }

    private bool HasCrossedHour(float previousTime, float currentTime, float hour)
    {
        float targetTime = HourToSeconds(hour);

        if (previousTime <= currentTime)
            return previousTime < targetTime && currentTime >= targetTime;

        return targetTime > previousTime || targetTime <= currentTime;
    }

    private float HourToSeconds(float hour)
    {
        hour = Mathf.Repeat(hour, 24f);
        return hour * 3600f;
    }

    public string GetFormattedTime()
    {
        int totalSeconds = Mathf.FloorToInt(gameTimeInSeconds);
        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        return horas.ToString("00") + ":" + minutos.ToString("00");
    }

    public string GetCurrentGameDay()
    {
        EnsureValidDays();
        currentDayIndex = Mathf.Clamp(currentDayIndex, 0, days.Length - 1);
        return days[currentDayIndex];
    }

    public void ActivarFinDeDia()
    {
        if (isTransitioningDay)
            return;

        esperandoNuevoDia = true;
        SetPanel(Stats, false);
        SetPanel(panelFinalDia, true);
        UpdateClockUI();
    }

    public void Dormir()
    {
        ActivarFinDeDia();
    }

    public void StartNewDayAutomatic()
    {
        if (isTransitioningDay)
            return;

        StartCoroutine(SequenceStartNewDay());
    }

    private IEnumerator SequenceStartNewDay()
    {
        isTransitioningDay = true;
        esperandoNuevoDia = true;

        SetPanel(panelFinalDia, false);
        SetPanel(Stats, false);

        yield return StartCoroutine(DoFade(1f));

        AdvanceToNextDay();
        ResetBedState();

        if (!string.IsNullOrEmpty(restaurantSceneName) && SceneManager.GetActiveScene().name != restaurantSceneName)
        {
            SceneManager.LoadScene(restaurantSceneName);
            yield return null;
        }

        CacheSceneReferences();
        InitializeSceneUI();
        UpdateClockUI();

        yield return StartCoroutine(DoFade(0f));

        esperandoNuevoDia = false;
        isTransitioningDay = false;
    }

    public void ComenzarNuevoDia()
    {
        if (esperandoNuevoDia || isTransitioningDay)
            return;

        StartCoroutine(SequenceComenzarNuevoDia());
    }

    private IEnumerator SequenceComenzarNuevoDia()
    {
        isTransitioningDay = true;
        esperandoNuevoDia = true;

        yield return StartCoroutine(DoFade(1f));

        UpdateStatsTexts();
        SetPanel(panelFinalDia, false);
        SetPanel(Stats, true);

        isTransitioningDay = false;
    }

    private IEnumerator DoFade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;
        float safeDuration = Mathf.Max(0.01f, fadeDuration);

        fadeCanvasGroup.blocksRaycasts = true;

        while (timer < safeDuration)
        {
            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / safeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0f;
    }

    public void CerrarPanel()
    {
        SetPanel(panelFinalDia, false);

        if (!isTransitioningDay)
            esperandoNuevoDia = false;
    }

    private void AdvanceToNextDay()
    {
        EnsureValidDays();
        currentDayIndex = (currentDayIndex + 1) % days.Length;
        gameTimeInSeconds = HourToSeconds(startHour);
        lastGameTimeInSeconds = gameTimeInSeconds;
    }

    private void ResetBedState()
    {
        Dormir[] dormirScripts = FindObjectsOfType<Dormir>();
        for (int i = 0; i < dormirScripts.Length; i++)
        {
            if (dormirScripts[i] != null)
                dormirScripts[i].nuevoDia = false;
        }

        if (Cama != null)
        {
            Dormir dormir = Cama.GetComponent<Dormir>();
            if (dormir != null)
                dormir.nuevoDia = false;
        }
    }

    private void CacheSceneReferences()
    {
        if (ManagerFinDia.Instance != null)
            Managerfindia = ManagerFinDia.Instance.gameObject;
        else if (Managerfindia == null)
        {
            ManagerFinDia manager = FindObjectOfType<ManagerFinDia>();
            if (manager != null)
                Managerfindia = manager.gameObject;
        }

        if (Cama == null)
        {
            Dormir dormir = FindObjectOfType<Dormir>();
            if (dormir != null)
                Cama = dormir.gameObject;
        }

        if (fadeCanvasGroup == null)
            fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }

    private void InitializeSceneUI()
    {
        SetPanel(panelFinalDia, false);
        SetPanel(Stats, false);

        if (fadeCanvasGroup != null && !isTransitioningDay)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void UpdateClockUI()
    {
        if (horaText != null)
            horaText.text = GetFormattedTime();

        if (diaText != null)
            diaText.text = GetCurrentGameDay();
    }

    private void UpdateStatsTexts()
    {
        ManagerFinDia manager = GetManagerFinDia();

        int dinero = manager != null ? manager.dinero : 0;
        int reputacion = manager != null ? manager.reputacion : 0;

        if (StatDinero != null)
            StatDinero.text = dinero.ToString("F0");

        if (StatReputacion != null)
            StatReputacion.text = reputacion.ToString("F0");
    }

    private ManagerFinDia GetManagerFinDia()
    {
        if (ManagerFinDia.Instance != null)
            return ManagerFinDia.Instance;

        if (Managerfindia != null)
            return Managerfindia.GetComponent<ManagerFinDia>();

        ManagerFinDia manager = FindObjectOfType<ManagerFinDia>();
        if (manager != null)
            Managerfindia = manager.gameObject;

        return manager;
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private void EnsureValidDays()
    {
        if (days == null || days.Length == 0)
            days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}