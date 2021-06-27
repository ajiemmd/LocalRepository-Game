using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempListhArray<T> : MonoBehaviour
{
    public static void Main(string[] args)
    {
        TempListhArray<object> list = new TempListhArray<object>();
        list.add(1);
        list.add(2);
        System.Console.WriteLine(list);
    }

    //数组的长度一旦被定义后就不能更改了，所以我们需要另外定义一个临时数组，用来暂时装载原数组elementdata的元素，
    //然后初始化elementdata数组，并且使它的长度加1，再把临时数组的元素赋值给原数组，最后再用elementdata数组去添加新的元素，
    
    //定义一个长度为0的elementdata
    private object[] elementdata = { };
    //定义一个tempdata
    private object[] tempdata;

    public bool add(T t)
    {
        //将elementdata中的元素暂时存放到tempdata中
        tempdata = elementdata;
        //初始化elementdata，长度加1
        elementdata = new Object[elementdata.Length + 1];

        //遍历tempdata，获取tempdata中的每一个元素
        for(int i = 0;i < tempdata.Length; i++)
        {
            //将tempdata中的元素归还给elementdata
            elementdata[i] = tempdata[i];
            //添加新元素
            elementdata[elementdata.Length - 1] = (T)t;
            return true;
        }
        


        return false;
    }
}
