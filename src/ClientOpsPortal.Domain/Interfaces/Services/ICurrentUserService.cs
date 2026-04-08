using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Domain.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}
