using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    [Header("Configurações das Órbitas")]
    [Tooltip("Incremento da distância (raio) entre cada órbita planetária.")]
    [SerializeField] private float orbitRadiusStep = 5f;

    [Header("Prefabs dos Elementos Visuais (Tabela 4)")]
    [SerializeField] private GameObject cloudPrefab;        // n: Nuvem difusa (Particle System)
    [SerializeField] private GameObject protoStarPrefab;    // p: Proto-estrela
    [SerializeField] private GameObject starPrefab;         // e: Estrela madura
    [SerializeField] private GameObject protoDiskPrefab;     // d3, d2, d1: Disco protoplanetário
    [SerializeField] private GameObject ringPrefab;          // D: Anel planetário
    [SerializeField] private GameObject planetesimalPrefab;  // m: Planetesimal
    [SerializeField] private GameObject protoPlanetPrefab;    // N: Proto-planeta
    [SerializeField] private GameObject planetPrefab;         // P: Planeta formado
    [SerializeField] private GameObject moonPrefab;           // 1: Lua

    [Header("Contentor Pai")]
    [Tooltip("Objeto onde todos os elementos instanciados vão ser organizados.")]
    [SerializeField] private Transform systemParent;

    // Guarda as referências dos objetos instanciados para podermos limpar ao mudar de passo
    private List<GameObject> spawnedObjects = new List<GameObject>();

    /// <summary>
    /// Limpa a cena anterior e constrói a nova cena 3D com base na palavra atual do L-System.
    /// </summary>
    public void BuildScene(List<Symbol> word)
    {
        ClearScene();

        if (word == null || word.Count == 0) return;

        // Se não houver um contentor pai atribuído, usa o próprio Transform deste GameObject
        Transform parentTransform = systemParent != null ? systemParent : transform;

        // Pilha LIFO para gestão de ramificações (Tópico 2.3.2)[cite: 51]
        Stack<Transform> branchStack = new Stack<Transform>();

        // Índice para calcular o raio das órbitas (r1 = 1*step, r2 = 2*step, etc.)[cite: 51]
        int planetOrbitIndex = 1;

        // Referência ao último corpo planetário criado no nível principal[cite: 49]
        Transform currentBodyParent = parentTransform;

        // 1. PRIMEIRA PASSAGEM: Calcular o raio máximo para ajustar a dimensão da Nuvem 'n'[cite: 51]
        int totalPlanets = CountPlanetaryOrbits(word);
        float maxRadius = totalPlanets * orbitRadiusStep;

        // 2. SEGUNDA PASSAGEM: Instanciar e posicionar os objetos na cena
        foreach (Symbol symbol in word)
        {
            switch (symbol.Id)
            {
                case "n":
                    // Nuvem de gás e poeira[cite: 49]
                    SpawnCloud(parentTransform, maxRadius);
                    break;

                case "p":
                    // Proto-estrela no centro do sistema[cite: 49, 51]
                    currentBodyParent = SpawnObject(protoStarPrefab, parentTransform, Vector3.zero).transform;
                    break;

                case "e":
                    // Estrela madura no centro do sistema[cite: 49, 51]
                    currentBodyParent = SpawnObject(starPrefab, parentTransform, Vector3.zero).transform;
                    break;

                case "d":
                    // Disco protoplanetário (d3, d2, d1)[cite: 49, 51]
                    SpawnProtoDisk(parentTransform, symbol.Parameter);
                    break;

                case "D":
                    // Anel planetário na órbita de raio r_i[cite: 49, 51]
                    float ringRadius = planetOrbitIndex * orbitRadiusStep;
                    currentBodyParent = SpawnRing(parentTransform, ringRadius).transform;
                    planetOrbitIndex++;
                    break;

                case "m":
                    // Planetesimal na órbita de raio r_i[cite: 49, 51]
                    currentBodyParent = SpawnOrbitingBody(planetesimalPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "N":
                    // Proto-planeta na órbita de raio r_i[cite: 49, 51]
                    currentBodyParent = SpawnOrbitingBody(protoPlanetPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "P":
                    // Planeta na órbita de raio r_i[cite: 49, 51]
                    currentBodyParent = SpawnOrbitingBody(planetPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "1":
                    // Lua: orbitando o corpo imediatamente anterior que iniciou o ramo[cite: 49, 50, 51]
                    Transform moonParent = (branchStack.Count > 0) ? branchStack.Peek() : currentBodyParent;
                    SpawnMoon(moonParent);
                    break;

                case "[":
                    // Início de Ramo: Empilha o corpo atual[cite: 50, 51]
                    branchStack.Push(currentBodyParent);
                    break;

                case "]":
                    // Fim de Ramo: Restaura o contexto anterior[cite: 50, 51]
                    if (branchStack.Count > 0)
                    {
                        branchStack.Pop();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Destrói todos os GameObjects criados no passo anterior[cite: 51].
    /// </summary>
    public void ClearScene()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    public void SetOrbitRadiusStep(float newStep)
    {
        orbitRadiusStep = newStep;
    }

    #region Métodos Auxiliares de Instanciação

    private GameObject SpawnObject(GameObject prefab, Transform parent, Vector3 localPosition)
    {
        if (prefab == null) return parent.gameObject;

        GameObject instance = Instantiate(prefab, parent);
        instance.transform.localPosition = localPosition;
        spawnedObjects.Add(instance);
        return instance;
    }

    private void SpawnCloud(Transform parent, float maxRadius)
    {
        if (cloudPrefab == null) return;

        GameObject cloud = Instantiate(cloudPrefab, parent);
        cloud.transform.localPosition = Vector3.zero;

        ParticleSystem ps = cloud.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var shape = ps.shape;
            shape.radius = Mathf.Max(maxRadius, 5f);
        }
        else
        {
            float scale = Mathf.Max(maxRadius * 2f, 10f);
            cloud.transform.localScale = new Vector3(scale, scale, scale);
        }

        spawnedObjects.Add(cloud);
    }

    private void SpawnProtoDisk(Transform parent, int parameter)
    {
        if (protoDiskPrefab == null) return;

        GameObject disk = Instantiate(protoDiskPrefab, parent);
        disk.transform.localPosition = Vector3.zero;

        float scaleMultiplier = parameter * orbitRadiusStep;
        disk.transform.localScale = new Vector3(scaleMultiplier, 1f, scaleMultiplier);

        spawnedObjects.Add(disk);
    }

    private GameObject SpawnRing(Transform parent, float radius)
    {
        if (ringPrefab == null) return parent.gameObject;

        GameObject ring = Instantiate(ringPrefab, parent);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);

        spawnedObjects.Add(ring);
        return ring;
    }

    private GameObject SpawnOrbitingBody(GameObject prefab, Transform parent, float radius)
    {
        if (prefab == null) return parent.gameObject;

        GameObject body = Instantiate(prefab, parent);
        body.transform.localPosition = new Vector3(radius, 0f, 0f);

        spawnedObjects.Add(body);
        return body;
    }

    private void SpawnMoon(Transform parentBody)
    {
        if (moonPrefab == null || parentBody == null) return;

        GameObject moon = Instantiate(moonPrefab, parentBody);
        float moonOffset = 1.5f;
        moon.transform.localPosition = new Vector3(moonOffset, 0f, 0f);

        spawnedObjects.Add(moon);
    }

    private int CountPlanetaryOrbits(List<Symbol> word)
    {
        int count = 0;
        foreach (Symbol s in word)
        {
            if (s.Id == "D" || s.Id == "m" || s.Id == "N" || s.Id == "P")
            {
                count++;
            }
        }
        return count;
    }

    #endregion
}