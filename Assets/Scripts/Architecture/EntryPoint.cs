using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private ushort port = 7777;

    private void Start()
    {
        // Для тестування можна використовувати аргументи командного рядка
        ParseCommandLineArgs();
    }

    private void ParseCommandLineArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-server":
                    StartServer();
                    return;

                case "-client":
                    if (i + 1 < args.Length)
                    {
                        ipAddress = args[i + 1];
                        StartClient();
                    }
                    return;

                case "-host":
                    StartHost();
                    return;
            }
        }

        // Якщо немає аргументів, показуємо UI для вибору режиму
        ShowConnectionUI();
    }

    private void ShowConnectionUI()
    {
        // Додаємо обробник OnGUI як метод класу
        enabled = true;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (GUILayout.Button("Start Host"))
        {
            StartHost();
            GUI.changed = true;
        }

        if (GUILayout.Button("Start Server"))
        {
            StartServer();
            GUI.changed = true;
        }

        if (GUILayout.Button("Start Client"))
        {
            StartClient();
            GUI.changed = true;
        }

        GUILayout.EndArea();
    }

    private void StartHost()
    {
        networkManager.GetComponent<UnityTransport>().SetConnectionData(ipAddress, port);
        networkManager.StartHost();
        Debug.Log("Started Host");
    }

    private void StartServer()
    {
        networkManager.GetComponent<UnityTransport>().SetConnectionData(ipAddress, port);
        networkManager.StartServer();
        Debug.Log("Started Server");
    }

    private void StartClient()
    {
        networkManager.GetComponent<UnityTransport>().SetConnectionData(ipAddress, port);
        networkManager.StartClient();
        Debug.Log($"Started Client, connecting to {ipAddress}:{port}");
    }
}