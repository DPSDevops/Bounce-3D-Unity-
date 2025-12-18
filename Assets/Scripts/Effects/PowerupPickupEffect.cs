using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PowerupPickupEffect : MonoBehaviour
{
    [Header("Effect Type")]
    public PowerupType effectType = PowerupType.SpeedBoost;
    
    [Header("Particle Settings")]
    [SerializeField] private int burstCount = 20;
    [SerializeField] private float particleLifetime = 1.5f;
    [SerializeField] private float particleSize = 0.3f;
    [SerializeField] private float emissionRadius = 0.5f;
    
    [Header("Movement")]
    [SerializeField] private float upwardForce = 5f;
    [SerializeField] private float spiralSpeed = 3f;
    
    public enum PowerupType
    {
        SpeedBoost,
        JumpBoost
    }
    
    private ParticleSystem ps;
    
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }
    
    void Start()
    {
        // Auto-play and destroy after lifetime
        ps.Play();
        Destroy(gameObject, particleLifetime + 2f);
    }
    
    void ConfigureParticleSystem()
    {
        // Main module
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = 2f;
        main.startSize = particleSize;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;
        
        // Set color based on powerup type
        if (effectType == PowerupType.SpeedBoost)
        {
            // Cyan/Blue gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0.0f),
                    new GradientColorKey(new Color(0.5f, 1f, 1f), 0.5f),
                    new GradientColorKey(new Color(0.3f, 0.8f, 1f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1.0f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(gradient);
        }
        else // JumpBoost
        {
            // Orange/Yellow gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.6f, 0.2f), 0.0f),
                    new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.6f, 0.2f), 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0.0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1.0f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(gradient);
        }
        
        // Emission - burst
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, burstCount)
        });
        
        // Shape - sphere
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = emissionRadius;
        shape.radiusThickness = 1f;
        
        // Velocity over lifetime - upward spiral
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        
        // Upward movement
        AnimationCurve upwardCurve = new AnimationCurve();
        upwardCurve.AddKey(0f, upwardForce);
        upwardCurve.AddKey(1f, upwardForce * 0.3f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, upwardCurve);
        
        // Spiral motion
        AnimationCurve spiralCurve = new AnimationCurve();
        spiralCurve.AddKey(0f, 0f);
        spiralCurve.AddKey(0.5f, spiralSpeed);
        spiralCurve.AddKey(1f, 0f);
        velocityOverLifetime.orbitalX = new ParticleSystem.MinMaxCurve(1f, spiralCurve);
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(1f, spiralCurve);
        
        // Size over lifetime - shrink
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - fade
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        // Enable additive blending for glow
        renderer.material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        renderer.material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
    }
    
    /// <summary>
    /// Static method to spawn a pickup effect at a position
    /// </summary>
    public static void SpawnEffect(Vector3 position, PowerupType type)
    {
        GameObject effectObj = new GameObject($"PickupEffect_{type}");
        effectObj.transform.position = position;
        
        var effect = effectObj.AddComponent<PowerupPickupEffect>();
        effect.effectType = type;
    }
}
