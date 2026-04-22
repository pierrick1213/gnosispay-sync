using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Data.Auth
{
    public sealed record ChallengeRequest(
    string Message,
    string Signature,
    int TtlInSeconds);
}
