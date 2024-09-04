using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CameraController cameraController; // CameraController 참조vv
    public float moveSpeed = 5f;        // 기본 이동 속도
    public float descendSpeed = 2f;     // 하강 속도
    public float ascendSpeed = 3f;      // 상승 속도
    public Transform[] waypoints;       // 경로를 구성하는 Waypoints 배열
    private int currentWaypointIndex = 0; // 현재 목표로 하는 Waypoint의 인덱스
    private Vector3 startPosition;      // 초기 위치 저장

    public float currentSpeed = 0f;    // 현재 속도
    private float accelerationTime = 2f; // 가속/감속 시간

    private bool isStopped = false;    // 상호작용을 위해 멈췄는지 여부

    public SpeechBubbleController speechBubbleController; // 말풍선 컨트롤러
    public Animator animator; // 애니메이터 추가

    // 버튼 입력
    public int[] buttons = new int[3]; // OX 버튼 값 [0, 1] 순서로 받음. 0: O, 1: X

    public void UpdateSpeed(float newSpeed)
    {
        // 속도 업데이트 메서드
        // 현재는 사용되지 않지만, 외부에서 속도 업데이트를 받는 기능을 구현할 수 있습니다.
        currentSpeed = newSpeed * 5;
        Debug.Log("Speed updated: " + currentSpeed);
    }

    void Start()
    {
        // 게임 시작 시 초기 위치 저장
        startPosition = transform.position;
    }

    void Update()
    {
        // 상호작용 중이라면 입력을 처리
        if (isStopped)
        {
            HandleInteractionInput(); // O 또는 X 입력 처리
        }
        else
        {
            // 현재 속도를 10f로 고정
            // currentSpeed = 10f;
            if (currentSpeed == 0)
            {
                currentSpeed = 0f;
            }
            else
            {
                // 현재 속도를 유지하거나 증가시킵니다.
                currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.deltaTime / accelerationTime);
            }

            // Waypoint 배열이 비어있지 않은지 확인한 후 이동
            if (waypoints.Length > 0)
            {
                MoveAlongPath(); // 경로를 따라 이동
            }

            // 입력 받기
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            // 이동 벡터 계산
            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            // 플레이어 이동
            transform.Translate(move * currentSpeed * Time.deltaTime, Space.World);

            // Shift 키를 누르면 Y축 하강
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.Translate(Vector3.down * descendSpeed * Time.deltaTime, Space.World);
            }

            // Spacebar를 누르면 Y축 상승
            if (Input.GetKey(KeyCode.Space))
            {
                transform.Translate(Vector3.up * ascendSpeed * Time.deltaTime, Space.World);
            }

            // R 키 입력 체크 및 초기 위치로 리셋
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPosition(); // 플레이어 위치 초기화
            }
        }
    }

    void MoveAlongPath()
    {
        if (waypoints.Length == 0) return; // Waypoints가 없는 경우 종료

        // 현재 Waypoint로 이동
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPosition = targetWaypoint.position;

        // 현재 위치에서 목표 위치로 부드럽게 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
        Transform viewpoint = targetWaypoint.Find("viewpoint");
        // 목표 위치에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {

            // Waypoint 인덱스에 따른 애니메이션과 말풍선 설정
            switch (currentWaypointIndex)
            {
                case 0:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isJogging"); // 첫 번째 Waypoint에서 애니메이션 재생
                    StartCoroutine(WaitForAnimation(animator, "Idle", () =>{}));
                    PlayWaypointAnimation("isGreeting");
                    ShowSpeechBubble(
                    "안녕하세요! 저는 통영의 마스코트, 동백이에요!\n" +
                    "저는 통영의 아름다운 자연과 따뜻한 마음을 상징하는 동백꽃에서 태어났답니다.\n" +
                    "통영의 역사와 문화를 알리고, 많은 분들이 통영의 매력을 느낄 수 있도록 도와주는 게 제 역할이에요!\n" +
                    "겨울에도 활짝 피어나는 동백꽃처럼, 언제나 밝고 활기찬 에너지를 전해드릴게요. 통영에 놀러 오시면 꼭 저를 기억해 주세요!\n" +
                    "\n계속 : O X",
                    new Color(0,0,0), 11
                    );

                    break;
                case 1:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isSpeaking"); // 세 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "통영은 오랜 역사와 문화를 간직한 도시로, 조선시대부터 중요한 해양 중심지 역할을 해왔습니다.\n" +
                        "그 시절의 흔적을 따라 통영에는 여러 전통 한옥들이 남아 있어요.\n"+
                        "특히, 통제영이라는 이름에서도 알 수 있듯이 통제사들이 머물렀던 통제영지는 전통과 역사를 느낄 수 있는 중요한 유적지입니다.\n"+
                        "통제영은 조선시대 해군 지휘관들이 거주하던 관아였으며, 건축 양식에서 조선시대의 전통적인 한옥 건축미를 느낄 수 있습니다.\n"+
                        "한옥들은 자연과 조화를 이루는 배치와 구조로, 전통적인 한국 건축의 우아함을 보여주고 있지요.\n"+
                        "\n계속 : O X",
                        new Color(0, 0, 0), 11
                    );
                    break;
                case 2:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isSitting"); // 세 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "[OX QUIZ]"+
                        "\n통영은 삼도수군통제영의 줄인 말이다.",
                        new Color(0, 0, 1), 20
                    );

                    // 첫 번째 말풍선이 표시된 후 O 또는 X 입력 대기
                    StartCoroutine(WaitForInputAndShowNextBubble());
                    break;
                case 4:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isNo"); // 세 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "여기까지가 한옥의 설명이에요.... ",
                        new Color(0, 0, 0), 11
                    );
                    break;

                case 10:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isFalling"); // 세 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "아고고,,, 이 곳은..? 통영시를 대표하는 거북선이 있는곳이에요",
                        new Color(0, 0, 0), 11
                    );
                    break;
                case 11:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isSpeaking"); // 두 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "통영 하면 이순신 장군과 거북선을 빼놓을 수 없습니다."+
                        "통영 앞바다에서는 임진왜란 당시 한산도 대첩이라는 중요한 해전이 벌어졌습니다."+
                        "이 전투는 이순신 장군이 거북선을 활용해 일본군을 크게 물리친 전투로,"+
                        "한국사에서 매우 중요한 의미를 가지고 있습니다."+
                        "이순신 장군은 이 지역을 근거지로 삼아 일본군을 물리쳤습니다.",
                        new Color(0, 0, 0), 11
                    );
                    break;
                case 12:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isSpeaking"); // 두 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "그 전투의 핵심인 거북선은 통영을 대표하는 역사적 상징이 되었습니다."+
                        "통영의 이순신 공원에는 실제 거북선의 복원 모형이 전시되어 있어,"+
                        "그 시절의 해전과 이순신 장군의 업적을 직접 느낄 수 있습니다.",
                        new Color(0, 0, 0), 11
                    );
                    break;
                case 13:
                    isStopped = true; // Waypoint에 도달하면 멈춤
                    PlayWaypointAnimation("isInfo"); // 세 번째 Waypoint에서 애니메이션 재생
                    ShowSpeechBubble(
                        "[OX QUIZ]" +
                        "\n통영은 삼면이 바다으로 둘러싸인 도시이다.",
                        new Color(0, 0, 1), 20
                    );

                    // 첫 번째 말풍선이 표시된 후 O 또는 X 입력 대기
                    StartCoroutine(WaitForInputAndShowNextBubble());
                    break;
                default:
                    isStopped = false; // 멈추지 않고 계속 이동
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    break;
            }
        }
    }

    IEnumerator WaitForInputAndShowNextBubble()
    {
        // O 또는 X 입력 대기
        bool inputReceived = false;
        while (!inputReceived)
        {
            if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.X))
            {
                inputReceived = true;
            }
            yield return null; // 한 프레임 대기
        }

        // 입력이 완료되면 다음 애니메이션 및 말풍선 표시
        PlayWaypointAnimation("isYes");
        ShowSpeechBubble("ㅇ", new Color(1, 0, 0), 100);

        // 2초 후에 말풍선 숨기기
        yield return new WaitForSeconds(1f); // 2초 대기
        HideSpeechBubble(); // 말풍선 숨기기
    }

    // O 또는 X 입력 처리
    void HandleInteractionInput()
    {
        // 키보드 입력 또는 버튼 입력을 처리
        if (Input.GetKeyDown(KeyCode.O) || buttons[0] == 1) // 키보드 O 또는 버튼 O
        {
            HideSpeechBubble(); // 말풍선 숨기기
            PlayWaypointAnimation("isYes"); // O를 눌렀을 때 애니메이션 재생
            StartCoroutine(WaitForAnimation(animator, "Idle", () =>
            {
                // 애니메이션 완료 후 다음 Waypoint로 이동
                isStopped = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }));
        }
        else if (Input.GetKeyDown(KeyCode.X) || buttons[1] == 1) // 키보드 X 또는 버튼 X
        {
            HideSpeechBubble(); // 말풍선 숨기기
            PlayWaypointAnimation("isNo"); // X를 눌렀을 때 애니메이션 재생
            StartCoroutine(WaitForAnimation(animator, "Idle", () =>
            {
                // 애니메이션 완료 후 다음 Waypoint로 이동
                isStopped = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }));
        }
    }

    // 애니메이션 트리거 활성화 메서드
    void PlayWaypointAnimation(string triggerParameter)
    {
        if (animator != null)
        {
            // 전달된 트리거를 활성화하여 애니메이션 실행
            animator.SetTrigger(triggerParameter);
        }
    }

    // 말풍선 표시 메서드
    void ShowSpeechBubble(string message, Color textColor, int fontSize)
    {
        if (speechBubbleController != null)
        {
            speechBubbleController.ShowSpeech(message, textColor, fontSize);
        }
    }

    // 말풍선 숨기기 메서드
    public void HideSpeechBubble()
    {
        if (speechBubbleController != null)
        {
            speechBubbleController.HideSpeech();
        }
    }

    // 플레이어 위치 리셋 메서드
    void ResetPosition()
    {
        transform.position = startPosition; // 초기 위치로 리셋
        currentWaypointIndex = 0; // Waypoint 인덱스 리셋
        currentSpeed = 5f; // 속도 초기화
    }

    // 애니메이션이 끝날 때까지 대기하는 코루틴
    IEnumerator WaitForAnimation(Animator anim, string animationName, System.Action onComplete)
    {
        while (anim.GetCurrentAnimatorStateInfo(0).IsName(animationName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        onComplete?.Invoke(); // 애니메이션 완료 후 동작 수행
    }
}