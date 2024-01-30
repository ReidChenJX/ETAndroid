using System;
using System.IO;
using UnityEngine;

public class CreateScriptHead : UnityEditor.AssetModificationProcessor
{
    // Start is called before the first frame update
    // 在该项目下创建的Script 脚本会自动生成头注释
    public static void OnWillCreateAsset(string metaName)
    {
        string filePath = metaName.Replace(".meta", "");
        string fileExt = Path.GetExtension(filePath);
        if (fileExt != ".cs")
        {
            return;
        }

        string fileFullPath = Application.dataPath.Replace("Assets", "") + filePath;
        string fileContent = File.ReadAllText(fileFullPath);
        // 按照自己的设计添加需要自动生成的信息，调整好间距
        string commentContent =
            "/*\n *FileName:      #FILENAME#\n *Author:        #AUTHOR#\n *Date:          #DATE#\n *UnityVersion:  #UNITYVERSION#\n *Description:\n*/\n";
        commentContent = commentContent.Replace("#FILENAME#", Path.GetFileName(fileFullPath));
        commentContent = commentContent.Replace("#AUTHOR#", "ReidChen");
        commentContent = commentContent.Replace("#DATE#", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        commentContent = commentContent.Replace("#UNITYVERSION#", Application.unityVersion);
        fileContent = fileContent.Insert(0, commentContent);
        File.WriteAllText(fileFullPath, fileContent);
    }
}
