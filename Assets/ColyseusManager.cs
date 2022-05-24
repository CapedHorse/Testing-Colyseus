using Colyseus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColyseusManager : ColyseusManager<ColyseusManager>
{
    public static ColyseusManager instance;

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
    
    
    public override void InitializeClient()
    {
        base.InitializeClient();
        
        client = new ColyseusClient("https://dev.node.maenyo.id");
    }
}
