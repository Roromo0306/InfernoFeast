using UnityEngine;

public class UpdateFadeShader : MonoBehaviour
{
    [Tooltip("Transform del personaje")]
    public Transform character;

    [Tooltip("Nombre del vector global usado por el shader")]
    public string shaderCharacterPosName = "_CharacterPos";

    void Update()
    {
        if (character != null)
        {
            Shader.SetGlobalVector(shaderCharacterPosName, character.position);
        }
    }
}
