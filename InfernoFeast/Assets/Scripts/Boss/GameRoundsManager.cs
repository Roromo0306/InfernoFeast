using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RoundConfig
{
    public string name = "Ronda";
    public float duration = 60f; // duración específica de esta ronda
    [Header("Opción A: Grupos ya en escena (referencias)")]
    public List<ClientTableGroup> groupsInScene = new List<ClientTableGroup>();
    [Header("Opción B: Prefabs a instanciar (si usas prefabs, déjalos vacíos en escena)")]
    public List<GameObject> groupPrefabs = new List<GameObject>(); // prefabs de ClientTableGroup
    [Header("Instanciación")]
    public Vector3 spawnPositionOffset = Vector3.zero; // opcional, si quieres instanciarlos con offset relativo al manager
}

public class GameRoundsManager : MonoBehaviour
{
    public static GameRoundsManager Instance { get; private set; }

    [Header("Rondas")]
    public List<RoundConfig> rounds = new List<RoundConfig>();

    [Header("Strikes")]
    public int maxStrikes = 3;
    public int currentStrikes = 0;

    [Header("Referencias")]
    public Timer timer;
    public DialogueBoss dialogueBoss;

    [Header("Escenas")]
    public string successSceneName = "VictoryScene";
    public string failSceneName = "FailScene";

    [Header("Mensajes (puedes editar)")]
    public List<string> winLines = new List<string>() { "¡Enhorabuena! Has superado el reto." };
    public List<string> loseLines = new List<string>() { "No has conseguido superar el reto. Inténtalo más tarde." };

    // estado
    List<ClientTableGroup> currentRoundGroups = new List<ClientTableGroup>();
    int currentRoundIndex = 0;
    string pendingSceneToLoad = "";

    // para limpiar instancias de prefabs cuando termine la ronda/partida
    List<GameObject> spawnedPrefabs = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (timer != null)
        {
            timer.OnTimerEnd += OnRoundTimeUp;
        }

        if (dialogueBoss != null)
        {
            dialogueBoss.OnDialogClosed += OnDialogClosed;
        }

        // Si no hay rondas configuradas, avisar (pero no lanzar)
        if (rounds == null || rounds.Count == 0)
        {
            Debug.LogWarning("[GameRoundsManager] No hay rondas configuradas en el inspector.");
            return;
        }

        // Iniciar la primera ronda
        StartRound(0);
    }

    void OnDestroy()
    {
        if (timer != null) timer.OnTimerEnd -= OnRoundTimeUp;
        if (dialogueBoss != null) dialogueBoss.OnDialogClosed -= OnDialogClosed;
    }

    void StartRound(int roundIndex)
    {
        // limpiar previas spawned (por si)
        CleanupSpawnedPrefabs();

        currentRoundIndex = roundIndex;
        currentRoundGroups.Clear();
        spawnedPrefabs.Clear();

        RoundConfig cfg = rounds[roundIndex];

        // 1) Añadir grupos referenciados en escena (Opción A)
        if (cfg.groupsInScene != null)
        {
            foreach (var g in cfg.groupsInScene)
            {
                if (g == null) continue;
                // asegurar que el grupo esté activo sólo si es parte de la ronda
                if (g.client != null) g.client.SetActive(true);
                if (g.table != null) g.table.SetActive(true);

                currentRoundGroups.Add(g);
            }
        }

        // 2) Instanciar prefabs (Opción B)
        if (cfg.groupPrefabs != null)
        {
            foreach (var prefab in cfg.groupPrefabs)
            {
                if (prefab == null) continue;
                Vector3 pos = transform.position + cfg.spawnPositionOffset;
                GameObject go = Instantiate(prefab, pos, Quaternion.identity);
                spawnedPrefabs.Add(go);

                ClientTableGroup g = go.GetComponent<ClientTableGroup>();
                if (g != null)
                {
                    currentRoundGroups.Add(g);
                }
            }
        }

        // Ajustar el Timer a la duración de la ronda actual (importante)
        if (timer != null)
        {
            timer.duration = cfg.duration;
            // si quieres mantener el countdown/behavior, lo dejamos como en el Timer
        }

        // Mostrar diálogo de inicio de ronda que arranca el timer al cerrarse
        if (dialogueBoss != null)
        {
            dialogueBoss.timer = timer;
            List<string> lines = new List<string> { $"Ronda {roundIndex + 1}: ¡A cocinar!" };
            dialogueBoss.StartDialog(lines);
        }
        else
        {
            timer?.StartTimer();
        }

        Debug.Log($"[GameRoundsManager] Iniciada ronda {roundIndex + 1} con {currentRoundGroups.Count} grupos y duración {cfg.duration}s.");
    }

    // llamado por Plate cuando se ancla en una mesa
    public void OnPlateDelivered(TableAnchor anchor, Plate plate)
    {
        if (anchor == null || anchor.group == null || plate == null) return;

        ClientTableGroup group = anchor.group;

        if (group.served) return;

        bool correct = (plate.dish == group.requiredDish);

        if (correct)
        {
            group.OnServed();
            Destroy(plate.gameObject, 0.05f);
            currentRoundGroups.Remove(group);

            if (currentRoundGroups.Count == 0)
            {
                // pasar a la siguiente ronda (si existe) o victoria
                NextRound();
            }
        }
        else
        {
            // comportamiento por plato incorrecto:
            // actualmente no hace nada (puedes añadir penalización aquí si quieres)
        }
    }

    void OnRoundTimeUp()
    {
        int missed = currentRoundGroups.Count;
        currentStrikes += missed;

        foreach (var g in currentRoundGroups)
        {
            if (g != null) g.OnMissed();
        }

        currentRoundGroups.Clear();

        if (currentStrikes >= maxStrikes)
        {
            LoseGame();
        }
        else
        {
            NextRound();
        }
    }

    void NextRound()
    {
        int next = currentRoundIndex + 1;
        if (next >= rounds.Count)
        {
            WinGame();
            return;
        }

        StartRound(next);
    }

    void WinGame()
    {
        timer?.StopTimer();
        if (dialogueBoss != null)
        {
            dialogueBoss.timer = null;
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
            dialogueBoss.timer = null;
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
        {
            SceneManager.LoadScene(pendingSceneToLoad);
        }
        else
        {
            // habitualmente el DialogueBoss ya habrá arrancado el timer para la ronda
        }
    }

    void CleanupSpawnedPrefabs()
    {
        if (spawnedPrefabs == null) return;
        foreach (var go in spawnedPrefabs)
        {
            if (go != null) Destroy(go);
        }
        spawnedPrefabs.Clear();
    }
}

