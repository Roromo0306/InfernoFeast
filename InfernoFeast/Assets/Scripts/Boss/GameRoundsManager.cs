using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRoundsManager : MonoBehaviour
{
    public static GameRoundsManager Instance { get; private set; }

    [Header("Rondas")]
    public int totalRounds = 3; // 3 rondas (índices 0..2)

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
    public List<string> roundStartLines = new List<string>() { "Ronda {0}: prepárate" }; // se formatea con número de ronda
    public List<string> winLines = new List<string>() { "¡Enhorabuena! Has superado el reto." };
    public List<string> loseLines = new List<string>() { "No has conseguido superar el reto. Inténtalo más tarde." };

    List<ClientTableGroup> allGroups = new List<ClientTableGroup>();
    List<ClientTableGroup> currentRoundGroups = new List<ClientTableGroup>();
    int currentRound = 0;
    string pendingSceneToLoad = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // recolectar todos los grupos de la escena
        allGroups = new List<ClientTableGroup>(FindObjectsOfType<ClientTableGroup>());

        if (timer != null)
        {
            timer.OnTimerEnd += OnRoundTimeUp;
        }

        if (dialogueBoss != null)
        {
            // Subscribimos el callback cuando el diálogo se cierra (para escenas finales)
            dialogueBoss.OnDialogClosed += OnDialogClosed;
        }

        StartRound(0);
    }

    void OnDestroy()
    {
        if (timer != null) timer.OnTimerEnd -= OnRoundTimeUp;
        if (dialogueBoss != null) dialogueBoss.OnDialogClosed -= OnDialogClosed;
    }

    void StartRound(int roundIndex)
    {
        currentRound = roundIndex;

        // Filtrar los grupos de esta ronda (los que tengan roundIndex == currentRound)
        currentRoundGroups.Clear();
        foreach (var g in allGroups)
        {
            if (g != null && g.roundIndex == currentRound && !g.served)
            {
                currentRoundGroups.Add(g);
                // asegurarnos de que estén visibles/activos al iniciar
                if (g.client != null) g.client.SetActive(true);
                if (g.table != null) g.table.SetActive(true);
            }
        }

        // Preparar diálogo de inicio de ronda.
        if (dialogueBoss != null)
        {
            dialogueBoss.timer = timer; // que el diálogo inicie el timer al cerrarse
            List<string> lines = new List<string> { string.Format("Ronda {0}. ¡A cocinar!", currentRound + 1) };
            dialogueBoss.StartDialog(lines);
        }
        else
        {
            // si no hay diálogo, arrancar el timer directamente
            timer?.StartTimer();
        }
    }

    // llamado por Plate cuando se ancla en una mesa
    public void OnPlateDelivered(TableAnchor anchor, Plate plate)
    {
        if (anchor == null || anchor.group == null || plate == null) return;

        ClientTableGroup group = anchor.group;

        // si ya estaba servido, ignoramos
        if (group.served) return;

        bool correct = (plate.dish == group.requiredDish);

        if (correct)
        {
            // marcar servido y eliminar grupo (visual)
            group.OnServed();
            // opcional: destruir plato
            Destroy(plate.gameObject, 0.05f);

            // quitar de la lista de grupos actuales
            currentRoundGroups.Remove(group);

            // comprobar si quedan clientes en la ronda
            if (currentRoundGroups.Count == 0)
            {
                // pasar a siguiente ronda (o victoria)
                NextRound();
            }
        }
        else
        {
            // Plato incorrecto: lo dejamos anclado (o podrías lanzarlo de vuelta)
            // Actualmente no damos strike por plato incorrecto, solo por no atender.
            // Puedes añadir efectos aquí (sonido, partícula, etc.)
        }
    }

    void OnRoundTimeUp()
    {
        // por cada cliente no atendido en la ronda, sumar un strike
        int missed = currentRoundGroups.Count;
        currentStrikes += missed;

        // marcar como no atendidos y desactivarlos
        foreach (var g in currentRoundGroups)
        {
            if (g != null)
                g.OnMissed();
        }

        currentRoundGroups.Clear();

        if (currentStrikes >= maxStrikes)
        {
            // perder
            LoseGame();
        }
        else
        {
            // pasar a siguiente ronda si existe
            NextRound();
        }
    }

    void NextRound()
    {
        int next = currentRound + 1;
        if (next >= totalRounds)
        {
            WinGame();
            return;
        }

        StartRound(next);
    }

    void WinGame()
    {
        // Diálogo de victoria y luego teleport (no queremos que DialogueBoss inicie timer)
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
        // Detenemos timer si está corriendo
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

    // llamado cuando el DialogueBoss cierra el panel (tanto en casos de inicio de ronda como en finales)
    void OnDialogClosed()
    {
        if (!string.IsNullOrEmpty(pendingSceneToLoad))
        {
            // cargar la escena pendiente (victoria / fallo)
            SceneManager.LoadScene(pendingSceneToLoad);
        }
        else
        {
            // si no hay escena pendiente, nada que hacer: en diálogos de inicio de ronda el DialogueBoss ya habrá arrancado el timer.
        }
    }
}
