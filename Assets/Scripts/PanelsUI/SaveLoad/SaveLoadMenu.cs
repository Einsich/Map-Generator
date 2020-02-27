using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class SaveLoadMenu : MonoBehaviour
{
    public InputField SVInputTExt;

    public RectTransform listContent;
    public SaveLoadItem itemPrefab;
    public Button BackSVmenu;
    bool saveMode;
    public GameObject prevMenu;
    public Text InputHeader, InputAction;

    public void BackForSV(GameObject prev)
    {
        gameObject.SetActive(false);
        prev.SetActive(true);
    }

    public void Action()
    {
        string path = GetSelectedPath();
        if (path == null)
            return;
        if (saveMode)
            Save(path);
        else
        {
            gameObject.SetActive(false);
            Load(path);
        }
    }

    public void SelectItem(string name)
    {
        SVInputTExt.text = name;
    }



    void FillList()
    {
        for (int i = 0; i < listContent.childCount; i++)
            Destroy(listContent.GetChild(i).gameObject);
        

        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.ram");
        Array.Sort(paths);
        float h = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        listContent.sizeDelta = new Vector2(0, paths.Length * h);
        for (int i = 0; i < paths.Length; i++)
        {
            SaveLoadItem item = Instantiate(itemPrefab);
            item.menu = this;
            item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
            item.transform.SetParent(listContent, false);
        }
    }
    public void Save(string path)
    {
        Main.Save(path);
        FillList();
    }

    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist " + path);
            return;
        }
        CameraController.locked = false;
        Player.play = true;

        Main.Load(path);
    }

    public void Delete()
    {
        string path = GetSelectedPath();
        if (path == null)
        {
            return;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        SVInputTExt.text = "";
        FillList();
    }

    public void OpenSaveLoadPanel(bool saveMode)
    {

        this.saveMode = saveMode;
        gameObject.SetActive(true);
        prevMenu.SetActive(false);
        BackSVmenu.onClick.RemoveAllListeners();
        BackSVmenu.onClick.AddListener((() => BackForSV(prevMenu)));
        if (saveMode)
        {
            InputHeader.text = "Сохранение карты";
            InputAction.text = "Сохранить";
            SVInputTExt.interactable = true;
            SVInputTExt.text = "";
        }
        else
        {
            InputHeader.text = "Загрузка карты";
            InputAction.text = "Загрузить";
            SVInputTExt.interactable = false;
            SVInputTExt.text = "";
        }
        FillList();
    }

    string GetSelectedPath()
    {
        string mapName = SVInputTExt.text;
        if (mapName.Length == 0)
        {
            return null;
        }
        return Path.Combine(Application.persistentDataPath, mapName + ".ram");
    }
}
