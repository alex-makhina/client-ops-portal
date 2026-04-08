using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class User
    {
        public string Id { get; set; } = null!;

        public Abonent? Abonent { get; set; }
        public Employee? Employee { get; set; }
    }
}
