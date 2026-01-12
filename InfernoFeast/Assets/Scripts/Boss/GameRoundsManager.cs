using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRoundsManager : MonoBehaviour
{
    public static GameRoundsManager Instance { get; private set; }

    [Header("Rondas")]
    public List<RoundConfig> rounds = new List<RoundConfig>();

    [Header("Strikes")]
    public int maxStrikes = 3;
    public int currentStrikes = 0;

    [Header("Audio de fallos")]
    public AudioClip strikeSound;      // Arrastrar el audio en el inspector
    

    [Header("UI de fallos")]
    public List<Image> strikeIcons;

    [Header("Referencias")]
    public Timer timer;
    public DialogueBoss dialogueBoss;

    [Header("Escenas")]
    public string successSceneName = "VictoryScene";
    public string failSceneName = "FailScene";

    [Header("Mensajes")]
    public List<string> winLines = new List<string>() { "¡Enhorabuena! Has superado el reto." };
    public List<string> loseLines = new List<string>() { "No has conseguido superar el reto." };

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;

    List<ClientTableGroup> allGroups = new List<ClientTableGroup>();
    List<ClientTableGroup> currentRoundGroups = new List<ClientTableGroup>();

    int currentRoundIndex = 0;
    string pendingSceneToLoad = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 🔎 Encuentra TODAS las mesas de la escena
        allGroups.AddRange(FindObjectsOfType<ClientTableGroup>());

        // 🔥 Apagar TODAS las mesas al iniciar
        DeactivateAllGroups();

        if (timer != null)
            timer.OnTimerEnd += OnRoundTimeUp;

        if (dialogueBoss != null)
            dialogueBoss.OnDialogClosed += OnDialogClosed;

        if (rounds.Count == 0)
        {
            Debug.LogWarning("[GameRoundsManager] No hay rondas configuradas.");
            return;
        }

        // ▶️ Iniciar primera ronda
        StartRound(0);
    }

    void OnDestroy()
    {
        if (timer != null) timer.OnTimerEnd -= OnRoundTimeUp;
        if (dialogueBoss != null) dialogueBoss.OnDialogClosed -= OnDialogClosed;
    }

    void DeactivateAllGroups()
    {
        foreach (var group in allGroups)
        {
            if (group != null)
                group.Deactivate();
        }
    }

    void StartRound(int roundIndex)
    {
        currentRoundIndex = roundIndex;
        currentRoundGroups.Clear();

        foreach (var group in allGroups)
        {
            if (group == null) continue;

            if (group.IsActiveInRound(roundIndex))
            {
                group.ActivateForRound();
                currentRoundGroups.Add(group);
            }
            else
            {
                group.Deactivate();
            }
        }

        if (timer != null)
            timer.duration = rounds[roundIndex].duration;

        if (dialogueBoss != null)
        {
            dialogueBoss.timer = timer;
            dialogueBoss.StartDialog(
                new List<string> { $"Ronda {roundIndex + 1}: ¡A cocinar!" }
            );
        }
        else
        {
            timer?.StartTimer();
        }

        Debug.Log($"[GameRoundsManager] Ronda {roundIndex + 1} iniciada con {currentRoundGroups.Count} mesas.");
    }
    void ShowStrikesUI()
    {
        bool playedSound = false; // Control para reproducir el audio solo una vez

        for (int i = 0; i < strikeIcons.Count; i++)
        {
            if (i < currentStrikes)
            {
                if (!strikeIcons[i].gameObject.activeSelf)
                {
                    StartCoroutine(PopIcon(strikeIcons[i]));

                    // Reproducir el sonido solo la primera vez que se activa un icono
                    if (!playedSound)
                    {
                        if (audioSource != null && strikeSound != null)
                            audioSource.PlayOneShot(strikeSound);

                        playedSound = true;
                    }
                }
            }
            else
            {
                strikeIcons[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnPlateDelivered(TableAnchor anchor, Plate plate)
    {
        if (anchor == null || anchor.group == null || plate == null) return;

        ClientTableGroup group = anchor.group;

        if (!currentRoundGroups.Contains(group)) return;
        if (group.served) return;

        bool correct = plate.dish == group.requiredDish;

        if (correct)
        {
            audioSource.PlayOneShot(successSound);

            group.OnServed();
            Destroy(plate.gameObject, 0.05f);
            currentRoundGroups.Remove(group);

            // Si ya no quedan mesas activas, pasamos a la siguiente ronda
            if (currentRoundGroups.Count == 0)
                NextRound();
        }
        else
        {
            // Plato incorrecto → contamos fallo
            group.OnMissed();

            currentStrikes++;
            if (currentStrikes > maxStrikes) currentStrikes = maxStrikes;

            ShowStrikesUI();

            if (currentStrikes >= maxStrikes)
                LoseGame();
        }
    }

    void OnRoundTimeUp()
    {
        // Todos los platos no servidos cuentan como fallos
        currentStrikes += currentRoundGroups.Count;
        if (currentStrikes > maxStrikes) currentStrikes = maxStrikes;

        ShowStrikesUI();

        foreach (var g in currentRoundGroups)
            g.OnMissed();

        currentRoundGroups.Clear();

        if (currentStrikes >= maxStrikes)
            LoseGame();
        else
            NextRound();
    }

    void NextRound()
    {
        int next = currentRoundIndex + 1;

        if (next >= rounds.Count)
            WinGame();
        else
            StartRound(next);
    }

    void WinGame()
    {
        timer?.StopTimer();

        if (dialogueBoss != null)
        {
            pendingSceneToLoad = successSceneName;
            dialogueBoss.StartDialog(winLines);
        }
        else
        {
            SceneManager.LoadScene(successSceneName);
        }
    }

    void LoseGame()
    {
        timer?.StopTimer();

        if (dialogueBoss != null)
        {
            pendingSceneToLoad = failSceneName;
            dialogueBoss.StartDialog(loseLines);
        }
        else
        {
            SceneManager.LoadScene(failSceneName);
        }
    }

    void OnDialogClosed()
    {
        if (!string.IsNullOrEmpty(pendingSceneToLoad))
            SceneManager.LoadScene(pendingSceneToLoad);
    }

    IEnumerator PopIcon(Image icon)
    {
        icon.gameObject.SetActive(true);

        float duration = 0.25f;        // Duración total del pop
        Vector3 startScale = Vector3.zero;
        Vector3 overshootScale = Vector3.one * 1.2f; // Tamaño máximo temporal
        Vector3 endScale = Vector3.one;              // Tamaño final
        float halfDuration = duration / 2f;
        float time = 0f;

        icon.rectTransform.localScale = startScale;

        // Primera fase: 0 → overshoot
        while (time < halfDuration)
        {
            float t = time / halfDuration;
            float scale = Mathf.SmoothStep(0f, overshootScale.x, t);
            icon.rectTransform.localScale = Vector3.one * scale;
            time += Time.deltaTime;
            yield return null;
        }

        // Aseguramos overshoot exacto
        icon.rectTransform.localScale = overshootScale;

        // Segunda fase: overshoot → tamaño final
        time = 0f;
        while (time < halfDuration)
        {
            float t = time / halfDuration;
            float scale = Mathf.SmoothStep(overshootScale.x, endScale.x, t);
            icon.rectTransform.localScale = Vector3.one * scale;
            time += Time.deltaTime;
            yield return null;
        }

        // Ajustamos al tamaño final exacto
        icon.rectTransform.localScale = endScale;
    }
}
