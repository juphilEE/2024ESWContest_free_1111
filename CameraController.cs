using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 10.0f; // 키보드 입력에 따른 회전 속도
    public float smoothSpeed = 1.0f;    // 부드러운 회전의 기본 속도

    public Transform baseViewObject;    // 기준이 되는 오브젝트
    private Coroutine currentRotationCoroutine;
    
    public void SetBaseViewObject(Transform newBaseViewObject)
    {
        baseViewObject = newBaseViewObject;
    }

    void Update()
    {
        // 추가적인 동작이 필요하다면 여기서 처리
    }

    public void SetRotation(float rotationX, float rotationY)
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
        }

        currentRotationCoroutine = StartCoroutine(SmoothRotation(rotationX, rotationY));
    }

    private IEnumerator SmoothRotation(float targetRotationX, float targetRotationY)
    {
        // 카메라와 baseViewObject 사이의 방향 벡터를 계산
        Vector3 directionToTarget = baseViewObject.position - transform.position;

        // 해당 방향 벡터를 사용하여 회전값을 계산
        Quaternion baseViewRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

        // 타겟 회전값을 3D 회전으로 변환
        Quaternion targetRotation = Quaternion.Euler(targetRotationX, targetRotationY+180, 0f);

        // 기준 회전값과 추가 회전값을 곱하여 최종 회전값을 계산
        Quaternion finalRotation = baseViewRotation * targetRotation;

        Quaternion currentRotation = transform.rotation;
        float angleDifference = Quaternion.Angle(currentRotation, finalRotation);
        float rotationSpeedAdjusted = smoothSpeed * angleDifference;

        float elapsedTime = 0.0f;

        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime * rotationSpeedAdjusted;
            transform.rotation = Quaternion.Slerp(currentRotation, finalRotation, elapsedTime);
            yield return null;
        }

        transform.rotation = finalRotation;
    }
}
