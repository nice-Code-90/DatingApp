using System;

namespace API.Helpers;

public class MemberParams : PagingParams
{
    public string? Gender { get; set; }
    public string? CurrentMemberId { get; set; }
}
