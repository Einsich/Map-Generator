using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public GameObject BackGround;
    public void ToGameMenu(bool tomenu)
    {
        BackGround.SetActive(tomenu);
        gameObject.SetActive(tomenu);
        CameraController.locked = tomenu;
        BackGround.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
    }
    public void QuitFromGame()
    {

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public void OpenSaveLoadMenu(bool saveMode)
    {
        MenuManager.OpenSaveLoadMenu(saveMode);
    }
}
