using UnityEngine;

public class InteractuarCounter : MonoBehaviour
{
    public GameObject Padre;
    public Animator animator;

    [HideInInspector] public bool Hold, Cortar, Pelar, Hornear, Hornear2, Hervir, Hervir2, Freir, Freir2, Batir, basura, Empezarturno, ObjetoDejado;

    [HideInInspector] public bool turnoEmpezado = false;

    [HideInInspector] public GameObject Counter, PadreFreir, PadreHorno, PadreHervir;

    [Header("Imagenes E")]
    public GameObject CortarE, Cortar2E, HornearE, Hornear2E, HervirE, Hervir2E, FreirE, Freir2E, BatirE, Batir2E, BasuraE, EmpezarTurnoE, CajaCarneE, CajaPescadoE, CajaEspeciasE, CajaPanE, CajaVegetalesE, EncimeraE, Encimera2E, CajaCarne2E, CajaPescado2E, CajaEspecias2E, CajaPan2E, CajaVegetales2E, CamaE;

    private CutCounter currentCutCounter;
    private BakeCounter currentBakeCounter;
    private PotCounter currentPotCounter;
    private FryCounter currentFryCounter;
    private MixCounter currentMixCounter;
    private Basura currentBasura;
    private EmpezarTurno currentEmpezarTurno;

    private void Update()
    {
        Hold = Padre != null && Padre.transform.childCount > 0;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (Empezarturno)
        {
            TryStartTurno();
            return;
        }

        if (!Hold)
            return;

        TryInteractWithCurrentCounter();
    }

    private void TryInteractWithCurrentCounter()
    {
        if (Counter == null)
            return;

        if (Cortar)
        {
            if (currentCutCounter == null)
                currentCutCounter = Counter.GetComponent<CutCounter>();

            if (currentCutCounter == null)
                return;

            if (animator != null)
                animator.SetTrigger("isCutting");

            currentCutCounter.cortar();
            return;
        }

        if (Hornear || Hornear2)
        {
            if (currentBakeCounter == null)
                currentBakeCounter = Counter.GetComponent<BakeCounter>();

            if (currentBakeCounter == null)
                return;

            ObjetoDejado = HasObjectInside(PadreHorno);
            if (!ObjetoDejado)
                currentBakeCounter.Hornear();

            return;
        }

        if (Hervir || Hervir2)
        {
            if (currentPotCounter == null)
                currentPotCounter = Counter.GetComponent<PotCounter>();

            if (currentPotCounter == null)
                return;

            ObjetoDejado = HasObjectInside(PadreHervir);
            if (!ObjetoDejado)
                currentPotCounter.Hervir();

            return;
        }

        if (Freir || Freir2)
        {
            if (currentFryCounter == null)
                currentFryCounter = Counter.GetComponent<FryCounter>();

            if (currentFryCounter == null)
                return;

            ObjetoDejado = HasObjectInside(PadreFreir);
            if (!ObjetoDejado)
                currentFryCounter.Freir();

            return;
        }

        if (Batir)
        {
            if (currentMixCounter == null)
                currentMixCounter = Counter.GetComponent<MixCounter>();

            if (currentMixCounter == null)
                return;

            if (animator != null)
                animator.SetTrigger("isMixing");

            currentMixCounter.StartMixing();
            return;
        }

        if (basura)
        {
            if (currentBasura == null)
                currentBasura = Counter.GetComponent<Basura>();

            if (currentBasura != null)
                currentBasura.Eliminar();
        }
    }

    private void TryStartTurno()
    {
        if (turnoEmpezado)
            return;

        if (Counter == null)
            return;

        if (currentEmpezarTurno == null)
            currentEmpezarTurno = Counter.GetComponent<EmpezarTurno>();

        if (currentEmpezarTurno == null)
            return;

        currentEmpezarTurno.TurnoStart();
        turnoEmpezado = true;
    }

    private bool HasObjectInside(GameObject parent)
    {
        return parent != null && parent.transform.childCount > 0;
    }

