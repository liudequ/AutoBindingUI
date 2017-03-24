using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaGenerate
{
    /// <summary>
    /// 拼装的字符串
    /// </summary>
    private string luaStr;

    /// <summary>
    /// 缩进数
    /// </summary>
    private int indentsetCnt;

    /// <summary>
    /// table的个数
    /// </summary>
    private int tableCnt;

    #region 拼装

    /// <summary>
    /// Class.def(path,'baseCalss')
    /// </summary>
    /// <param name="path"></param>
    /// <param name="baseClass"></param>
    /// <returns></returns>
    public string ClassDef(string path, string baseClass)
    {
        string classdef = string.Format("Class.Def({0},'{1}')", path, baseClass);
        return classdef;
    }

    /// <summary>
    /// var:function
    /// </summary>
    /// <param name="var"></param>
    /// <param name="functionName"></param>
    /// <returns></returns>
    public string VariableFunction(string var, string functionName)
    {
        var vf = string.Format("{0}:{1}", var, functionName);
        return vf;
    }

    /// <summary>
    /// var1.var2
    /// </summary>
    /// <param name="var1"></param>
    /// <param name="var2"></param>
    /// <returns></returns>
    public string VariableVarialbe(string var1, string var2)
    {
        var vv = string.Format("{0}.{1}", var1, var2);
        return vv;
    }

    #endregion


    #region 添加项

    /// <summary>
    /// local xxx = xxx
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LuaGenerate Local(string name, string value)
    {
        this.AppendIndent();
        var local = string.Format("local {0} = {1}", name, value);
        this.luaStr += local;
        this.NextLine();
        return this;
    }

    /// <summary>
    /// self.XXX
    /// </summary>
    /// <param name="var"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public LuaGenerate SelfVariable(string var, string value = "")
    {
        this.AppendIndent();
        var sv = this.VariableVarialbe("self", var);
        if (!string.IsNullOrEmpty(value))
        {
            this.luaStr += string.Format("{0} = {1}", sv, value);
        }
        else
        {
            this.luaStr += sv;
        }
        this.Separate();
        this.NextLine();
        return this;
    }

    /// <summary>
    /// 添加注释
    /// </summary>
    /// <param name="comments">内容</param>
    /// <param name="isPreLine">是否为上一行加注释</param>
    /// <param name="spacing">距行首间隔</param>
    /// <returns></returns>
    public LuaGenerate AppendComments(string comments, bool isPreLine = true, int spacing = 102)
    {
        int alreadyWriteCnt = 0;  //当前要加注释行已经写入的字符数

        if (isPreLine)  ////求出倒数的两个'\n'间的内容长度
        {
            this.luaStr = this.luaStr.Remove(this.luaStr.Length - 1, 1);  //移除最后的换行
            for (int i = luaStr.Length - 1; i > 0; i--)
            {
                char c = luaStr[i];
                if (c != '\n')
                {
                    alreadyWriteCnt++;
                }
                else
                {
                    break;
                }
            }

            if (alreadyWriteCnt > spacing)  //超出规定的间隔
            {
                this.Space().Space();
            }
            else
            {
                for (; alreadyWriteCnt < spacing; alreadyWriteCnt++)
                {
                    this.Space();
                }
            }
        }
        else
        {
            this.NextLine();
            this.AppendIndent();
        }

        this.luaStr += string.Format("-- {0}", comments);
        this.NextLine();
        return this;
    }

    /// <summary>
    /// require
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public LuaGenerate Require(string param)
    {
        this.AppendIndent();
        var require = string.Format("require('{0}')", param);
        this.luaStr += require;
        this.NextLine();
        return this;
    }

    /// <summary>
    /// 方法
    /// </summary>
    /// <param name="name"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public LuaGenerate Function(string name, params string[] param)
    {
        this.AppendIndent();
        var paramStr = string.Empty;
        if (param != null && param.Length > 0)
        {
            for (int i = 0; i < param.Length - 1; i++)
            {
                var s = param[i];
                paramStr += string.Format("{0},", s);
            }
            paramStr += param[param.Length - 1];
        }

        var function = string.Format("function {0}({1})", name, paramStr);
        this.luaStr += function;
        this.NextLine();

        this.indentsetCnt++;
        return this;
    }

    /// <summary>
    /// 开始lua表
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public LuaGenerate BeginLuaTable(string name)
    {
        this.AppendIndent();
        var blt = this.VariableVarialbe("self", name);
        blt = string.Format("{0} = ", blt);
        this.luaStr += blt;
        this.NextLine();
        this.LefBracket();
        this.indentsetCnt++;
        this.tableCnt++;
        return this;
    }

    /// <summary>
    /// 结束lua表
    /// </summary>
    /// <returns></returns>
    public LuaGenerate EndLuaTable()
    {
        this.tableCnt--;
        this.indentsetCnt--;
        this.RightBracket();
        return this;
    }

    /// <summary>
    /// 分隔符
    /// </summary>
    /// <returns></returns>
    public LuaGenerate Separate()
    {
        if (this.tableCnt > 0)
        {
            this.Comma();
        }
        return this;
    }

    /// <summary>
    /// end
    /// </summary>
    /// <returns></returns>
    public LuaGenerate End()
    {
        this.luaStr += "end";
        this.NextLine();
        this.indentsetCnt--;
        this.AppendIndent();
        return this;
    }

    /// <summary>
    /// 开始块注释
    /// </summary>
    /// <returns></returns>
    public LuaGenerate BeginBlockComments()
    {
        this.luaStr += "--[[";
        this.NextLine();
        return this;
    }

    /// <summary>
    /// 结束块注释
    /// </summary>
    /// <returns></returns>
    public LuaGenerate EndBlockComments()
    {
        this.luaStr += "]]";
        this.NextLine();
        return this;
    }

    /// <summary>
    /// 换行
    /// </summary>
    /// <returns></returns>
    public LuaGenerate NextLine()
    {
        this.luaStr += '\n';
        return this;
    }

    /// <summary>
    /// 缩进
    /// </summary>
    /// <returns></returns>
    public LuaGenerate AppendIndent()
    {
        for (int i = 0; i < this.indentsetCnt; i++)
        {
            this.Tab();
        }
        return this;
    }

    /// <summary>
    /// 四个空格
    /// </summary>
    /// <returns></returns>
    public LuaGenerate Tab()
    {
        this.Space().Space().Space().Space();
        return this;
    }

    /// <summary>
    /// 空格
    /// </summary>
    /// <returns></returns>
    public LuaGenerate Space()
    {
        this.luaStr += ' ';
        return this;
    }

    /// <summary>
    /// 左括号
    /// </summary>
    /// <returns></returns>
    public LuaGenerate LefBracket()
    {
        this.AppendIndent();
        this.luaStr += "{";
        this.NextLine();
        return this;
    }

    /// <summary>
    /// 右括号
    /// </summary>
    /// <returns></returns>
    public LuaGenerate RightBracket()
    {
        this.AppendIndent();
        this.luaStr += "}";
        this.Separate();
        this.NextLine();
        return this;
    }


    /// <summary>
    /// 逗号
    /// </summary>
    /// <returns></returns>
    public LuaGenerate Comma()
    {
        this.luaStr += ",";
        return this;
    }

    #endregion

    public string ToString()
    {
        return this.luaStr;
    }
}
