using UnityEngine;
using System.Collections.Generic;

public class BlockParticleSystem : MonoBehaviour
{
    public static BlockParticleSystem Instance;

    [Header("Prefab")]
    public GameObject particlePrefab;

    [Header("Pool")]
    public int poolSize = 200;

    [Header("Break Particle")]
    public int breakParticleCount = 20;
    public float breakParticleLifetime = 1.5f;
    public float breakVelocityMin = 2f;
    public float breakVelocityMax = 5f;
    public float breakVelocityUp = 3f;

    [Header("Dig Particle")]
    public int digParticleCount = 4;
    public float digParticleLifetime = 0.8f;
    public float digVelocityMin = 0.5f;
    public float digVelocityMax = 2f;

    [Header("Place Particle")]
    public int placeParticleCount = 8;
    public float placeParticleLifetime = 0.6f;
    public float placeVelocityMin = 0.5f;
    public float placeVelocityMax = 1.5f;

    [Header("Physics")]
    public float particleGravity = 15f;
    public float particleDrag = 2f;
    public float particleScale = 0.15f;

    private Queue<PooledParticle> pool = new Queue<PooledParticle>();
    private List<PooledParticle> active = new List<PooledParticle>();

    private class PooledParticle
    {
        public GameObject obj;
        public MeshRenderer renderer;
        public Vector3 velocity;
        public float lifetime;
        public float maxLifetime;
        public MaterialPropertyBlock props;
    }

    void Awake()
    {
        Instance = this;
        InitPool();
    }

    void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(particlePrefab, transform);
            obj.SetActive(false);

            PooledParticle p = new PooledParticle
            {
                obj = obj,
                renderer = obj.GetComponent<MeshRenderer>(),
                props = new MaterialPropertyBlock()
            };
            pool.Enqueue(p);
        }
    }

    public void SpawnBreakParticles(Vector3 blockCenter, BlockSO block, BlockFace face)
    {
        for (int i = 0; i < breakParticleCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.4f, 0.4f));

            Vector3 vel = Random.insideUnitSphere.normalized * Random.Range(breakVelocityMin, breakVelocityMax);
            vel.y = Mathf.Abs(vel.y) + breakVelocityUp * Random.Range(0.5f, 1.2f);

            BlockFace randomFace = GetRandomFaceForBreak();
            Color color = BlockColorSampler.SampleRandomFromFace(block, randomFace);

            SpawnParticle(blockCenter + offset, vel, breakParticleLifetime, color);
        }
    }

    public void SpawnDigParticles(Vector3 hitPoint, BlockSO block, BlockFace face)
    {
        for (int i = 0; i < digParticleCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f));

            Vector3 vel = Random.insideUnitSphere.normalized * Random.Range(digVelocityMin, digVelocityMax);
            vel.y = Mathf.Abs(vel.y * 0.5f);

            Color color = BlockColorSampler.SampleRandomFromFace(block, face);
            SpawnParticle(hitPoint + offset, vel, digParticleLifetime, color);
        }
    }

    public void SpawnPlaceParticles(Vector3 blockCenter, BlockSO block)
    {
        for (int i = 0; i < placeParticleCount; i++)
        {
            float angle = (i / (float)placeParticleCount) * Mathf.PI * 2f;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * 0.4f,
                -0.4f + Random.Range(-0.1f, 0.1f),
                Mathf.Sin(angle) * 0.4f);

            Vector3 vel = new Vector3(
                Mathf.Cos(angle) * Random.Range(placeVelocityMin, placeVelocityMax),
                Random.Range(0.5f, 1.5f),
                Mathf.Sin(angle) * Random.Range(placeVelocityMin, placeVelocityMax));

            Color color = BlockColorSampler.SampleRandomFromFace(block, BlockFace.Bottom);
            SpawnParticle(blockCenter + offset, vel, placeParticleLifetime, color);
        }
    }

    BlockFace GetRandomFaceForBreak()
    {
        int r = Random.Range(0, 6);
        return (BlockFace)r;
    }

    void SpawnParticle(Vector3 position, Vector3 velocity, float lifetime, Color color)
    {
        if (pool.Count == 0) return;

        PooledParticle p = pool.Dequeue();
        p.obj.transform.position = position;
        p.obj.transform.rotation = Random.rotation;
        p.obj.transform.localScale = Vector3.one * particleScale * Random.Range(0.7f, 1.3f);
        p.velocity = velocity;
        p.lifetime = 0f;
        p.maxLifetime = lifetime;

        p.props.SetColor("_BaseColor", color);
        p.renderer.SetPropertyBlock(p.props);

        p.obj.SetActive(true);
        active.Add(p);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            PooledParticle p = active[i];
            p.lifetime += dt;

            if (p.lifetime >= p.maxLifetime)
            {
                p.obj.SetActive(false);
                pool.Enqueue(p);
                active.RemoveAt(i);
                continue;
            }

            p.velocity.y -= particleGravity * dt;
            p.velocity *= (1f - particleDrag * dt);

            p.obj.transform.position += p.velocity * dt;
            p.obj.transform.Rotate(new Vector3(90f, 60f, 30f) * dt);

            float t = p.lifetime / p.maxLifetime;
            if (t > 0.7f)
            {
                float fade = 1f - ((t - 0.7f) / 0.3f);
                p.obj.transform.localScale = Vector3.one * particleScale * fade;
            }
        }
    }
}