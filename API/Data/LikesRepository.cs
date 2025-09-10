using System;
using API.Entities;
using API.Interfaces;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        throw new NotImplementedException();
    }

    public void DeleteLike(MemberLike like)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<MemberLike> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Member>> GetMemberLikes(string predicate, string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveAllChanges()
    {
        throw new NotImplementedException();
    }
}
