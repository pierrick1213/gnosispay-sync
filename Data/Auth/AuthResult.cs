using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Data.Auth
{
    public sealed record AuthResult(string Jwt, DateTime ExpiresAt);
}
