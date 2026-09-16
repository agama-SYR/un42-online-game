using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class OnlineGame : MonoBehaviour
{
    public string serverUrl = "https://un42-online-game.onrender.com";
    private string playerId;
    private readonly Dictionary<string, GameObject> cubes = new Dictionary<string, GameObject>();
    private string status = "Connecting...";

    [Serializable] private class MoveRequest { public string id; public int dx; public int dz; }
    [Serializable] private class Player { public string id; public float x; public float z; }
    [Serializable] private class State { public Player[] players; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateGame()
    {
        if (FindFirstObjectByType<OnlineGame>() == null)
            new GameObject("Online Game").AddComponent<OnlineGame>();
    }

    private void Start()
    {
        playerId = "unity-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Game Floor";
        floor.transform.localScale = new Vector3(5, 1, 5);
        floor.GetComponent<Renderer>().material.color = new Color(0.15f, 0.35f, 0.28f);

        var camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Game Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }
        camera.transform.position = new Vector3(0, 26, -18);
        camera.transform.LookAt(Vector3.zero);
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            int dx = 0, dz = 0;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) dx--;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) dx++;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) dz++;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) dz--;
            }
            // Zero movement also registers the player and keeps idle players visible.
            var data = JsonUtility.ToJson(new MoveRequest { id = playerId, dx = dx, dz = dz });
            using (var request = new UnityWebRequest(serverUrl.TrimEnd('/') + "/move", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success) status = request.error;
            }
            using (var request = UnityWebRequest.Get(serverUrl.TrimEnd('/') + "/state"))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    status = "Connected";
                    var state = JsonUtility.FromJson<State>(request.downloadHandler.text);
                    if (state != null && state.players != null) ShowPlayers(state.players);
                }
                else status = "Server: " + request.error;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void ShowPlayers(Player[] players)
    {
        var active = new HashSet<string>();
        foreach (var player in players)
        {
            active.Add(player.id);
            if (!cubes.TryGetValue(player.id, out var cube))
            {
                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = player.id;
                cube.GetComponent<Renderer>().material.color = player.id == "browser"
                    ? Color.red : player.id == playerId ? Color.blue : Color.yellow;
                cubes.Add(player.id, cube);
            }
            cube.transform.position = new Vector3(player.x, 0.5f, player.z);
        }
        foreach (var id in new List<string>(cubes.Keys))
            if (!active.Contains(id)) { Destroy(cubes[id]); cubes.Remove(id); }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(15, 15, 900, 30), "UN42 Online Game | WASD / arrows | " + status);
        GUI.Label(new Rect(15, 42, 900, 30), "Server: " + serverUrl);
        string edited = GUI.TextField(new Rect(15, 70, 500, 25), serverUrl);
        if (edited != serverUrl) serverUrl = edited;
    }
}
