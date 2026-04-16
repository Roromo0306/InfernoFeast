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
    public AudioClip strikeSound;

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

    [Header("Sprite receta")]
    public Sprite RecetaBoss;

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
        allGroups.AddRange(FindObjectsOfType<ClientTableGroup>());
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

        // ⏸️ Aseguramos que el tiempo NO corre durante el mensaje
        timer?.StopTimer();

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

    public void OnPlateDelivered(TableAnchor anchor, Plate plate)
    {
        if (anchor == null || anchor.group == null || plate == null) return;

        ClientTableGroup group = anchor.group;

        if (!currentRoundGroups.Contains(group)) return;
        if (group.served) return;

        bool correct = plate.dish == group.requiredDish;
        Debug.Log($"Plato entregado: {plate.dish} | Plato requerido: {group.requiredDish} | Mesa: {group.name}");

        if (correct)
        {
            audioSource.PlayOneShot(successSound);

            group.OnServed();
            Destroy(plate.gameObject, 0.05f);
            currentRoundGroups.Remove(group);

            if (currentRoundGroups.Count == 0)
                NextRound();
        }
        else
        {
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
        // ⏸️ Parar el tiempo de la ronda anterior
        timer?.StopTimer();

        int next = currentRoundIndex + 1;

        if (next >= rounds.Count)
            WinGame();
        else
            StartRound(next);
    }

    void WinGame()
    {
        ComandasManager.Instance?.AddComanda(RecetaBoss);
        Debug.Log("Hola");

        timer?.StopTimer();

        if (dialogueBoss != null)
        {
            pendingSceneToLoad = successSceneName;
            dialogueBoss.StartDialog(winLines);
        }
        else
        {
            SceneFadeManager.Instance.LoadSceneWithFade(successSceneName);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            WinGame();
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
            SceneFadeManager.Instance.LoadSceneWithFade(failSceneName);
        }
    }

    void OnDialogClosed()
    {
        // Si hay escena pendiente (victoria o derrota)
        if (!string.IsNullOrEmpty(pendingSceneToLoad))
        {
            SceneFadeManager.Instance.LoadSceneWithFade(pendingSceneToLoad);
            pendingSceneToLoad = ""; // Limpiar para evitar reentradas
            return;
        }

        // ▶️ Arranca el timer SOLO cuando se cierra el mensaje de ronda
        timer?.StartTimer();
    }

    void ShowStrikesUI()
    {
        bool playedSound = false;

        for (int i = 0; i < strikeIcons.Count; i++)
        {
            if (i < currentStrikes)
            {
                if (!strikeIcons[i].gameObject.activeSelf)
                {
                    StartCoroutine(PopIcon(strikeIcons[i]));

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

    IEnumerator PopIcon(Image icon)
    {
        icon.gameObject.SetActive(true);

        float duration = 0.25f;
        Vector3 startScale = Vector3.zero;
        Vector3 overshootScale = Vector3.one * 1.2f;
        Vector3 endScale = Vector3.one;
        float halfDuration = duration / 2f;
        float time = 0f;

        icon.rectTransform.localScale = startScale;

        while (time < halfDuration)
        {
            float t = time / halfDuration;
            float scale = Mathf.SmoothStep(0f, overshootScale.x, t);
            icon.rectTransform.localScale = Vector3.one * scale;
            time += Time.deltaTime;
            yield return null;
        }

        icon.rectTransform.localScale = overshootScale;

        time = 0f;
        while (time < halfDuration)
        {
            float t = time / halfDuration;
            float scale = Mathf.SmoothStep(overshootScale.x, endScale.x, t);
            icon.rectTransform.localScale = Vector3.one * scale;
            time += Time.deltaTime;
            yield return null;
        }

        icon.rectTransform.localScale = endScale;
    }
}
