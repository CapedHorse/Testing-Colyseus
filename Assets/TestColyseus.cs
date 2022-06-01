using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Leguar.TotalJSON;
using System.Net.Security;
using UnityEngine.SceneManagement;

namespace ColyseusTest
{
    public class TestColyseus : MonoBehaviour
    {
        public static TestColyseus instance;
        public GameObject exchangePanel, joiningPanel, playPanel, deadPanel;
        public TMP_InputField tokenInput, scoreInput;
        public TextMeshProUGUI scoreText, enemyScoreText;

        JSON gameData, replayData;

        //Gameplay variables
        public bool enemyDead;
        public int playerScore, enemyScore;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        string accessToken, pubKey, match_id;
        public void InitiateGame()
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
                    pubKey = gameData.GetJSON("data").GetString("pub");
                    match_id = gameData.GetJSON("data").GetString("match_id");
                    Debug.Log("AccessToken: " + accessToken + " & pub key: " + pubKey);
                    JoinLobby();
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
            joiningPanel.SetActive(false);
            exchangePanel.SetActive(false);
            playPanel.SetActive(true);
        }

        public void JoinLobby()
        {
            joiningPanel.SetActive(true);
            ColyseusClientManager.instance.JoinRoom("wss://dev.node.maenyo.id", match_id, accessToken);
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

        public void UpdateScore() //set in inspector
        {
            playerScore += int.Parse(scoreInput.text);
            scoreText.text = playerScore.ToString();

            ColyseusClientManager.instance.SendScoreToServer(playerScore, pubKey);
        }
        public void UpdateEnemyScore(int score) //sendmessage'd by jslib
        {
            enemyScore = score;
            enemyScoreText.text = enemyScore.ToString();
        }

        public void Dead() //set in inspector
        {
            deadPanel.SetActive(true);
            replayData = new JSON();
            //the flow is -> Collect to a JSON first, seperti biasa. -> stringify & masukin ke JSON baru lagi yg bernama "replayData" (ini yg akan dibaca di server) -> stringify lagi, setor ke server.
            JSON replayLog = new JSON();
            replayLog.Add("log", "this is ceritanya replay log " + DateTime.Now);
            replayLog.Add("lastScore", playerScore);

            replayData.Add("replayData", replayLog);

            ColyseusClientManager.instance.NotifyDeadToServer(replayData.CreateString(), pubKey);
        }

        public void Restart() //set in inspector
        {
            SceneManager.LoadScene(0);
        }
    }
}