    private GameObject GetFirstChild(GameObject parent)
    {
        if (parent == null || parent.transform.childCount <= 0)
            return null;

        return parent.transform.GetChild(0).gameObject;
    }

    private void SetPrompt(GameObject prompt, bool active)
    {
        if (prompt != null)
            prompt.SetActive(active);
    }

    private void CacheCounter(GameObject counter)
    {
        Counter = counter;
        currentCutCounter = null;
        currentBakeCounter = null;
        currentPotCounter = null;
        currentFryCounter = null;
        currentMixCounter = null;
        currentBasura = null;
        currentEmpezarTurno = null;
    }

    private void ClearCounter(GameObject counter)
    {
        if (Counter != counter)
            return;

        Counter = null;
        currentCutCounter = null;
        currentBakeCounter = null;
        currentPotCounter = null;
        currentFryCounter = null;
        currentMixCounter = null;
        currentBasura = null;
        currentEmpezarTurno = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        string objectName = other.name;

        switch (objectName)
        {
            case "Cortar":
                Cortar = true;
                CacheCounter(other);
                currentCutCounter = other.GetComponent<CutCounter>();
                SetPrompt(CortarE, true);
                break;

            case "Cortar2":
                Cortar = true;
                CacheCounter(other);
                currentCutCounter = other.GetComponent<CutCounter>();
                SetPrompt(Cortar2E, true);
                break;

            case "Pelar":
                Pelar = true;
                CacheCounter(other);
                break;

            case "Horno":
                Hornear = true;
                CacheCounter(other);
                currentBakeCounter = other.GetComponent<BakeCounter>();
                SetPrompt(HornearE, true);
                PadreHorno = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreHorno);
                break;

            case "Horno2":
                Hornear2 = true;
                CacheCounter(other);
                currentBakeCounter = other.GetComponent<BakeCounter>();
                SetPrompt(Hornear2E, true);
                PadreHorno = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreHorno);
                break;

            case "Batir":
                Batir = true;
                CacheCounter(other);
                currentMixCounter = other.GetComponent<MixCounter>();
                SetPrompt(BatirE, true);
                break;

            case "Batir2":
                Batir = true;
                CacheCounter(other);
                currentMixCounter = other.GetComponent<MixCounter>();
                SetPrompt(Batir2E, true);
                break;

