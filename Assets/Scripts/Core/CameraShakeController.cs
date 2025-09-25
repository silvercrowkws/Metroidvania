using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCam;

    private CinemachineBasicMultiChannelPerlin noise;

    private float shakeDuration;
    private float shakeElapsed;
    private float initialAmplitude;
    private float initialFrequency;

    private bool isShaking = false;

    void Awake()
    {
        noise = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void StartShake(float duration, float amplitude, float frequency)
    {
        if (noise == null) return;

        initialAmplitude = amplitude;
        initialFrequency = frequency;
        shakeDuration = duration;
        shakeElapsed = 0f;
        isShaking = true;

        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
    }

    void LateUpdate()
    {
        if (isShaking)
        {
            //shakeElapsed += Time.deltaTime;
            shakeElapsed += Time.unscaledDeltaTime;
            float normalizedTime = shakeElapsed / shakeDuration;

            // 점점 줄어들게 (선형 감소)
            float currentAmplitude = Mathf.Lerp(initialAmplitude, 0f, normalizedTime);
            float currentFrequency = Mathf.Lerp(initialFrequency, 0f, normalizedTime);

            noise.m_AmplitudeGain = currentAmplitude;
            noise.m_FrequencyGain = currentFrequency;

            if (shakeElapsed >= shakeDuration)
            {
                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;
                isShaking = false;
            }
        }
    }
}
