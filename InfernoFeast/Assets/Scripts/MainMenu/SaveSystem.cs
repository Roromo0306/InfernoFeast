using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    public Button[] saveSlotButtons;
    public InputField nameInputField;
    public GameObject namePopup;
    public string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private int selectedSlot = -1;

    void Start()
    {
        LoadSlots();
    }

    void LoadSlots()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            string saveName = PlayerPrefs.GetString($"SaveSlot{i}_Name", "Empty Save");
            int savedDay = PlayerPrefs.GetInt($"SaveSlot{i}_Day", 0);

            string displayText = saveName;
            if (saveName != "Empty Save" && savedDay > 0)
            {
                displayText += " (" + days[savedDay % days.Length] + ")"; // Mostrar el día actual
            }

            saveSlotButtons[i].GetComponentInChildren<Text>().text = displayText;
        }
    }

    public void SelectSlot(int slot)
    {
        selectedSlot = slot;
        PlayerPrefs.SetInt("SelectedSlot", slot);

        string saveName = PlayerPrefs.GetString($"SaveSlot{slot}_Name", "Empty Save");
        int savedDay = PlayerPrefs.GetInt($"SaveSlot{slot}_Day", 1); // Día lunes por defecto

        if (saveName == "Empty Save")
        {
            namePopup.SetActive(true);
        }
        else
        {
            // Cargar el juego y establecer el día actual
            SceneManager.LoadScene("Restaurant");
            Debug.Log($"Loading game: {saveName}, Day: {savedDay}");
            PlayerPrefs.SetInt("CurrentDay", savedDay); // Para que el juego lo use
        }
    }

    public void ConfirmNewGame()
    {
        if (selectedSlot == -1 || string.IsNullOrEmpty(nameInputField.text)) return;

        PlayerPrefs.SetString($"SaveSlot{selectedSlot}_Name", nameInputField.text);
        PlayerPrefs.Save();

        namePopup.SetActive(false);
        LoadSlots();

        Debug.Log($"New game created in slot {selectedSlot}: {nameInputField.text}");
    }
}
