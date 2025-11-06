using System;

namespace Ecommerce.Application.Features.Ponto.DTOs
{
    // DTO para mostrar o resultado de um registo de ponto
    public class RegistroPontoDto
    {
        public Guid Id { get; set; }
        public Guid FuncionarioId { get; set; }
        public DateTime DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; } // Pode ser nulo se ainda não saiu
        public string Status { get; set; } = string.Empty; // Ex: "Entrada registrada", "Saída registrada"
    }
}