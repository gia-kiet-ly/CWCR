using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Infrastructure
{
    public interface IJwtTokenGenerator
    {
        (string token, DateTime expiredAt) GenerateToken(Guid userId, string email, string role);
    }
}
