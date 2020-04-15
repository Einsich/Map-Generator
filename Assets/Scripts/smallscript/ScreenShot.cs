using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{

    void Start()
    {
        InputManager.Instance.F12 += MakeScreenShot;
    }
    private void OnDestroy()
    {
        InputManager.Instance.F12 -= MakeScreenShot;

    }
    void MakeScreenShot()
    {
        string folder = Application.dataPath + "/ScreenShotsFolder";
        System.IO.Directory.CreateDirectory(folder);
        int ind = 0;
        foreach (var s in System.IO.Directory.GetFiles(folder))
        {
            var file = s.Split(' ');
            if (file.Length > 1)
            {
                int k;
                if (int.TryParse(file[1], out k))
                {
                    if (ind <= k)
                        ind = k + 1;
                }
            }
        }
        string path = $"{folder}/ScreenShot {ind} .png";
        ScreenCapture.CaptureScreenshot(path);
    }

}
