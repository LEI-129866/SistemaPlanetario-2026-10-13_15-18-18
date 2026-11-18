using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{
    
    [SerializeField] private float orbitRadiusStep = 5f;
    [SerializeField] private float currentOrbitSpeed = 4f;

    [Header("Prefabs dos Elementos Visuais")]
    [SerializeField] private GameObject cloudPrefab;        // n: Nuvem difusa (Particle System)
    [SerializeField] private GameObject protoStarPrefab;    // p: Proto-estrela
    [SerializeField] private GameObject starPrefab;         // e: Estrela madura
    [SerializeField] private GameObject protoDiskPrefab;     // d3, d2, d1: Disco protoplanetário
    [SerializeField] private GameObject ringPrefab;          // D: Anel planetário
    [SerializeField] private GameObject planetesimalPrefab;  // m: Planetesimal
    [SerializeField] private GameObject protoPlanetPrefab;    // N: Proto-planeta
    [SerializeField] private GameObject planetPrefab;         // P: Planeta formado
    [SerializeField] private GameObject moonPrefab;           // 1: Lua

    [SerializeField] private GameObject systemOrigin;
    
    // Guarda as referências dos objetos instanciados para podermos limpar ao mudar de passo
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<OrbitParent> planetaryOrbits = new List<OrbitParent>();

    // Limpa a cena anterior e constrói a nova cena 3D com base na palavra atual do L-System.
    public void BuildScene(List<Symbol> word)
    {
        ClearScene();

        if (word == null || word.Count == 0) return;

        // Se não houver um contentor pai atribuído, usa o próprio Transform deste GameObject
        Transform parentTransform = systemOrigin != null ? systemOrigin.transform : transform;

        // Pilha LIFO para gestão de ramificações 
        Stack<Transform> branchStack = new Stack<Transform>();

        // Índice para calcular o raio das órbitas
        int planetOrbitIndex = 1;

        // Referência ao último corpo planetário criado no nível principal
        Transform currentBodyParent = parentTransform;

        // Calcular o raio máximo para ajustar a dimensão da Nuvem 'n'
        int totalPlanets = CountPlanetaryOrbits(word);
        float maxRadius = totalPlanets * orbitRadiusStep;

        // Instanciar e posicionar os objetos na cena
        foreach (Symbol symbol in word)
        {
            switch (symbol.Id)
            {
                case "n":
                    // Nuvem de gás e poeira
                    SpawnCloud(parentTransform, maxRadius);
                    break;

                case "p":
                    // Proto-estrela no centro do sistema
                    currentBodyParent = SpawnObject(protoStarPrefab, parentTransform, Vector3.zero).transform;
                    break;

                case "e":
                    // Estrela madura no centro do sistema
                    currentBodyParent = SpawnObject(starPrefab, parentTransform, Vector3.zero).transform;
                    break;

                case "d":
                    // Disco protoplanetário (d3, d2, d1)
                    SpawnProtoDisk(parentTransform, symbol.Parameter);
                    break;

                case "D":
                    // Anel planetário na órbita de raio r_i
                    float ringRadius = planetOrbitIndex * orbitRadiusStep;
                    currentBodyParent = SpawnRing(parentTransform, ringRadius).transform;
                    planetOrbitIndex++;
                    break;

                case "M":
                    // Planetesimal na órbita de raio r_i
                    currentBodyParent = SpawnOrbitingBody(planetesimalPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "N":
                    // Proto-planeta na órbita de raio r_i
                    currentBodyParent = SpawnOrbitingBody(protoPlanetPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "P":
                    // Planeta na órbita de raio r_i
                    currentBodyParent = SpawnOrbitingBody(planetPrefab, parentTransform, planetOrbitIndex * orbitRadiusStep).transform;
                    planetOrbitIndex++;
                    break;

                case "1":
                    // Lua: orbita o corpo imediatamente anterior que iniciou o ramo
                    Transform moonParent = (branchStack.Count > 0) ? branchStack.Peek() : currentBodyParent;
                    SpawnMoon(moonParent);
                    break;

                case "[":
                    // Início de Ramo: Empilha o corpo atual
                    branchStack.Push(currentBodyParent);
                    break;

                case "]":
                    // Fim de Ramo: Restaura o contexto anterior
                    if (branchStack.Count > 0)
                    {
                        branchStack.Pop();
                    }
                    break;
            }
        }
    }

    // Destrói todos os GameObjects criados no passo anterior
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
        planetaryOrbits.Clear();
    }

    public void SetOrbitRadiusStep(float newStep)
    {
        orbitRadiusStep = newStep;

        // atualiza o raio dos planetas existentes em tempo real sem destruir/reiniciar a órbita
        for (int i = 0; i < planetaryOrbits.Count; i++)
        {
            if (planetaryOrbits[i] != null)
            {
                planetaryOrbits[i].SetRadius((i + 1) * orbitRadiusStep);
            }
        }
    }

    public void SetOrbitSpeed(float newSpeed)
    {
        currentOrbitSpeed = newSpeed;
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                OrbitParent orbit = obj.GetComponent<OrbitParent>();
                if (orbit != null)
                {
                    orbit.SetSpeed(currentOrbitSpeed);
                }
            }
        }
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
            //float scale = Mathf.Max(maxRadius * 2f, 10f);
            //cloud.transform.localScale = new Vector3(scale, scale, scale);
        }

        spawnedObjects.Add(cloud);
    }

    private void SpawnProtoDisk(Transform parent, int parameter)
    {
        if (protoDiskPrefab == null) return;

        GameObject disk = Instantiate(protoDiskPrefab, parent);
        disk.transform.localPosition = Vector3.zero;

        float scaleMultiplier = 1 - ((3 - parameter) * 0.1f);
        disk.transform.localScale = new Vector3(disk.transform.localScale.x * scaleMultiplier,
                                                disk.transform.localScale.y,
                                                disk.transform.localScale.z * scaleMultiplier);

        spawnedObjects.Add(disk);
    }

    private GameObject SpawnRing(Transform parent, float radius)
    {
        if (ringPrefab == null) return parent.gameObject;

        GameObject ring = Instantiate(ringPrefab, parent);
        //ring.transform.localPosition = Vector3.zero;
        //ring.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);

        spawnedObjects.Add(ring);
        return ring;
    }

    private GameObject SpawnOrbitingBody(GameObject prefab, Transform parent, float radius)
    {
        if (prefab == null) return parent.gameObject;

        GameObject body = Instantiate(prefab, parent);

        // tentar corrigir erros do raio - ajustar OrbitParent se ele existir no Prefab
        OrbitParent orbitComp = body.GetComponent<OrbitParent>();
        if (orbitComp != null){
            orbitComp.SetRadius(radius); // setRadius é método no OrbitParent
            orbitComp.SetSpeed(currentOrbitSpeed); // aplica a velocidade atual do slider
            planetaryOrbits.Add(orbitComp);        // regista para atualização suave do raio
        }

        else
        {
          body.transform.localPosition = new Vector3(radius, 0f, 0f);  
        }

        spawnedObjects.Add(body);
        return body;
    }

    private void SpawnMoon(Transform parentBody)
    {
        if (moonPrefab == null || parentBody == null) return;

        GameObject moon = Instantiate(moonPrefab, parentBody);

        // garante que a Lua é sempre mais pequena que o planeta pai (ex: 30% do tamanho do planeta)
        moon.transform.localScale = Vector3.one * 0.3f;

        // define a distância (raio) da Lua ao planeta
        // (Se o planeta já tiver mais luas, afasta ligeiramente a segunda para não colidirem)
        int existingMoons = parentBody.childCount - 1; 
        float moonRadius = 1.8f + (existingMoons * 0.8f);

        // configura o OrbitParent se existir no Prefab da Lua, caso contrário posiciona manualmente
        OrbitParent orbitComp = moon.GetComponent<OrbitParent>();
        if (orbitComp != null)
        {
            orbitComp.SetRadius(moonRadius);
            orbitComp.SetSpeed(currentOrbitSpeed); // aplica a velocidade atual do slider
        }
        else
        {
            moon.transform.localPosition = new Vector3(moonRadius, 0f, 0f);
        }

        spawnedObjects.Add(moon);
    }

    private int CountPlanetaryOrbits(List<Symbol> word)
    {
        int count = 0;
        foreach (Symbol s in word)
        {
            if (s.Id == "D" || s.Id == "M" || s.Id == "N" || s.Id == "P")
            {
                count++;
            }
        }
        return count;
    }

    #endregion
}