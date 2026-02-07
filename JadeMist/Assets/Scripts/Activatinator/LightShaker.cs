using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class LightShaker : MonoBehaviour, Activatinator
{
    public AnimationCurve lightCurve;
    public float durationSec = 10;
    public float amplitude = 0.5f;
    public bool lightEnabled = true;
    public Light _light;
    public ParticleSystem fireParticles;

    float timeStart;
    float lightIntensity;
    float y;

    public void Activate()
    {
        lightEnabled = true;
        var m = fireParticles.main;
        m.prewarm = false;
        fireParticles.Play();
    }

    public void Deactivate()
    {
        lightEnabled = false;
        _light.intensity = 0;
        fireParticles.Stop();
    }

    void Start()
    {
        if (!lightEnabled)
        {
            Deactivate();
            fireParticles.Clear();
        }
        timeStart = Time.time - Random.Range(0f, durationSec);
        lightIntensity = _light.intensity;
        y = transform.position.y;
    }

    void Update()
    {
        if (!lightEnabled) return;

        var time = (Time.time - timeStart) / durationSec % 1f;
        _light.intensity = lightCurve.Evaluate(time) * lightIntensity;
        var x = _light.transform.position.x;
        var z = _light.transform.position.z;
        _light.transform.position = new Vector3(x, y + lightCurve.Evaluate(time) * amplitude, z);
    }
}
