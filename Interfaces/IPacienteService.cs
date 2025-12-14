using GestionAgenda.DTOs;
using GestionAgenda.Modelo;

namespace GestionAgenda.Interfaces
{
    public interface IPacienteService
    {
        Paciente GetById(int id);
        void Update(int id, PacienteDTO dto);
    }
}
