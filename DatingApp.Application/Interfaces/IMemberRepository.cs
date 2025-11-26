using System;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<Member?> GetMemberByIdAsync(string id);
    IQueryable<Member> GetMembersAsQueryable();
    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser);
    Task<Member?> GetMemberForUpdate(string id);
}