            case "Freir":
                Freir = true;
                CacheCounter(other);
                currentFryCounter = other.GetComponent<FryCounter>();
                SetPrompt(FreirE, true);
                PadreFreir = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreFreir);
                break;

            case "Freir2":
                Freir2 = true;
                CacheCounter(other);
                currentFryCounter = other.GetComponent<FryCounter>();
                SetPrompt(Freir2E, true);
                PadreFreir = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreFreir);
                break;

            case "Hervir":
                Hervir = true;
                CacheCounter(other);
                currentPotCounter = other.GetComponent<PotCounter>();
                SetPrompt(HervirE, true);
                PadreHervir = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreHervir);
                break;

            case "Hervir2":
                Hervir2 = true;
                CacheCounter(other);
                currentPotCounter = other.GetComponent<PotCounter>();
                SetPrompt(Hervir2E, true);
                PadreHervir = GetFirstChild(other);
                ObjetoDejado = HasObjectInside(PadreHervir);
                break;

            case "Basura":
                basura = true;
                CacheCounter(other);
                currentBasura = other.GetComponent<Basura>();
                SetPrompt(BasuraE, true);
                break;

            case "EmpezarTurno":
                Empezarturno = true;
                CacheCounter(other);
                currentEmpezarTurno = other.GetComponent<EmpezarTurno>();
                SetPrompt(EmpezarTurnoE, true);
                break;

            case "CajaCarne":
                SetPrompt(CajaCarneE, true);
                break;

            case "CajaPescado":
                SetPrompt(CajaPescadoE, true);
                break;

            case "CajaEspecias":
                SetPrompt(CajaEspeciasE, true);
                break;

            case "CajaPan":
                SetPrompt(CajaPanE, true);
                break;

            case "CajaVerdura":
                SetPrompt(CajaVegetalesE, true);
                break;

            case "Encimera":
                SetPrompt(EncimeraE, true);
                break;

            case "Encimera2":
                SetPrompt(Encimera2E, true);
                break;

            case "CajaCarne2":
                SetPrompt(CajaCarne2E, true);
                break;

            case "CajaPescado2":
                SetPrompt(CajaPescado2E, true);
                break;

            case "CajaEspecias2":
                SetPrompt(CajaEspecias2E, true);
                break;

            case "CajaPan2":
                SetPrompt(CajaPan2E, true);
                break;

            case "CajaVerdura2":
                SetPrompt(CajaVegetales2E, true);
                break;

            case "Cama":
                SetPrompt(CamaE, true);
                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject other = collision.gameObject;
        string objectName = other.name;

        switch (objectName)
        {
            case "Cortar":
                Cortar = false;
                ClearCounter(other);
                SetPrompt(CortarE, false);
                break;

            case "Cortar2":
                Cortar = false;
                ClearCounter(other);
                SetPrompt(Cortar2E, false);
                break;

            case "Pelar":
                Pelar = false;
                ClearCounter(other);
                break;

            case "Horno":
                Hornear = false;
                ClearCounter(other);
                SetPrompt(HornearE, false);
                ObjetoDejado = false;
                PadreHorno = null;
                break;

            case "Horno2":
                Hornear2 = false;
                ClearCounter(other);
                SetPrompt(Hornear2E, false);
                ObjetoDejado = false;
                PadreHorno = null;
                break;

            case "Batir":
                Batir = false;
                ClearCounter(other);
                SetPrompt(BatirE, false);
                break;

            case "Batir2":
                Batir = false;
                ClearCounter(other);
                SetPrompt(Batir2E, false);
                break;

            case "Freir":
                Freir = false;
                ClearCounter(other);
                SetPrompt(FreirE, false);
                ObjetoDejado = false;
                PadreFreir = null;
                break;

            case "Freir2":
                Freir2 = false;
                ClearCounter(other);
                SetPrompt(Freir2E, false);
                ObjetoDejado = false;
                PadreFreir = null;
                break;

            case "Hervir":
                Hervir = false;
                ClearCounter(other);
                SetPrompt(HervirE, false);
                ObjetoDejado = false;
                PadreHervir = null;
                break;

            case "Hervir2":
                Hervir2 = false;
                ClearCounter(other);
                SetPrompt(Hervir2E, false);
                ObjetoDejado = false;
                PadreHervir = null;
                break;

            case "Basura":
                basura = false;
                ClearCounter(other);
                SetPrompt(BasuraE, false);
                break;

            case "EmpezarTurno":
                Empezarturno = false;
                ClearCounter(other);
                SetPrompt(EmpezarTurnoE, false);
                break;

            case "CajaCarne":
                SetPrompt(CajaCarneE, false);
                break;

            case "CajaPescado":
                SetPrompt(CajaPescadoE, false);
                break;

            case "CajaEspecias":
                SetPrompt(CajaEspeciasE, false);
                break;

            case "CajaPan":
                SetPrompt(CajaPanE, false);
                break;

            case "CajaVerdura":
                SetPrompt(CajaVegetalesE, false);
                break;

            case "Encimera":
                SetPrompt(EncimeraE, false);
                break;

            case "Encimera2":
                SetPrompt(Encimera2E, false);
                break;

            case "CajaCarne2":
                SetPrompt(CajaCarne2E, false);
                break;

            case "CajaPescado2":
                SetPrompt(CajaPescado2E, false);
                break;

            case "CajaEspecias2":
                SetPrompt(CajaEspecias2E, false);
                break;

            case "CajaPan2":
                SetPrompt(CajaPan2E, false);
                break;

            case "CajaVerdura2":
                SetPrompt(CajaVegetales2E, false);
                break;

            case "Cama":
                SetPrompt(CamaE, false);
                break;
        }
    }
}