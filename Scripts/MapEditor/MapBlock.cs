﻿/*
 * 更改时间：2018年2月25日12:21:22
 * 作者：vszed
 * 功能：地图图块相关 => 图块type、碰撞事件
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBlock : MonoBehaviour
{
    //图块种类
    public int type;

    //能否触发事件
    public bool canDoEvent = true;

    private static MapBlock instance;

    public static MapBlock getInstance()
    {
        return instance;
    }

    // Use this for initialization
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //图块碰撞事件：根据type值匹配分发
    public void collisionEvent()
    {
        switch (type)
        {
            case 0:  //砖块
                {
                    brickEvent();
                    break;
                }
            default:
                break;
        }
    }

    #region BlockEvent

    void brickEvent()
    {
        if (canDoEvent)
        {
            canDoEvent = false;

            if (!AudioControler.getInstance().SE_Hit_Block.isPlaying)
            {
                AudioControler.getInstance().SE_Hit_Block.Play();
            }

            GetComponent<Animator>().SetBool("isHit", true);

            Invoke("resetCanDoEvent", 0.2f);
        }
    }

    #endregion

    //根据某些具体图块觉得是否可重置
    private void resetCanDoEvent()
    {
        canDoEvent = true;
    }
}
