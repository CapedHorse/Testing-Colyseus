using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Leguar.TotalJSON;
using System.Net.Security;

public class TestColyseus : MonoBehaviour
{
    public GameObject exchangePanel, playPanel;
    public TMP_InputField tokenInput;
    public TextMeshProUGUI scoreText;

    JSON gameData;

    string accessToken, pubKey, match_id;
    public void ExchangeTokenForPlay()
    {
        StartCoroutine(Exchange());
    }

    public void ReplayData()
    {
        StartCoroutine(RequestReplayData());
    }
    private IEnumerator Exchange()
    {
        WWWForm tokenExchange = new WWWForm();
        tokenExchange.AddField("game_token", tokenInput.text);

        UnityWebRequest init = UnityWebRequest.Post("https://dev.node.maenyo.id/api/v1/match/exchange-token", tokenExchange);

        yield return init.SendWebRequest();

        if (init.result == UnityWebRequest.Result.Success)
        {
            gameData = JSON.ParseString(init.downloadHandler.text);
            Debug.Log(init.downloadHandler.text);
            
            //check if token is valid
            if (!gameData.GetBool("err"))
            {
                //start init game like chunk level player etc
                accessToken = gameData.GetJSON("data").GetString("token");
                pubKey = gameData.GetJSON("data").GetJSON("config").GetString("pub");
                match_id = gameData.GetJSON("data").GetString("match_id");
                Debug.Log("AccessToken: " + accessToken + " & pub key: " + pubKey);
                StartPlaying();
            }
            else
                Debug.Log("Error json data");
        }
        else
        {
            //Debug.Log("Server Error");
        }
    }

    public void StartPlaying()
    {
        exchangePanel.SetActive(false);
        playPanel.SetActive(true);

        //some colyseus code here

    }

    public void JoinRoom()
    {

    }

    private IEnumerator RequestReplayData()
    {
        UnityWebRequest getReplay = UnityWebRequest.Get("https://dev.node.maenyo.id/api/v1/match/replay/data?game_token=" + tokenInput.text);
        yield return getReplay.SendWebRequest();
        if (getReplay.result == UnityWebRequest.Result.Success)
        {
        Debug.Log(getReplay.downloadHandler.text);
        }
    }
}
