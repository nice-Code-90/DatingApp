using System;

namespace DatingApp.Application.Helpers;


public class LikesParams : PagingParams
{
    public string MemberId { get; set; } = "";
    public string Predicate { get; set; } = "liked";
}
