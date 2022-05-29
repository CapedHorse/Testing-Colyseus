using Colyseus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using SchemaTest.ArraySchemaTypes;
using SchemaTest.FilteredTypes;

public class ColyseusClientManager : ColyseusManager<ColyseusClientManager>, IColyseusRoom
{
    public static ColyseusClientManager instance;
    

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
    public ColyseusClient client;

    public event ColyseusCloseEventHandler OnLeave;

    public ColyseusRoom<State> room;

    State asda;
    public override void InitializeClient()
    {
        base.InitializeClient();
        
        client = new ColyseusClient("https://dev.node.maenyo.id");
    }

    private void OnEnable()
    {
        //listening on changes when enemy players joined the room
        room.State.players.OnAdd += Players_OnAdd;
    }

    private void Room_OnStateChange(ColyseusClientManager state, bool isFirstState)
    {
        
    }

    private void OnDisable()
    {
        //listening on changes when enemy players joined the room
        room.State.players.OnAdd -= Players_OnAdd;
    }
    public async void JoiningRoom(string match_id, string access_token, UnityAction actionAfterJoin)
    {
        Dictionary<string, object> roomOptions = new Dictionary<string, object>
        {
            ["accessToken"] = access_token
        };

        room = await client.JoinById<State>(match_id, roomOptions);

        await room.Send("ready");

        //listening when gameover
        room.OnMessage<ColyseusClientManager>("gameOver", (message) =>
        {
            Debug.Log("GameOver! " + message);
            room.Leave();
        });

        //listening when there's error
        room.OnMessage<ColyseusClientManager>("error", (message) =>
        {
            Debug.Log(message);
           
        });

        //listening when a match start!
        room.OnMessage<ColyseusClientManager>("start", (message) =>
        {
            Debug.Log("Start the game!");
            actionAfterJoin?.Invoke();
            Debug.Log("Invoke what happens after joined the room");
        });
    }

    private void Players_OnAdd(int key, Player player)
    {
        Debug.Log(player + " has been added at " + key); //this player is an object, nfor c#, cannot be tracked for the user id
        // add your player entity to the game world!

        //listen on enemy's change
        player.OnChange += (update) =>
        {
            //update.ForEach((obj) =>
            //{
            //    Debug.Log(obj.Field);
            //    Debug.Log(obj.Value);
            //    Debug.Log(obj.PreviousValue);
            //});
            Debug.Log("isdead? " + update.Find(x => x.Field == "dead").Value);
            TestColyseus.instance.enemyDead = (bool) update.Find(x => x.Field == "dead").Value;
            Debug.Log("score? " + update.Find(x => x.Field == "score").Value);
            TestColyseus.instance.enemyScore = (int) update.Find(x => x.Field == "score").Value;
            if (TestColyseus.instance.enemyDead)
            {
                Debug.Log("reporting game over with last score: " + TestColyseus.instance.enemyScore);
            }
            else
            {
                TestColyseus.instance.enemyScoreText.text = TestColyseus.instance.enemyScore.ToString();
            }           
        };
        // force "OnChange" to be called immediatelly
        player.TriggerAll();
    }

    public async void SendScoreToServer(int score, string pubKey) //incremental, so every +1, need to be send, multiplied 5x means send 5x
    {
        await room.Send("score", Cryptor.Encrypt(score.ToString(), pubKey));
    }

    public async void NotifyDeadToServer(string replayData, string pubKey) //sending replay data
    {
        await room.Send("dead", Cryptor.Encrypt(replayData, pubKey));
    }

    public Task Connect()
    {
        throw new NotImplementedException();
    }

    public Task Leave(bool consented)
    {
        throw new NotImplementedException();
    }
}
