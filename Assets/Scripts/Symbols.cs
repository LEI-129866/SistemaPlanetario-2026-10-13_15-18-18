using System;

[Serializable] public class Symbol // indica ao compilador a possibilidade de armazenar/transmitir um determinado dado
{
    public string Id { get; set; }           // símbolo de 2 carateres: ex. "Nv", "Eb", "Pc", "ff", "[_", "]"
    public int Parameter { get; set; }       // 0 na Fase 2
    
    // parâmetros orbitais (para Pc, Pb, Pa, La)
    public float RotationSpeed { get; set; } // alpha: velocidade angular de rotação (rad/s)
    public float OrbitSpeed { get; set; }    // beta: velocidade angular de órbita (rad/s)
    
    // identificador único para evolução do corpo (ex: Pc -> Pb -> Pa) para preservar a fase orbital
    public int BodyId { get; set; }

    private static int nextBodyId = 1;

    public Symbol(string id, int parameter = 0, float rotationSpeed = 0f, float orbitSpeed = 0f, int bodyId = -1)
    {
        Id = id;
        Parameter = parameter;
        RotationSpeed = rotationSpeed;
        OrbitSpeed = orbitSpeed;
        BodyId = (bodyId >= 0) ? bodyId : nextBodyId++;
    }

    // cria uma cópia exata do símbolo mantendo o mesmo BodyId
    public Symbol Clone()
    {
        return new Symbol(Id, Parameter, RotationSpeed, OrbitSpeed, BodyId);
    }

    public override string ToString()
    {
        if (Parameter > 0)
            return $"{Id}{Parameter}";
        return Id;
    }
}

// -- Código Fase 1 -- //

// [Serializable] public class Symbol
// {
//     public string Id { get; set; }       // ex: "n", "p", "d", "D", "P", "[", "]"
//     public int Parameter { get; set; }   // ex: 3 (para d3), ou 0 se não tiver parâmetro

//     public Symbol(string id, int parameter = 0)
//     {
//         Id = id;
//         Parameter = parameter;
//     }

//     // visualização na consola do Unity
//     public override string ToString()
//     {
//         if (Parameter > 0)
//             return $"{Id}{Parameter}";
//         return Id;
//     }
// }

