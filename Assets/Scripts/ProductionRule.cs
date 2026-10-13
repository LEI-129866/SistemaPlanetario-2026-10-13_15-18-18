using System.Collections.Generic;

public class ProductionRule
{
    public string PredecessorId { get; set; }
    public int? PredecessorParam { get; set; } // Null se aplicar a qualquer valor ou sem parâmetro
    public List<Symbol> Successor { get; set; }

    public ProductionRule (string predecessorId, int? predecessorParam, List<Symbol> successor)
    {
        PredecessorId = predecessorId;
        PredecessorParam = predecessorParam;
        Successor = successor;
    }

    // Verifica se a regra se aplica a um dado Símbolo
    public bool Matches(Symbol symbol)
    {
        if (symbol.Id != PredecessorId) return false;
        if (PredecessorParam.HasValue && symbol.Parameter != PredecessorParam.Value) return false;
        return true;
    }
}
