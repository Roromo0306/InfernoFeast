using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encimera : MonoBehaviour
{
    public List<RecetasSO> recetas;
    public GameObject PadreEncimera, objeto1, objeto2 = null;
    public TipoIngrediente mezcla;
    public bool TieneObjeto = false, EncontradoPareja = true;
    void Start()
    {
        
    }

    void Update()
    {
        if (PadreEncimera.transform.childCount == 1)
        {
            objeto1 = PadreEncimera.transform.GetChild(0).gameObject;
            TieneObjeto = true;
        }

        if(PadreEncimera.transform.childCount <= 0)
        {
            objeto1 = null;
            TieneObjeto = false;
        }

        if(objeto2 != null && objeto1 != null)
        {
            Recetas();
        }
       
    }

    private void Recetas()
    {

        for(int i = 0; i < recetas.Count; i++)
        {
            /*Debug.Log("Nombre objeto 1: " + objeto1.name);
            Debug.Log("Nombre objeto 2: " + objeto2.name);
            Debug.Log("Nombre Ingrediente 1: " + recetas[i].Ingrediente1.name);
            Debug.Log("Nombre Ingrediente 1: " + recetas[i].Ingrediente2.name);*/
            if (recetas[i].Ingrediente1.name == objeto1.name && recetas[i].Ingrediente2.name == objeto2.name)
            {
                Debug.Log("Encontrado");
                Destroy(objeto1);
                Destroy(objeto2);

                Instantiate(recetas[i].Resultado.prefabIngrediente, PadreEncimera.transform.position, recetas[i].Resultado.prefabIngrediente.transform.rotation, PadreEncimera.transform);
                PadreEncimera.transform.GetChild(0).name = PadreEncimera.transform.GetChild(0).name.Replace("(Clone)", "").Trim(); //Esto lo que hace es eliminar la palabara clone de su nombre

                objeto1 = objeto2 = null;
                EncontradoPareja = true;
                break;
            }
            else
            {
                EncontradoPareja = false;
            }
        }

        if (!EncontradoPareja)
        {
            Debug.Log("No Encontrado");
            Destroy(objeto1);
            Destroy(objeto2);

            Instantiate(mezcla.prefabIngrediente, PadreEncimera.transform.position, PadreEncimera.transform.rotation, PadreEncimera.transform);
            PadreEncimera.transform.GetChild(0).name = PadreEncimera.transform.GetChild(0).name.Replace("(Clone)", "").Trim(); //Esto lo que hace es eliminar la palabara clone de su nombre

            objeto1 = objeto2 = null;
        }
    }
}
