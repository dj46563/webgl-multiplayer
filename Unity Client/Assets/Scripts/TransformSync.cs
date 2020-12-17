using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using NetStack.Quantization;
using NetStack.Serialization;

public class TransformSync : MonoBehaviour
{
    private WebSocket _ws;
    private bool _open = false;

    BitBuffer _bitBuffer = new BitBuffer(1024);
    byte[] buffer = new byte[10];
    
    System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

    public GameObject PlayerPrefab;
    private Transform _ownedTransform;
    private Dictionary<string, Transform> otherPlayers = new Dictionary<string, Transform>();
    private string _ownedId;
    
    private BoundedRange[] worldBounds = new BoundedRange[3]
    {
        new BoundedRange(-50f, 50f, 0.05f),
        new BoundedRange(-50f, 50f, 0.05f),
        new BoundedRange(-50f, 50f, 0.05f),
    };

    // Start is called before the first frame update
    async void Start()
    {
        _ws = new WebSocket(Constants.GameServerHost);
        _ws.OnOpen += () => _open = true;
        _ws.OnError += msg => Debug.Log("Error: " + msg);
        _ws.OnMessage += WsOnOnMessage;
        await _ws.Connect();
    }

    private void WsOnOnMessage(byte[] data)
    {
        _bitBuffer.Clear();
        _bitBuffer.FromArray(data, data.Length);
        byte messageId = _bitBuffer.ReadByte();
        switch (messageId)
        {
            case 1:
            {
                string id = _encoding.GetString(data, 1, 32);
                _ownedId = id;
                Debug.Log("Create owner " + id);
                GameObject player = Instantiate(PlayerPrefab);
                _ownedTransform = player.transform;
                player.AddComponent<Movement>();
                _open = true;
                break;
            }
            case 2:
            {
                string id = _encoding.GetString(data, 1, 32);
                Debug.Log("Create non owner " + id);
                otherPlayers[id] = CreatePlayer();
                break;
            }
            case 3:
            {
                byte count = _bitBuffer.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    string id = _bitBuffer.ReadString();

                    QuantizedVector3 quantizedPosition = new QuantizedVector3(
                        _bitBuffer.ReadUInt(),
                        _bitBuffer.ReadUInt(),
                        _bitBuffer.ReadUInt()
                    );
                    Vector3 postion = BoundedRange.Dequantize(quantizedPosition, worldBounds);

                    // Don't create players or set positions if its for the owned player
                    if (id != _ownedId)
                    {
                        if (!otherPlayers.ContainsKey(id))
                        {
                            otherPlayers[id] = CreatePlayer();
                        }
                        otherPlayers[id].position = postion;
                    }
                }

                break;
            }
            case 4:
            {
                string id = _encoding.GetString(data, 1, 32);
                Debug.Log("Delete " + id);
                if (otherPlayers.ContainsKey(id))
                {
                    Destroy(otherPlayers[id].gameObject);
                    otherPlayers.Remove(id);
                }

                break;
            }
        }
    }

    private void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
                _ws.DispatchMessageQueue();
        #endif
    }

    async void OnDestroy()
    {
        await _ws.Close();
    }

    private void FixedUpdate()
    {
        if (!_open)
            return;
        
        QuantizedVector3 quantizedPosition = BoundedRange.Quantize(_ownedTransform.position, worldBounds);
        _bitBuffer.Clear();
        _bitBuffer.AddUInt(quantizedPosition.x)
            .AddUInt(quantizedPosition.y)
            .AddUInt(quantizedPosition.z)
            .ToArray(buffer);
        _ws.Send(buffer);
    }

    private Transform CreatePlayer()
    {
        return Instantiate(PlayerPrefab).transform;
    }
}
