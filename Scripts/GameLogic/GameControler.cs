﻿/*
 * 时间：2018年1月25日11:15:07
 * 作者：vszed
 * 功能：游戏流程总控制器：倒计时、游戏结束、呼出菜单等
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using UnityEngine.UI;

public class GameControler : MonoBehaviour
{
    //存放图块的transform
    public Transform TransformMapBlock;

    //倒计时时间label
    public GameObject LabelTimeLeft;

    //定时器
    private float currentTime;
    private float countDownUpdate;  //倒计时

    //游戏结束开关
    public bool GameOver;

    //清算关卡得分开关
    public bool balanceScoreSwitch;

    //4000分换1UP
    int ScoreToLives;

    //主菜单UI
    public GameObject menuUI;

    //单例
    private static GameControler instance;
    public static GameControler getInstance()
    {
        return instance;
    }

    // Use this for initialization
    void Start()
    {
        instance = this;

        countDownUpdate = Time.time;
        loadMap();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime = Time.time;

        if (currentTime - countDownUpdate >= 1 && GameOver == false)
        {
            countDownUpdate = Time.time;
            countDown();
        }

        popUpMenu();

        balanceScore();
    }

    public void loadMap()  //根据GameDt加载地图
    {
        string mapName = "map" + GameData.currentMissionNum;
        //Debug.Log(mapName);

        var JsonFile = Resources.Load(@"MapConfig/" + mapName) as TextAsset;
        var JsonObj = JsonMapper.ToObject(JsonFile.text);
        var JsonItems = JsonObj["MapBlocks"];

        foreach (JsonData item in JsonItems)
        {
            var x = Convert.ToSingle(item["position.x"].ToString());
            var y = Convert.ToSingle(item["position.y"].ToString());
            var type = int.Parse(item["type"].ToString());
            var blockEvent = item["blockEvent"].ToString();
            var doEventTimes = int.Parse(item["doEventTimes"].ToString());

            //MapEditor.getInstance().drawBlock(new Vector3(x, y, 0), type);

            var prefabBlock = Instantiate(Resources.Load("Prefab/BlockPrefab/Ground_" + type.ToString()), new Vector3(x, y, 0), new Quaternion(), TransformMapBlock);

            //坐标作为图块Name
            prefabBlock.name = new Vector3(x, y, 0).ToString();
            ((GameObject)prefabBlock).GetComponent<MapBlock>().type = type;

            switch (blockEvent)
            {
                case "Coin":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.Coin;
                        break;
                    }
                case "Goomba":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.Goomba;
                        break;
                    }
                case "TortoiseFly":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.TortoiseFly;
                        break;
                    }
                case "TortoiseLand":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.TortoiseLand;
                        break;
                    }
                case "UnmatchStar":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.UnmatchStar;
                        break;
                    }
                case "PlayBGM_SickCow":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.PlayBGM_SickCow;
                        break;
                    }
                case "PlayBGM_Schnappi":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.PlayBGM_Schnappi;
                        break;
                    }
                case "PlaySE_Hahaha":
                    {
                        ((GameObject)prefabBlock).GetComponent<MapBlock>().BlockEvent = MapBlock.EventType.PlaySE_Hahaha;
                        break;
                    }

                default:
                    break;
            }

            ((GameObject)prefabBlock).GetComponent<MapBlock>().canDoEventTimes = doEventTimes;
        }

        //================================================================================================================================//
        var JsonEnemies = JsonObj["EnemyBlocks"];

        foreach (JsonData item in JsonEnemies)
        {
            var x = Convert.ToSingle(item["position.x"].ToString());
            var y = Convert.ToSingle(item["position.y"].ToString());
            var type = item["type"].ToString();
            //Debug.Log(type);
            var prefabBlock = Instantiate(Resources.Load("Prefab/Enemy/" + type), new Vector3(x, y, 0), new Quaternion(), GameObject.FindWithTag("EnemyPack").transform);

            prefabBlock.name = type;
        }
    }

    //游戏倒计时
    public void countDown()
    {
        //Debug.Log(LabelTimeLeft.GetComponent<Text>().text);
        var str_timeLeft = LabelTimeLeft.GetComponent<Text>().text;
        int result = 0;
        int.TryParse(str_timeLeft, out result);
        result = result <= 0 ? 0 : --result;
        str_timeLeft = result.ToString();

        switch (str_timeLeft.Length)
        {
            case 1:
                str_timeLeft = "00" + str_timeLeft;
                break;
            case 2:
                str_timeLeft = "0" + str_timeLeft;
                break;
            default:
                break;
        }

        LabelTimeLeft.GetComponent<Text>().text = str_timeLeft;

        if (result <= 100 && !AudioControler.getInstance().BGM_Ground_Hurry.isPlaying)
        {
            //BGM
            AudioControler.getInstance().stopAllBGM();
            AudioControler.getInstance().BGM_Ground_Hurry.PlayDelayed(2.5f);

            //SE
            AudioControler.getInstance().SE_HurryUp.Play();
        }
        else if (result == 0)
        {
            gameOver();
        }
    }

    //游戏结束
    public void gameOver()
    {
        if (GameOver == false)
        {
            GameOver = true;

            //BGM
            AudioControler.getInstance().stopAllBGM();

            Character.getInstance().characterDie();

            //游戏结束逻辑
            --GameData.MarioLives;

            if (GameData.MarioLives > 0)
            {
                StartCoroutine(SceneTransition.getInstance().loadScene("LivesScene", 2, 3));
            }
            else
            {
                //刷新游戏数据
                GameData.dataUpdate();
                StartCoroutine(SceneTransition.getInstance().loadScene("GameOverScene", 2, 3));
            }
        }
    }

    //分数控制(延时执行)
    public IEnumerator ScoreUIControl(int score, Vector2 pos, float delaySecond)
    {
        yield return new WaitForSeconds(delaySecond);

        //更改UI大分数
        int currentScore = 0;
        int.TryParse(GameObject.Find("ScoreNum").GetComponent<Text>().text, out currentScore);
        currentScore += score;
        currentScore = currentScore < 0 ? 0 : currentScore;
        var str_currentScore = currentScore.ToString();

        while (str_currentScore.Length < 9)
            str_currentScore = "0" + str_currentScore;

        GameObject.Find("ScoreNum").GetComponent<Text>().text = str_currentScore;

        //得分后实现“小分数”提醒
        var littleScoreTips = Instantiate(Resources.Load("Prefab/UI/GainScoreView"), new Vector2(pos.x, pos.y + 0.5f), new Quaternion());
        ((GameObject)littleScoreTips).GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 360));
        var scoreTexture = (Texture2D)Resources.Load("Pictures/UI/scoreNumbers/" + score.ToString());
        ((GameObject)littleScoreTips).GetComponent<SpriteRenderer>().sprite = Sprite.Create(scoreTexture, new Rect(0, 0, scoreTexture.width, scoreTexture.height), new Vector2(0.5f, 0.5f));
        GameObject.Destroy(littleScoreTips, 0.5f);
    }

    //金币控制
    public void coinControl(int coinAddNumber)
    {
        int currentCoin = 0;
        int.TryParse(GameObject.Find("CoinNum").GetComponent<Text>().text, out currentCoin);
        currentCoin += coinAddNumber;
        var str_coin = currentCoin.ToString();
        while (str_coin.Length < 2)
        {
            str_coin = "0" + str_coin;
        }
        GameObject.Find("CoinNum").GetComponent<Text>().text = str_coin;
    }

    //1UP加命
    public void oneUp()
    {
        AudioControler.getInstance().SE_ONE_UP.Play();
        Destroy(Instantiate(Resources.Load("Prefab/UI/1UP"), GameObject.Find("Canvas").transform), 1);
        ++GameData.MarioLives;
    }

    //呼出、关闭菜单（重开游戏、退出游戏等）
    public void popUpMenu()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Debug.Log("popUpMenu");
            AudioControler.getInstance().SE_SYS_PAUSE.Play();

            if (menuUI.activeSelf)
                menuUI.SetActive(false);
            else
                menuUI.SetActive(true);
        }
    }

    //分数结算
    public void balanceScore()
    {
        if (balanceScoreSwitch)
        {
            if (!AudioControler.getInstance().SE_SCORE_COUNT.isPlaying)
                AudioControler.getInstance().SE_SCORE_COUNT.Play();

            int currentScore = 0;
            int.TryParse(GameObject.Find("ScoreNum").GetComponent<Text>().text, out currentScore);

            currentScore = currentScore - 10 > 0 ? currentScore - 10 : 0;

            var str_currentScore = currentScore.ToString();
            while (str_currentScore.Length < 9)
                str_currentScore = "0" + str_currentScore;

            GameObject.Find("ScoreNum").GetComponent<Text>().text = str_currentScore;

            //分数兑换命数
            ScoreToLives += 10;
            if (ScoreToLives >= 4000)
            {
                ScoreToLives = 0;
                oneUp();
            }

            if (currentScore == 0)
            {
                balanceScoreSwitch = false;
                AudioControler.getInstance().SE_SCORE_COUNT.Stop();
                AudioControler.getInstance().SE_SCORE_COUNT_FINISH.Play();

                //排行榜界面
                goToRankListScene();
            }
        }
    }

    //过关->排行榜
    public void goToRankListScene()
    {
        //重置数据
        Flag.alreadyGetFlag = false;

        StartCoroutine(SceneTransition.getInstance().loadScene("RankListScene", 1.5f, 3));
    }

    //操作GameData：得分、耗时 数据用于“排行榜”
    public void countScoreTime()
    {
        //得分
        int.TryParse(GameObject.Find("ScoreNum").GetComponent<Text>().text, out GameData.GetScore);
        //耗时
        int.TryParse(LabelTimeLeft.GetComponent<Text>().text, out GameData.CostTime);
        GameData.CostTime = 300 - GameData.CostTime;  //耗时
    }
}