using UnityEngine;

public class Dormir : MonoBehaviour
{
    public GameObject EndDayPannel;
    public GameTimeSystem gameTimeSystem;

    [HideInInspector] public bool EnContacto = false;
    [HideInInspector] public bool nuevoDia = false;

    private void Awake()
    {
        if (gameTimeSystem == null)
            gameTimeSystem = GameTimeSystem.Instance != null ? GameTimeSystem.Instance : FindObjectOfType<GameTimeSystem>();
    }

    private void Start()
    {
        if (EndDayPannel != null)
            EndDayPannel.SetActive(false);
    }

    private void Update()
    {
        if (!EnContacto)
            return;

        if (nuevoDia)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        nuevoDia = true;
        NuevoDia();
    }

    private void NuevoDia()
    {
        if (gameTimeSystem == null)
            gameTimeSystem = GameTimeSystem.Instance != null ? GameTimeSystem.Instance : FindObjectOfType<GameTimeSystem>();

        if (gameTimeSystem != null)
        {
            gameTimeSystem.Dormir();
            return;
        }

        if (EndDayPannel != null)
        {
            EndDayPannel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Dormir] Falta EndDayPannel o GameTimeSystem en " + gameObject.name);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
            EnContacto = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name == "Player")
            EnContacto = false;
    }
}