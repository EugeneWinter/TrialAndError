using UnityEngine;
using System.Collections.Generic;

public class KnappingParticles : MonoBehaviour
{
    public GameObject particlePrefab;
    public int particlesPerHit = 15;
    public float particleLifetime = 1f;
    public float particleSize = 0.02f;
    public float gravity = 3f;

    private Queue<PooledParticle> pool = new Queue<PooledParticle>();
    private List<PooledParticle> active = new List<PooledParticle>();

    private class PooledParticle
    {
        public GameObject obj;
        public Vector3 velocity;
        public float lifetime;
        public float maxLifetime;
    }

    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject obj = Instantiate(particlePrefab, transform);
            obj.transform.localScale = Vector3.one * particleSize;
            obj.SetActive(false);
            pool.Enqueue(new PooledParticle { obj = obj });
        }
    }

    public void Burst(Vector3 worldPos, Vector3 normal, Color color)
    {
        for (int i = 0; i < particlesPerHit; i++)
        {
            if (pool.Count == 0) break;

            PooledParticle p = pool.Dequeue();
            p.obj.transform.position = worldPos;
            p.obj.transform.rotation = Random.rotation;

            var mr = p.obj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", color);
                mr.SetPropertyBlock(block);
            }

            Vector3 randomDir = (normal + Random.insideUnitSphere * 0.6f).normalized;
            p.velocity = randomDir * Random.Range(0.3f, 1.2f);
            p.lifetime = 0f;
            p.maxLifetime = particleLifetime * Random.Range(0.7f, 1.3f);

            p.obj.SetActive(true);
            active.Add(p);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var p = active[i];
            p.lifetime += dt;

            if (p.lifetime >= p.maxLifetime)
            {
                p.obj.SetActive(false);
                pool.Enqueue(p);
                active.RemoveAt(i);
                continue;
            }

            p.velocity.y -= gravity * dt;
            p.obj.transform.position += p.velocity * dt;
            p.obj.transform.Rotate(new Vector3(180, 90, 45) * dt);
        }
    }
}