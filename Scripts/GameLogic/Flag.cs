﻿/*
 * 时间：2018年3月27日01:34:45
 * 作者：vszed
 * 功能：挂在到旗子图块下
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    //castle读取此静态变量：true => 开启castle的mask
    public static bool alreadyGetFlag;
    bool playAnimSwitch;
    Collider2D ob;  //取旗的角色

    public GameObject redFlag;
    public GameObject blackFlag;

    //红旗上升最终坐标
    public Vector3 marioContactPos;
    //旗子上、下极限Y坐标(2.018, -1.845)
    public Transform upLimit;
    public Transform downLimit;

    // Use this for initialization
    void Start()
    {
        if (EnemyEditor.getInstance() != null)
        {
            GetComponentInParent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(isGroundedSetStatic());
        goToCastleAnim(ob);
        flagControl();
    }

    //落地后移除刚体
    public IEnumerator isGroundedSetStatic()
    {
        //在编辑地图中会直接删掉刚体
        if (GetComponentInParent<Rigidbody2D>() && GetComponentInParent<Rigidbody2D>().velocity.y == 0)
        {
            yield return new WaitForSeconds(2);

            Destroy(GetComponentInParent<Rigidbody2D>());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Character>() && !alreadyGetFlag)
        {
            alreadyGetFlag = true;
            marioContactPos = GameObject.Find("kosi_jnt").transform.position;  //碰撞后获取角色手骨坐标为取旗坐标
            StartCoroutine(getFlag(collision));
            getScore();
        }
    }

    //取得旗子
    public IEnumerator getFlag(Collider2D collision)
    {
        GameControler.getInstance().GameOver = true;
        collision.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        collision.GetComponentInChildren<Animator>().SetBool("isGetFlag", true);

        yield return new WaitForSeconds(0.15f);
        AudioControler.getInstance().stopAllBGM();

        collision.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        collision.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        AudioControler.getInstance().SE_SYS_GOAL_FLAG.Play();

        yield return new WaitForSeconds(1);
        AudioControler.getInstance().SE_course_clear.Play();
        playAnimSwitch = true;
        ob = collision;

        yield return new WaitForSeconds(1.5f);
        AudioControler.getInstance().SE_VOC_MA_CLEAR_NORMAL.Play();

        //取分、取耗时 => 排行榜
        GameControler.getInstance().countScoreTime();

        yield return new WaitForSeconds(3);
        GameControler.getInstance().balanceScoreSwitch = true;
    }

    //取旗后去城堡动画
    public void goToCastleAnim(Collider2D ob)
    {
        if (playAnimSwitch)
        {
            ob.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            var character = ob.gameObject;
            character.GetComponentInChildren<Animator>().SetBool("isGetFlag", false);
            character.GetComponentInChildren<Animator>().SetBool("goToCastle", true);

            //确保朝向右
            character.transform.rotation = Quaternion.Euler(0, 0, 0);
            character.transform.position = Vector2.MoveTowards(character.transform.position, new Vector2(character.transform.position.x + 1, character.transform.position.y), character.GetComponent<Character>().MoveSpeed / 2 * Time.deltaTime);
        }
    }

    //升降旗子
    public void flagControl()
    {
        if (alreadyGetFlag)
        {
            var color = redFlag.GetComponent<SpriteRenderer>().color;
            redFlag.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, color.a + 0.1f);

            if (marioContactPos.y > upLimit.position.y)
                marioContactPos.y = upLimit.position.y;
            else if (marioContactPos.y < downLimit.position.y)
                marioContactPos.y = downLimit.position.y;

            redFlag.transform.position = Vector3.MoveTowards(redFlag.transform.position, new Vector2(redFlag.transform.position.x, marioContactPos.y), 0.05f);
            blackFlag.transform.position = Vector3.MoveTowards(blackFlag.transform.position, new Vector2(blackFlag.transform.position.x, downLimit.position.y), 0.05f);

            if (blackFlag.transform.position.y <= downLimit.position.y)
            {
                var color_2 = blackFlag.GetComponent<SpriteRenderer>().color;
                blackFlag.GetComponent<SpriteRenderer>().color = new Color(color_2.r, color_2.g, color_2.b, color_2.a - 0.1f);
            }
        }
    }

    //取旗得分
    public void getScore()
    {
        int score = (int)((marioContactPos.y - downLimit.position.y) / (upLimit.position.y - downLimit.position.y) * 1000 / 100) * 100;
        score = score < 100 ? 100 : score;
        score = score > 1000 ? 1000 : score;
        //Debug.Log(score);
        //1.5为视觉偏移修正
        StartCoroutine(GameControler.getInstance().ScoreUIControl(score, new Vector2(redFlag.transform.position.x + 1.5f, marioContactPos.y), 0.1f));
    }
}
