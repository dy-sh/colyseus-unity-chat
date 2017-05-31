using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Colyseus;

public class ChatTest : MonoBehaviour
{

    public void StartTest(Room chatRoom)
    {
        if (LoginGUI.roomName == "test1")
            StartCoroutine(Test1());
        if (LoginGUI.roomName == "test2")
            StartCoroutine(Test2());
        if (LoginGUI.roomName == "test3")
            StartCoroutine(Test3());
        if (LoginGUI.roomName == "test4")
            StartCoroutine(Test3());
    }


    private int ping;
    private IEnumerator Test1()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            SendMessage("ping: " + ++ping);
        }
    }

    private IEnumerator Test2()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            SendMessage("ping: " + ++ping);
        }
    }

    private IEnumerator Test3()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            SendMessage("ping: " + ++ping);
        }
    }

    private IEnumerator Test4()
    {
        while (true)
        {
            for (int i = 0; i < 10; i++)
            {
                SendMessage("ping: " + ++ping);
            }
            yield return new WaitForSeconds(0.001f);
        }
    }
}