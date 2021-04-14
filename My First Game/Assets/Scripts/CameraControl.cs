using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制镜头跟随Player
/// </summary>
public class CameraControl : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.position = new Vector3(player.position.x, 0, -10f);//Y轴改为0可让镜头不在竖直移动，只水平移动
    }
}
