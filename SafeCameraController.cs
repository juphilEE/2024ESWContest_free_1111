using UnityEngine;
using UnityEngine.UI;

public class SafeCameraController : MonoBehaviour
{
    public RawImage display;  // USB 카메라 영상을 출력할 UI 요소
    private WebCamTexture webCamTexture;
    private bool isCameraRunning = false;
    public PlayerController playerController; // PlayerController 참조

    void Start()
    {
        // 연결된 웹캠 리스트 확인
        if (WebCamTexture.devices.Length > 0)
        {
            WebCamDevice device = WebCamTexture.devices[1]; // 첫 번째 웹캠 선택
            webCamTexture = new WebCamTexture(device.name);

            // 시작 시 RawImage를 비활성화
            display.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("No camera found!");
        }
        // PlayerController를 찾음
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    void Update()
    {
        // M 키 또는 3번 버튼을 눌렀을 때 카메라 토글
        if (Input.GetKeyDown(KeyCode.M) || playerController.buttons[2] == 1)
        {
            if (playerController != null)
            {
                playerController.HideSpeechBubble(); // 말풍선 닫기
            }
            ToggleCamera();
        }
    }

    private void ToggleCamera()
    {
        if (isCameraRunning)
        {
            StopCamera();
        }
        else
        {
            StartCamera();
        }
    }

    private void StartCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Play(); // 카메라 시작
            display.texture = webCamTexture; // 카메라 영상을 RawImage 텍스처로 설정

            // RawImage를 활성화
            display.gameObject.SetActive(true);

            isCameraRunning = true;
        }
    }

    private void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop(); // 카메라 정지

            // RawImage를 비활성화
            display.gameObject.SetActive(false);

            isCameraRunning = false;
        }
    }
}
