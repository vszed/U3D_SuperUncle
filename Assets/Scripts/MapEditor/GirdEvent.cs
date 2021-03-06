﻿/*
 * 时间：2018年1月17日01:21:36
 * 作者：vszed
 * 功能：相应点击事件，将格子的pos传给绘制Block的pos
 * 注意：1.因为是物体绑定点击事件，一定要在物体上添加collider才能响应事件
 * 注意: 2.需要在Camera上添加射线检测
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GirdEvent : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setBlockPos()
    {
        if (GameObject.Find("ChangeFunction").GetComponent<Toggle>().isOn)
            EnemyEditor.getInstance().drawEnemy(new Vector3(transform.position.x, transform.position.y, 0), EnemyEditor.getInstance().toggle_type);
        else
            MapEditor.getInstance().drawBlock(new Vector3(transform.position.x, transform.position.y, 0), MapEditor.getInstance().toggleChoice, "None", 0);
    }

}
