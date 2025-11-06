using System;

namespace Ecommerce.Domain.Entities
{
    public class Peca
    {
        public Guid Id { get; set; } // Atende RQ05 e RN01 (ID único)
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        
        // Usamos 'decimal' para dinheiro, nunca 'double' ou 'float'
        public decimal Preco { get; set; } // Atende RN03 (Preço > 0, vamos validar na Application)
        
        public int QuantidadeEstoque { get; set; } // Atende RN02 (Qtde >= 0)
    }
}