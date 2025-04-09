using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    float shakeTime;
    public static CameraShake ins;

    void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
    }

    public void Shake(float AmplitudeGain, float FrequencyGain, float Dur)
    {
        noise.AmplitudeGain = AmplitudeGain;
        noise.FrequencyGain = FrequencyGain;
        shakeTime = Dur;
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
        }
        if (shakeTime <= 0)
        {
            noise.AmplitudeGain = 1;
            noise.FrequencyGain = 1;
        }
    }
}