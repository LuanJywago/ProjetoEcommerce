using System;

namespace Ecommerce.Domain.Entities
{
    public class RegistroPonto
    {
        public Guid Id { get; set; }
        
        // Chave estrangeira para o Usuário (Funcionário)
        public Guid FuncionarioId { get; set; } 
        public Usuario? Funcionario { get; set; } // Propriedade de navegação

        public DateTime DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; } // '?' permite valor nulo (o funcionário ainda não saiu)
    }
}