using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FinishCelebrationEffect : MonoBehaviour
{
    [Header("Celebration Settings")]
    [SerializeField] private int particleCount = 50;
    [SerializeField] private float particleLifetime = 2f;
    [SerializeField] private float explosionForce = 8f;
    [SerializeField] private float emissionRadius = 1f;
    
    private ParticleSystem ps;
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }
    
    void Start()
    {
        ps.Play();
        Destroy(gameObject, particleLifetime + 2f);
    }
    
    void ConfigureParticleSystem()
    {
        // Main module
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = explosionForce;
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;
        
        // Rainbow gradient
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0f, 0f), 0.0f),      // Red
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.2f),    // Orange
                new GradientColorKey(new Color(1f, 1f, 0f), 0.35f),     // Yellow
                new GradientColorKey(new Color(0f, 1f, 0f), 0.5f),      // Green
                new GradientColorKey(new Color(0f, 0.5f, 1f), 0.65f),   // Blue
                new GradientColorKey(new Color(0.5f, 0f, 1f), 0.8f),    // Purple
                new GradientColorKey(new Color(1f, 0f, 0.5f), 1.0f)     // Pink
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(gradient);
        
        // Emission - firework burst
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, particleCount),
            new ParticleSystem.Burst(0.2f, particleCount / 2),
            new ParticleSystem.Burst(0.4f, particleCount / 3)
        });
        
        // Shape - sphere explosion
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = emissionRadius;
        shape.radiusThickness = 1f;
        
        // Velocity over lifetime - gravity
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        
        // Add gravity
        AnimationCurve gravityCurve = new AnimationCurve();
        gravityCurve.AddKey(0f, 0f);
        gravityCurve.AddKey(1f, -5f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, gravityCurve);
        
        // Size over lifetime - grow then shrink
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Rotation over lifetime
        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
        
        // Color over lifetime - sparkle fade
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 1f, 0.8f), 0.5f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);
        
        // Renderer - additive for sparkles
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        // Additive blending for bright sparkles
        renderer.material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        renderer.material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
    }
    
    /// <summary>
    /// Spawn celebration effect at position
    /// </summary>
    public static void SpawnCelebration(Vector3 position)
    {
        GameObject effectObj = new GameObject("FinishCelebration");
        effectObj.transform.position = position;
        effectObj.AddComponent<FinishCelebrationEffect>();
    }
}
