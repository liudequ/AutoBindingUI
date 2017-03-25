using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AutoBindingUI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public class AutoBindingUIWindow : EditorWindow
{
    [MenuItem("Tools/AutoBindingUI")]
    public static void Open()
    {
        EditorWindow.GetWindow<AutoBindingUIWindow>();
    }


    private TextAsset proflieAsset;

    private UIProflie root;

    private GameObject target;


    private List<Object> bindingUIs;

    void OnGUI()
    {
        #region 
        //        if (GUILayout.Button("test"))
        //        {
        //            var generate = new LuaGenerate();
        //            generate.Require("GameBase.Common")
        //                .Local("_M", generate.ClassDef("...", "GameUI.UIBase.UIBhvrBase"))
        //                .Function(generate.VariableFunction("_M", "OnInit"))
        //                .BeginLuaTable("CompareItems")
        //                    .SelfVariable("HPArrLab")
        //                    .SelfVariable("DamageArrLab")
        //                    .SelfVariable("ForceArrLab")
        //                    .SelfVariable("RotateArrLab")
        //                    .BeginLuaTable("SelCraftDataObjs")
        //                        .SelfVariable("SelFirstObj")
        //                        .SelfVariable("SelSecondObj")
        //                        .SelfVariable("SelThirdObj")
        //                    .EndLuaTable()
        //                    .SelfVariable("Hahahahaah")
        //                .EndLuaTable()
        //                .End();
        //
        //            Debug.Log(generate.ToString());
        //        }
        //        return;
        #endregion

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("绑定UI描述文件");
        proflieAsset = EditorGUILayout.ObjectField(proflieAsset, typeof(TextAsset), true) as TextAsset;
        EditorGUILayout.EndHorizontal();
        if (proflieAsset != null && !string.IsNullOrEmpty(proflieAsset.text))
        {
            if (GUILayout.Button("Read") && proflieAsset != null)
            {
                root = Proflie.Read(proflieAsset.text);
            }
        }


        if (root != null && root.Children != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("绑定UI预制件");
            target = EditorGUILayout.ObjectField(target, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();

            if (target != null && GUILayout.Button("Binding"))
            {
                // 获取所有标识的UI组件对象
                bindingUIs = new List<Object>();
                var childTrans = target.transform.GetComponentsInChildren<Transform>(true);
                this.FindUI(root, childTrans);

                // 添加UILuaBhvr组件，Set UI组件


                // 生成相应Lua脚本
                var luaGen = new LuaGenerate();
                luaGen.Require("GameBase.Common")
                    .Local("_M", luaGen.ClassDef("...", "GameUI.UIBase.UIBhvrBase"))
                    .Function(luaGen.VariableFunction("_M", "OnInit"));
                this.GenerateLua(this.root.Children, luaGen, true);   //root节点下的除Table外属性可以被注释
                luaGen.End();
                string lua = luaGen.ToString();
                Debug.Log("生成lua内容：");
                Debug.Log(lua);
                string path = Application.dataPath;
                lua = Encoding.UTF8.GetString(Encoding.Default.GetBytes(lua));
                this.CreateFile(path, "UIBinding.lua", lua);
            }
        }
    }

    /// <summary>
    /// 生成lua脚本
    /// </summary>
    /// <param name="proflies"></param>
    /// <param name="luaGen"></param>
    /// <param name="isCommentsVariable">是否注释属性</param>
    private void GenerateLua(List<UIProflie> proflies, LuaGenerate luaGen, bool isCommentsVariable = false)
    {

        List<UIProflie> tableList = new List<UIProflie>();

        if (isCommentsVariable)
            luaGen.BeginBlockComments();

        for (int i = 0; i < proflies.Count; i++)
        {
            var proflie = proflies[i];

            if (proflie.Children != null && proflie.Children.Count > 0)
            {
                if (isCommentsVariable)
                {
                    tableList.Add(proflie);
                }
                else
                {
                    this.GenerateTable(proflie, luaGen);
                }

            }
            else
            {
                luaGen.SelfVariable(proflie.Name);
                luaGen.AppendComments(proflie.Description);
            }

        }

        if (isCommentsVariable)
        {
            luaGen.EndBlockComments();
            if (tableList.Count > 0)
            {
                for (int i = 0; i < tableList.Count; i++)
                {
                    var proflie = tableList[i];
                    this.GenerateTable(proflie, luaGen);
                }
            }
        }
    }


    private void GenerateTable(UIProflie proflie, LuaGenerate luaGen)
    {
        luaGen.AppendComments(proflie.Description,false);
        luaGen.BeginLuaTable(proflie.Name);
        this.GenerateLua(proflie.Children, luaGen);
        luaGen.EndLuaTable();
    }



    /// <summary>
    /// 从全部的UI对象里，找到有关联的UI
    /// </summary>
    /// <param name="UI"></param>
    /// <param name="proflie"></param>
    /// <param name="childTrans"></param>
    private void FindUI(UIProflie proflie, Transform[] childTrans)
    {
        if (proflie.Children != null)
        {
            foreach (var child in proflie.Children)
            {
                FindUI(child, childTrans);
            }
        }
        else
        {


            for (int i = 0; i < childTrans.Length; i++)
            {
                var children = childTrans[i].gameObject;
                if (children.name == proflie.Name)
                {
                    bindingUIs.Add(children);
                }
            }

        }
    }


    /** 
   * path：文件创建目录 
   * name：文件的名称 
   *  info：写入的内容 
   */
    void CreateFile(string path, string name, string info)
    {
        //文件流信息  
        StreamWriter sw;
        FileInfo t = new FileInfo(path + "//" + name);
        sw = t.CreateText();

        //以行的形式写入信息  
        sw.WriteLine(info);
        //关闭流  
        sw.Close();
        //销毁流  
        sw.Dispose();
    }
}
