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
    
    // guardar as referências dos objetos instanciados para serem limpos depois
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // guarda os corpos que orbitam diretamente a estrela para ajustar raios em tempo real
    private List<OrbitParent> planetaryOrbits = new List<OrbitParent>();

    // limpa a cena anterior e constrói a nova cena 3D com base na palavra atual do L-System.
    public void BuildScene(List<Symbol> word)
    {
        ClearScene();

        if (word == null || word.Count == 0) return;

        // usa a origem definida na cena ou o próprio Transform deste GameObject como raiz
        Transform parentTransform = systemOrigin != null ? systemOrigin.transform : transform;

        // pilha LIFO para gerir ramificações ('[' e ']')
        Stack<Transform> branchStack = new Stack<Transform>();

        // índice acumulado para calcular o raio das órbitas
        int planetOrbitIndex = 1;

        // referência ao último corpo planetário criado no nível principal
        Transform currentBodyParent = parentTransform;

        // calcular o raio máximo para ajustar a dimensão da Nuvem 'n'
        int totalPlanets = CountPlanetaryOrbits(word);
        float maxRadius = totalPlanets * orbitRadiusStep;

        // instanciar e posicionar os objetos na cena
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
                    // Início de Ramo: guarda o corpo atual como pai dos elementos seguintes
                    branchStack.Push(currentBodyParent);
                    break;

                case "]":
                    // Fim de Ramo: restaura o contexto anterior
                    if (branchStack.Count > 0)
                    {
                        branchStack.Pop();
                    }
                    break;
            }
        }
    }

    // destrói todos os GameObjects criados no passo anterior e limpa as listas
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

    // atualiza o raio das órbitas existentes em tempo real sem reiniciar a animação
    public void SetOrbitRadiusStep(float newStep)
    {
        orbitRadiusStep = newStep;

        for (int i = 0; i < planetaryOrbits.Count; i++)
        {
            if (planetaryOrbits[i] != null)
            {
                planetaryOrbits[i].SetRadius((i + 1) * orbitRadiusStep);
            }
        }
    }

    // atualiza a velocidade orbital de todos os corpos (mantendo as luas 3x mais rápidas que os planetas)
    public void SetOrbitSpeed(float newSpeed)
    {
        currentOrbitSpeed = newSpeed;
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                OrbitParent orbit = obj.GetComponentInChildren<OrbitParent>();
                if (orbit != null)
                {
                    bool isMoon = obj.transform.parent != null && obj.transform.parent != (systemOrigin != null ? systemOrigin.transform : transform);
                    orbit.SetSpeed(isMoon ? currentOrbitSpeed * 3f : currentOrbitSpeed);
                }
            }
        }
    }

    #region Métodos Auxiliares de Instanciação

    // instancia um objeto estático na posição local indicada (usado para estrelas)
    private GameObject SpawnObject(GameObject prefab, Transform parent, Vector3 localPosition)
    {
        if (prefab == null) return parent.gameObject;

        GameObject instance = Instantiate(prefab, parent);
        instance.transform.localPosition = localPosition;
        spawnedObjects.Add(instance);
        return instance;
    }

    // instancia a nuvem de gás/poeira e ajusta o raio do sistema de partículas ao tamanho do sistema
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

        spawnedObjects.Add(cloud);
    }

    // instancia o disco protoplanetário e ajusta a sua escala conforme o parâmetro (d3 -> d2 -> d1)
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

    // instancia o anel planetário (D)
    private GameObject SpawnRing(Transform parent, float radius)
    {
        if (ringPrefab == null) return parent.gameObject;

        GameObject ring = Instantiate(ringPrefab, parent);
        spawnedObjects.Add(ring);
        return ring;
    }

    // instancia um corpo em órbita direta da estrela (M, N ou P) e configura o seu OrbitParent
    private GameObject SpawnOrbitingBody(GameObject prefab, Transform parent, float radius)
    {
        if (prefab == null) return parent.gameObject;

        GameObject body = Instantiate(prefab, parent);

        OrbitParent orbitComp = body.GetComponent<OrbitParent>();
        if (orbitComp != null){
            orbitComp.SetRadius(radius); 
            orbitComp.SetSpeed(currentOrbitSpeed); // aplica a velocidade atual do slider
            planetaryOrbits.Add(orbitComp); // registo para atualização suave do raio
        }

        else
        {
          body.transform.localPosition = new Vector3(radius, 0f, 0f);  
        }

        spawnedObjects.Add(body);
        return body;
    }

    // instancia uma lua (1) como filha do planeta pai e configura uma órbita própria mais rápida
    private void SpawnMoon(Transform parentBody)
    {
        if (moonPrefab == null || parentBody == null) return;

        GameObject moon = Instantiate(moonPrefab, parentBody);

        // garante que a Lua é sempre mais pequena que o planeta pai (ex: 30% do tamanho do planeta)
        moon.transform.localScale = Vector3.one * 0.3f;

        // define a distância (raio) da Lua ao planeta
        // (se o planeta já tiver mais luas, afasta ligeiramente a segunda para não colidirem)
        int existingMoons = parentBody.childCount - 1; 
        float moonRadius = 1.8f + (existingMoons * 0.8f);

        // configura o OrbitParent se existir no Prefab da Lua, caso contrário posiciona manualmente
        OrbitParent orbitComp = moon.GetComponentInChildren<OrbitParent>();
        if (orbitComp != null)
        {
            orbitComp.SetRadius(moonRadius);
            orbitComp.SetSpeed(currentOrbitSpeed * 3f); // aplica a velocidade atual do slider
        }
        else
        {
            moon.transform.localPosition = new Vector3(moonRadius, 0f, 0f);
        }

        spawnedObjects.Add(moon);
    }

    // conta quantos elementos ocupam órbitas na palavra atual
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