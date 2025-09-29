using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleBarMap : MonoBehaviour
{
    public RectTransform handleObject; //Objecto que moverá el mapa
    public Scrollbar scrollbar; //El scrollbar en la UI
    public float minY = -200f;
    public float maxY = 200f;

   public void OnScrollValue()
    {
        float value = scrollbar.value; //Este valor va de 0 a 1
        float yPos = Mathf.Lerp(minY, maxY, value); 

        Vector2 newPos = handleObject.anchoredPosition;
        newPos.y = yPos;
        handleObject.anchoredPosition = newPos;
    }
}
