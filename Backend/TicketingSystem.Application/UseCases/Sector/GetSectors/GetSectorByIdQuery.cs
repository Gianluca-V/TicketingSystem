using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.GetSectors
{
    public class GetSectorByIdQuery : IQuery<SectorDto?>
    {
        public int EventId { get; set; }
        public int SectorId { get; set; }
    }
}
