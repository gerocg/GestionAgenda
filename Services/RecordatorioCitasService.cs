using GestionAgenda.Context;
using GestionAgenda.Enums;
using GestionAgenda.Services;
using Microsoft.EntityFrameworkCore;
using System;

public class RecordatorioCitasService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecordatorioCitasService> _logger;

    public RecordatorioCitasService(
        IServiceScopeFactory scopeFactory,
        ILogger<RecordatorioCitasService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarRecordatorios();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios de citas");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcesarRecordatorios()
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ContextBd>();
        var emailService = scope.ServiceProvider.GetRequiredService<CitasEmailService>();

        var ahora = DateTime.Now;

        var citas = await context.Citas.Include(c => c.Paciente).ThenInclude(p => p.Usuario).Where(c => c.Estado == EstadoCita.Confirmada && c.FechaAgendada > ahora).ToListAsync();

        foreach (var cita in citas)
        {
            var diferencia = cita.FechaAgendada - ahora;

            if (diferencia.TotalHours <= 2 && diferencia.TotalHours > 0 && !cita.Recordatorio2hEnviado)
            {
                await emailService.EnviarRecordatorio(cita, cita.Paciente.Usuario.Email, cita.Paciente.Usuario.NombreCompleto);
                cita.Recordatorio2hEnviado = true;
            }
            else if (diferencia.TotalHours <= 24 && diferencia.TotalHours > 2  && !cita.Recordatorio24hEnviado)
            {
                await emailService.EnviarRecordatorio(cita, cita.Paciente.Usuario.Email, cita.Paciente.Usuario.NombreCompleto);
                cita.Recordatorio24hEnviado = true;
            } 
        }

        await context.SaveChangesAsync();
    }
}