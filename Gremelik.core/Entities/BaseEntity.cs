
using System;

namespace Gremelik.core.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public required string Usuario { get; set; }
        public DateTime FUM { get; set; }
    }
}
