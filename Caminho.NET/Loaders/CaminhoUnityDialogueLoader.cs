#if UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Caminho.Loaders;
using UnityEngine;

public class CaminhoUnityDialogueLoader : ICaminhoDialogueLoader
{

    public string DialoguePath { get; set; }

    public CaminhoUnityDialogueLoader()
    {
        DialoguePath = "Dialogues";
    }

    Stream ICaminhoDialogueLoader.LoadDialogue(string file)
    {
        var assetName = string.Format("{0}/{1}", DialoguePath, file);

        var textAsset =
            Resources.Load(assetName, typeof(TextAsset)) as TextAsset;

        if (textAsset != null)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(textAsset.text);
            return new MemoryStream(byteArray);
        }

        return null;
    }

}
#endif
