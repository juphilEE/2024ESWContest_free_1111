using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UnitySocketServer : MonoBehaviour
{
    public CameraController cameraController;
    public PlayerController playerController;
    private TcpListener server;
    private Thread serverThread;

    void Start()
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError("CameraController could not be found. Please assign it in the Inspector.");
            }
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController could not be found. Please assign it in the Inspector.");
            }
        }

        serverThread = new Thread(new ThreadStart(ServerThread));
        serverThread.IsBackground = true;
        serverThread.Start();
        Debug.Log("Socket Server Started");
    }


    private void ServerThread()
    {
        server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
        server.Start();

        while (true)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                Debug.Log("Data received: " + dataReceived);

                // 수신된 데이터를 JSON으로 파싱
                ReceivedData receivedData = JsonUtility.FromJson<ReceivedData>(dataReceived);

                // 메인 스레드에서 회전 적용
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    cameraController.SetRotation(receivedData.rotationX, receivedData.rotationY);
                    Debug.Log($"Rotation applied - X: {receivedData.rotationX}, Y: {receivedData.rotationY}");

                    // playerController에 속도 값 전달
                    playerController.UpdateSpeed(receivedData.speed);
                    playerController.buttons = receivedData.buttons; // 버튼 상태 업데이트
                    Debug.Log($"Speed applied: {receivedData.speed}, Buttons: {string.Join(",", receivedData.buttons)}");
                });

                client.Close();
            }
            catch (Exception e)
            {
                Debug.Log("Socket exception: " + e);
            }
        }
    }

    void OnDestroy()
    {
        server.Stop();
        serverThread.Abort();
    }

    [Serializable]
    public class ReceivedData
    {
        public float rotationX;
        public float rotationY;
        public float speed;  // 속도 데이터 필드 추가
        public int[] buttons;  // OX 버튼 상태를 나타내는 배열 추가
    }
}
