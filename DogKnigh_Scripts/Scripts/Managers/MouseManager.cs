using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using UnityEngine.Events;

/// <summary>
/// �����������ƶ�
/// </summary>

//[System.Serializable]//���л���������Unity����ʾ����  //ע��ԭ��ʹ�õ���ģʽ����ʡ��ק��ɫ���������һ��Ϊ
//public class EventVector3 : UnityEvent<Vector3> //�����¼�
//{
//}
public class MouseManager : Singleton<MouseManager>
{


    public Texture2D point, doorway, attack, target, arrow;//���ͼ�����

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

    void SetCursorTexture()//���������ͼ
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//���� ������������λ�õ� һ������

        if (Physics.Raycast(ray, out hitInfo))
        {
            //�л������ͼ
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
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)//���������� �� �����λ�ò�Ϊ�գ����λ�ò��ڵ�ͼ�⣩
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))//������µ�λ�� ��Tag��Ground
                //����������������¼�,���ж�������¼���ӽ�ȥ�ķ������ᱻִ��
                OnMouseClicked?.Invoke(hitInfo.point);// ?���жϱ�ʶ���� ��Ϊtrueʱʵ�ֺ���Ĵ��롣��hitInfo.point���������¼�������
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
                OnMouseClicked?.Invoke(hitInfo.point);
        }
    }

}
