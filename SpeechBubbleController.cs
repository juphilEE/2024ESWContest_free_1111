using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스 추가

public class SpeechBubbleController : MonoBehaviour
{
    public TextMeshProUGUI speechText; // TextMeshPro를 사용한 텍스트 요소
    public GameObject speechBubble;    // 말풍선 전체 오브젝트

    // 말풍선에 텍스트를 설정하고 표시하는 메서드
    public void ShowSpeech(string message, Color textColor, int fontSize)
    {
        if (speechText != null && speechBubble != null)
        {
            speechText.text = message; // 메시지 설정
            speechText.color = textColor; // 글씨 색상 설정
            speechText.fontSize = fontSize; // 글씨 크기 설정
            speechBubble.SetActive(true); // 말풍선 표시
        }
    }

    // 말풍선을 숨기는 메서드
    public void HideSpeech()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(false); // 말풍선 숨기기
        }
    }
}
