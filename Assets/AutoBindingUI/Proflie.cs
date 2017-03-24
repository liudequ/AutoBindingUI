using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AutoBindingUI
{

    public class UIProflie 
    {

        public string Name;

        public string Description;

        public List<UIProflie> Children;

        //todo 类型获取
    }




    public class Proflie
    {

        public static UIProflie Read(string text)
        {

            text = text.Trim();
            var split = text.Split('\n');
            List<string> items = new List<string>();
            for (int index = 0; index < split.Length; index++)
            {
                var s = split[index];
//                Debug.Log(s);
                items.Add(s);
            }

            var root = new UIProflie() { Name = "Root" };
            Read(items, root);
            return root;
        }


        private static void Read(List<string> childrenStr, UIProflie node)
        {
            UIProflie preNode = null;
            for (int i = 0; i < childrenStr.Count; i++)
            {
                string line = childrenStr[i];
                line = Regex.Replace(line, @"\s", "");
                var leftParenthesis = 0;
                if (line == "{")  // 当前开始一个子节点
                {
                    List<string> childLines = new List<string>();
                    leftParenthesis++;
                    for (i++; i < childrenStr.Count; i++)
                    {
                        line = childrenStr[i];
                        line = Regex.Replace(line, @"\s", "");
                        if (line == "{")
                        {
                            leftParenthesis++;
                        }
                        else if (line == "}")
                        {
                            leftParenthesis--;
                            if (leftParenthesis == 0)
                                break;
                        }
                        childLines.Add(line);
                    }

                    Read(childLines, preNode);
                }
                else
                {
                    if (node.Children == null) node.Children = new List<UIProflie>();
                    preNode = ReadLine(line);
                    node.Children.Add(preNode);
                }
            }
        }




        private static UIProflie ReadLine(string line)
        {
            var split = Regex.Split(line, "--", RegexOptions.IgnoreCase);
            UIProflie node = new UIProflie() { Name = split[0], Description = split[1] };
            return node;
        }

    }




}

