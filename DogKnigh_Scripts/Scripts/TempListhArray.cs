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

    //����ĳ���һ���������Ͳ��ܸ����ˣ�����������Ҫ���ⶨ��һ����ʱ���飬������ʱװ��ԭ����elementdata��Ԫ�أ�
    //Ȼ���ʼ��elementdata���飬����ʹ���ĳ��ȼ�1���ٰ���ʱ�����Ԫ�ظ�ֵ��ԭ���飬�������elementdata����ȥ����µ�Ԫ�أ�
    
    //����һ������Ϊ0��elementdata
    private object[] elementdata = { };
    //����һ��tempdata
    private object[] tempdata;

    public bool add(T t)
    {
        //��elementdata�е�Ԫ����ʱ��ŵ�tempdata��
        tempdata = elementdata;
        //��ʼ��elementdata�����ȼ�1
        elementdata = new Object[elementdata.Length + 1];

        //����tempdata����ȡtempdata�е�ÿһ��Ԫ��
        for(int i = 0;i < tempdata.Length; i++)
        {
            //��tempdata�е�Ԫ�ع黹��elementdata
            elementdata[i] = tempdata[i];
            //�����Ԫ��
            elementdata[elementdata.Length - 1] = (T)t;
            return true;
        }
        


        return false;
    }
}
