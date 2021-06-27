using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Events;

/// <summary>
/// 鼠标控制人物移动
/// </summary>

//[System.Serializable]//序列化，才能在Unity中显示出来  //注释原因：使用单例模式，节省拖拽角色到组件中这一行为
//public class EventVector3 : UnityEvent<Vector3> //创建事件
//{
//}
public class MouseManager : Singleton<MouseManager>
{


    public Texture2D point, doorway, attack, target, arrow;//鼠标图标变量

    RaycastHit hitInfo;

    public event Action<Vector3> OnMouseClicked;

    public event Action<GameObject> OnEnemyClicked;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

   

    void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()//设置鼠标贴图
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//返回 摄像机到鼠标点击位置的 一条射线

        if (Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch(hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target,new  Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                //default:
                //    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                //    break;
            }
        }
    }

    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)//如果按下鼠标 且 点击的位置不为空（点击位置不在地图外）
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))//如果按下的位置 的Tag是Ground
                //点击鼠标启用了这个事件,所有订阅这个事件添加进去的方法都会被执行
                OnMouseClicked?.Invoke(hitInfo.point);// ?是判断标识符， 当为true时实现后面的代码。将hitInfo.point参数传给事件处理器
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
                OnMouseClicked?.Invoke(hitInfo.point);
        }
    }

}
