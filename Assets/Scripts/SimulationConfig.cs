using System.Collections.Generic;

// guarda todos os parâmetros físicos, geométricos e gramaticais lidos do ficheiro de configuração (Fase 2)
public class SimulationConfig
{
    // Linha 1: deslocamento (c) em metros e ângulo de rotação (a) em radianos da tartaruga
    public float stepDistance;
    public float rotationAngle;

    // Linha 2: massas da estrela (me), planetas (mp) e luas (ml) em kg
    public int starMass;
    public int planetMass;
    public int moonMass;

    // Linha 3: massa da nave espacial em kg
    public float spaceshipMass;

    // Linha 4: raios das esferas da estrela (re), planetas (rp) e luas (rl) em metros
    public float starRadius;
    public float planetRadius;
    public float moonRadius;

    // Linha 5: larguras radiais dos anéis de gás (dg -> G1) e de rochas (dr -> R1) em metros
    public float gasRingWidth;
    public float rockRingWidth;

    // Linha 6: constante da gravitação universal (G)
    public float gravitationalConstant;

    // Linha 7: axioma inicial do L-System (ex: "Nv")
    public List<Symbol> axiom = new List<Symbol>();

    // Linha 8 e seguintes: lista de produções estocásticas
    public List<StochasticRule> rules = new List<StochasticRule>();
}

// produção estocástica: <peso> <Cabeça> -> <Corpo>
public class StochasticRule
{
    public int weight;                                         // peso inteiro positivo
    public string predecessor;                                 // símbolo cabeça (ex: "G1", "R1")
    public List<Symbol> successor = new List<Symbol>();        // símbolos do corpo da produção
    public List<OrbitParams> orbitParameters = new List<OrbitParams>(); // pares (alpha, beta) para cada corpo orbital no corpo

    public StochasticRule(int weight, string predecessor, List<Symbol> successor)
    {
        this.weight = weight;
        this.predecessor = predecessor;
        this.successor = successor;
    }
}

// velocidades angulares (em rad/s) de um corpo com órbita (Pc, Pb, Pa, La)
public struct OrbitParams
{
    public float rotationSpeed; // alpha: rotação em torno do vetor u da tartaruga
    public float orbitSpeed;    // beta: órbita em torno da origem do referencial atual

    public OrbitParams(float alpha, float beta)
    {
        this.rotationSpeed = alpha;
        this.orbitSpeed = beta;
    }
}