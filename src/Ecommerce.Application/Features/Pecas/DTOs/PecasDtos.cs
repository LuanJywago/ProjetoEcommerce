using System;

namespace Ecommerce.Application.Features.Pecas.DTOs
{
    // Usado quando vamos CRIAR uma nova peça (Frontend -> Backend)
    public class CriarPecaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
    }

    // Usado quando vamos ATUALIZAR uma peça (Frontend -> Backend)
    public class AtualizarPecaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
    }

    // Usado quando vamos MOSTRAR a peça na lista (Backend -> Frontend)
    public class PecaResponseDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}