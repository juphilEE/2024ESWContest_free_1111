using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float minAmplitude = 0.3f; // 진폭의 최소값
    public float maxAmplitude = 0.7f; // 진폭의 최대값
    public float minSpeed = 0.5f;     // 속도의 최소값
    public float maxSpeed = 1.5f;     // 속도의 최대값

    private float amplitude;
    private float speed;
    private Vector3 startPosition;

    void Start()
    {
        // 초기 위치 저장
        startPosition = transform.position;

        // 진폭과 속도를 랜덤으로 설정
        amplitude = Random.Range(minAmplitude, maxAmplitude);
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        Vector3 tempPosition = startPosition;
        tempPosition.y += Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = tempPosition;
    }
}
