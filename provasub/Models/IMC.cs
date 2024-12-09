using provasub.Models;

public class IMC
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
    public double Altura { get; set; }
    public double Peso { get; set; }
    public string? Classificacao { get; set; }
    public double ImcCalculado { get; set; } // Adiciona a propriedade para armazenar o valor do IMC calculado
}
