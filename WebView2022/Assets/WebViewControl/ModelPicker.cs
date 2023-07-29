using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TriLibCore;
#if WINDOWS_UWP
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

public class ModelPicker : MonoBehaviour
{
    public void OpenPicker()
    {
#if UNITY_EDITOR
        AssetLoader.LoadModelFromStream(new FileStream("C:/Users/akihiro/Downloads/Dolphin.obj", FileMode.Open), "Dolphin", "obj", load =>
        {
            load.RootGameObject.SetActive(true);
        });
#endif
        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
#if WINDOWS_UWP
            var filepicker = new FileOpenPicker();
            filepicker.FileTypeFilter.Add("*");
            filepicker.SuggestedStartLocation = PickerLocationId.Downloads;
            var file = await filepicker.PickSingleFileAsync(); // FilePickerを起動して選択されるのを待つ
            if (file == null) return;
            var data=await FileIO.ReadBufferAsync(file);
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                AssetLoader.LoadModelFromStream(data.AsStream(),Path.GetFileNameWithoutExtension(file.Name),Path.GetExtension(file.Name), load => {
                    load.RootGameObject.SetActive(true);
                }); 
            }, false);
#endif
        }, false);
    }
}
