using System;

[Serializable] // indica ao compilador a possibilidade de armazenar/transmitir um determinado dado
public class Symbol
{
    public string Id { get; set; }       // ex: "n", "p", "d", "D", "P", "[", "]"
    public int Parameter { get; set; }   // ex: 3 (para d3), ou 0 se não tiver parâmetro

    public Symbol(string id, int parameter = 0)
    {
        Id = id;
        Parameter = parameter;
    }

    // visualização na Consola do Unity
    public override string ToString()
    {
        if (Parameter > 0)
            return $"{Id}{Parameter}";
        return Id;
    }
}

