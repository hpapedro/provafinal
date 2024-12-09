namespace provasub.Models;

    public class Aluno
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public DateOnly DataNasimento { get; set; }
    }