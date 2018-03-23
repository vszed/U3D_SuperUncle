﻿/*
 * 时间：2018年3月23日11:54:10
 * 作者：vszed
 * 功能：场景过渡 => 需要遮罩sprite
 * 用法：
 *       1.脚本挂载到遮罩上
 *       2.遮罩放置UI最上层且alpha为255
 *       3.Start()自动降低不透明度
 *       4.手动调用
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneTransition : MonoBehaviour
{
    public GameObject TransionMask;

    private static SceneTransition instance;

    public static SceneTransition getInstance()
    {
        return instance;
    }

    public bool IncreaseSwitch;
    public bool DeclineSwitch = true;

    private void Awake()
    {
        instance = this;
        TransionMask.SetActive(true);
        TransionMask.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);  //=>rect属性为readOnly
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        alphaIncrease();
        alphaDecline();
    }

    //场景跳转
    public IEnumerator loadScene(string SceneName, float delayTime)
    {
        DeclineSwitch = false;
        IncreaseSwitch = true;

        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(SceneName);
    }

    private void alphaIncrease()
    {
        if (IncreaseSwitch)
        {
            var color = TransionMask.GetComponent<Image>().color;
            if (color.a < 255)
                TransionMask.GetComponent<Image>().color = new Color(color.r, color.g, color.b, color.a + 0.01f);
        }
    }

    private void alphaDecline()
    {
        if (DeclineSwitch)
        {
            var color = TransionMask.GetComponent<Image>().color;
            if (color.a > 0)
                TransionMask.GetComponent<Image>().color = new Color(color.r, color.g, color.b, color.a - 0.01f);
        }
    }

}