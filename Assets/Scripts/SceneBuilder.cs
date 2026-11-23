using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder : MonoBehaviour
{

    [Header("Referências")]
    [SerializeField] private TurtleInterpreter turtleInterpreter;
    [SerializeField] private GameObject systemOrigin;

    [Header("Prefabs dos Elementos Visuais (Fase 2)")]
    [SerializeField] private GameObject cloudPrefab;         // Nv: Nuvem difusa
    [SerializeField] private GameObject protoStarPrefab;     // Eb: Proto-estrela
    [SerializeField] private GameObject starPrefab;          // Ea: Estrela madura
    [SerializeField] private GameObject gasRingPrefab;       // G1: Anel de gás (largura dg)
    [SerializeField] private GameObject rockRingPrefab;      // R1: Anel de rochas (largura dr)
    [SerializeField] private GameObject planetesimalPrefab;  // Pc: Planetesimal
    [SerializeField] private GameObject protoPlanetPrefab;   // Pb: Proto-planeta
    [SerializeField] private GameObject planetPrefab;        // Pa: Planeta formado
    [SerializeField] private GameObject moonPrefab;          // La: Lua

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Dictionary<int, float> savedOrbitalPhases = new Dictionary<int, float>();
    private System.Random phaseRng;
    private bool useRandomInitialPhase = true;

    // Chamado pelo UIManager/LSystemManager sempre que um novo passo é gerado (Fase 2)
    // Chamado pelo LSystemTester sempre que um novo passo é gerado (Fase 2)
    public void BuildScene(List<Symbol> word, SimulationConfig config, int orbitPhaseSeed = 123, bool useRandomInitialPhase = true)
    {
        SaveCurrentPhases();
        ClearScene();

        if (word == null || word.Count == 0 || config == null) return;

        // Guarda na classe para o SpawnOrbitingBody usar
        this.phaseRng = new System.Random(orbitPhaseSeed);
        this.useRandomInitialPhase = useRandomInitialPhase;

        Transform rootFrame = systemOrigin != null ? systemOrigin.transform : transform;

        if (turtleInterpreter == null)
            turtleInterpreter = GetComponent<TurtleInterpreter>();

        if (turtleInterpreter != null)
        {
            turtleInterpreter.Interpret(word, config, rootFrame, this);
        }
    }

    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj != null && !spawnedObjects.Contains(obj))
            spawnedObjects.Add(obj);
    }

    // Chamado pelo TurtleInterpreter sempre que encontra Nv, Eb, Ea, G1, Pc, Pb, Pa, R1, La
    public Transform SpawnStellarObject(
        Symbol symbol,
        SimulationConfig config,
        Transform currentFrame,
        Vector3 turtleLocalPos,
        Quaternion turtleLocalRot)
    {
        switch (symbol.Id)
        {
            case "Nv":
                SpawnCloud(currentFrame, turtleLocalPos, config.starRadius * 10f);
                return null;

            case "Eb":
                return SpawnStar(protoStarPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.starRadius, config.starMass);

            case "Ea":
                return SpawnStar(starPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.starRadius, config.starMass);

            case "G1":
                return SpawnRing("GasRing_G1", gasRingPrefab, currentFrame, turtleLocalPos, turtleLocalRot, config.gasRingWidth);

            case "R1":
                return SpawnRing("RockRing_R1", rockRingPrefab, currentFrame, turtleLocalPos, turtleLocalRot, config.rockRingWidth);

            case "Pc":
                return SpawnOrbitingBody(planetesimalPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.planetRadius, config.planetMass);

            case "Pb":
                return SpawnOrbitingBody(protoPlanetPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.planetRadius, config.planetMass);

            case "Pa":
                return SpawnOrbitingBody(planetPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.planetRadius, config.planetMass);

            case "La":
                return SpawnOrbitingBody(moonPrefab, symbol, currentFrame, turtleLocalPos, turtleLocalRot, config.moonRadius, config.moonMass);

            default:
                return null;
        }
    }

    private Transform SpawnStar(GameObject prefab, Symbol symbol, Transform parent, Vector3 localPos, Quaternion localRot, float radius, float mass)
    {
        if (prefab == null) return null;

        GameObject star = Instantiate(prefab, parent);
        star.transform.localPosition = localPos;
        star.transform.localRotation = localRot;
        star.transform.localScale = Vector3.one * (2f * radius);

        AttachCelestialBody(star, symbol.Id, symbol.BodyId, mass);
        RegisterSpawnedObject(star);
        return star.transform;
    }

    private void SpawnCloud(Transform parent, Vector3 localPos, float radius)
    {
        if (cloudPrefab == null) return;

        GameObject cloud = Instantiate(cloudPrefab, parent);
        cloud.transform.localPosition = localPos;

        ParticleSystem ps = cloud.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var shape = ps.shape;
            shape.radius = Mathf.Max(radius, 5f);
        }

        RegisterSpawnedObject(cloud);
    }

    private Transform SpawnRing(string ringName, GameObject materialSourcePrefab, Transform parent, Vector3 turtleLocalPos, Quaternion turtleLocalRot, float ringWidth)
    {
        float innerRadius = turtleLocalPos.magnitude;
        float outerRadius = innerRadius + ringWidth;

        GameObject ring = new GameObject(ringName);
        ring.transform.SetParent(parent, worldPositionStays: false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localRotation = turtleLocalRot;

        MeshFilter meshFilter = ring.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = ring.AddComponent<MeshRenderer>();

        if (materialSourcePrefab != null)
        {
            MeshRenderer sourceRenderer = materialSourcePrefab.GetComponentInChildren<MeshRenderer>();
            if (sourceRenderer != null)
            {
                meshRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
            }
        }

        meshFilter.mesh = BuildAnnulusMesh(innerRadius, outerRadius, 72);

        RegisterSpawnedObject(ring);
        return ring.transform;
    }

    private Mesh BuildAnnulusMesh(float innerRadius, float outerRadius, int segments)
    {
        Mesh mesh = new Mesh();
        int vertCountPerSide = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCountPerSide * 2];
        Vector3[] normals = new Vector3[vertCountPerSide * 2];
        Vector2[] uvs = new Vector2[vertCountPerSide * 2];
        int[] triangles = new int[segments * 12];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = t * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 innerPos = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            Vector3 outerPos = new Vector3(cos * outerRadius, 0f, sin * outerRadius);

            // Face superior (+Y)
            int topIdx = i * 2;
            vertices[topIdx] = innerPos;
            vertices[topIdx + 1] = outerPos;
            normals[topIdx] = Vector3.up;
            normals[topIdx + 1] = Vector3.up;
            uvs[topIdx] = new Vector2(0f, t);
            uvs[topIdx + 1] = new Vector2(1f, t);

            // Face inferior (-Y)
            int botIdx = vertCountPerSide + i * 2;
            vertices[botIdx] = innerPos;
            vertices[botIdx + 1] = outerPos;
            normals[botIdx] = Vector3.down;
            normals[botIdx + 1] = Vector3.down;
            uvs[botIdx] = new Vector2(0f, t);
            uvs[botIdx + 1] = new Vector2(1f, t);
        }

        int triIdx = 0;
        for (int i = 0; i < segments; i++)
        {
            int t0 = i * 2;
            int t1 = t0 + 1;
            int t2 = t0 + 2;
            int t3 = t0 + 3;

            triangles[triIdx++] = t0;
            triangles[triIdx++] = t2;
            triangles[triIdx++] = t1;

            triangles[triIdx++] = t1;
            triangles[triIdx++] = t2;
            triangles[triIdx++] = t3;

            int b0 = vertCountPerSide + i * 2;
            int b1 = b0 + 1;
            int b2 = b0 + 2;
            int b3 = b0 + 3;

            triangles[triIdx++] = b0;
            triangles[triIdx++] = b1;
            triangles[triIdx++] = b2;

            triangles[triIdx++] = b1;
            triangles[triIdx++] = b3;
            triangles[triIdx++] = b2;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private Transform SpawnOrbitingBody(
        GameObject prefab,
        Symbol symbol,
        Transform parent,
        Vector3 turtleLocalPos,
        Quaternion turtleLocalRot,
        float radius,
        float mass)
    {
        if (prefab == null) return null;

        // Preserva a fase se este corpo já existia no passo anterior (BodyId),
        // caso contrário usa a seed aleatória (se ativa) ou a posição base (NaN)
        float phaseToUse = float.NaN;
        if (symbol.BodyId >= 0 && savedOrbitalPhases.TryGetValue(symbol.BodyId, out float previousPhase))
        {
            phaseToUse = previousPhase;
        }
        else if (useRandomInitialPhase && phaseRng != null)
        {
            phaseToUse = (float)(phaseRng.NextDouble() * 2.0 * Mathf.PI);
        }

        GameObject body = Instantiate(prefab, parent);
        body.transform.localPosition = turtleLocalPos;
        body.transform.localRotation = turtleLocalRot;
        body.transform.localScale = Vector3.one * (2f * radius);

        SelfRotationScript selfRot = body.GetComponentInChildren<SelfRotationScript>();
        if (selfRot != null)
        {
            selfRot.SetRotationSpeed(symbol.RotationSpeed * Mathf.Rad2Deg);
        }

        OrbitParent orbitComp = body.GetComponent<OrbitParent>();
        if (orbitComp == null)
            orbitComp = body.AddComponent<OrbitParent>();

        orbitComp.InitOrbit(turtleLocalPos, symbol.OrbitSpeed, phaseToUse);

        AttachCelestialBody(body, symbol.Id, symbol.BodyId, mass);
        RegisterSpawnedObject(body);
        return body.transform;
    }

    private void AttachCelestialBody(GameObject obj, string symbolId, int bodyId, float mass)
    {
        CelestialBody cb = obj.GetComponent<CelestialBody>();
        if (cb == null)
            cb = obj.AddComponent<CelestialBody>();

        cb.SymbolId = symbolId;
        cb.BodyId = bodyId;
        cb.Mass = mass;
    }

    private void SaveCurrentPhases()
    {
        savedOrbitalPhases.Clear();

        foreach (GameObject obj in spawnedObjects)
        {
            if (obj == null) continue;

            CelestialBody cb = obj.GetComponent<CelestialBody>();
            OrbitParent orbit = obj.GetComponent<OrbitParent>();

            if (cb != null && orbit != null && cb.BodyId >= 0)
            {
                savedOrbitalPhases[cb.BodyId] = orbit.GetCurrentPhase();
            }
        }
    }

    public void ClearScene()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    // Métodos mantidos para compatibilidade caso a UI antiga ainda os chame
    public void SetOrbitRadiusStep(float newStep) { }
    public void SetOrbitSpeed(float newSpeed) { }


    /*
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
                    orbit.SetSpeed(isMoon ? currentOrbitSpeed * 2f : currentOrbitSpeed);
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
    private GameObject SpawnOrbitingBody(GameObject prefab, Transform parent, float radius, bool randomizeInitialState = false)
    {
        if (prefab == null) return parent.gameObject;

        //orbitComp.randomize = true;

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

        // garante que a Lua é sempre mais pequena que o planeta pai (30% do tamanho do planeta)
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
            orbitComp.SetSpeed(currentOrbitSpeed * 2f); // aplica a velocidade atual do slider
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
    */
}

// Componente leve anexado a cada astro para guardar a massa (gravidade) e o BodyId (preservação de fase)
public class CelestialBody : MonoBehaviour
{
    public string SymbolId;
    public int BodyId = -1;
    public float Mass = 0f;
}