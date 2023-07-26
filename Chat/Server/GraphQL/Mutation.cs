using System.Security.Claims;
using Chat.Server.Data;
using Chat.Server.Data.Entity;
using Chat.Shared.Request;

namespace Chat.Server.GraphQL;

public class Mutation
{
    public async Task<DirectChat> AddDirectChat([Service] ApplicationDbContext dbContext, CancellationToken cancellationToken, ClaimsPrincipal principal, DirectChatRequest request)
    {
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principalId != request.User1Id && principalId != request.User2Id)
            throw new ArgumentException("Invalid user id");

        var compare = request.User1Id.CompareTo(request.User2Id);
        var user1Id = compare < 0 ? request.User1Id : request.User2Id;
        var user2Id = compare < 0 ? request.User2Id : request.User1Id;

        var user1 = await dbContext.Users.FindAsync(user1Id, cancellationToken);
        if (user1 == null)
            throw new Exception("User " + user1Id + " not found");

        var user2 = await dbContext.Users.FindAsync(user2Id, cancellationToken);
        if (user2 == null)
            throw new Exception("User " + user2Id + " not found");

        var existing = dbContext.DirectChats.FirstOrDefault(e => e.User1 == user1 && e.User2 == user2);
        if (existing != null)
            return existing;

        var directChat = new DirectChat
        {
            User1 = user1,
            User2 = user2
        };

        await dbContext.DirectChats.AddAsync(directChat, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return directChat;
    }

    public async Task<Space> AddSpace([Service] ApplicationDbContext dbContext, CancellationToken cancellationToken, ClaimsPrincipal principal, SpaceRequest request)
    {
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!request.Members.Contains(principalId))
            throw new ArgumentException("Invalid user id");

        var space = new Space
        {
            Name = request.Name,
            Description = request.Description,
        };

        var members = new List<SpaceMembership>();
        foreach (var userId in request.Members)
        {
            var user = await dbContext.Users.FindAsync(userId, cancellationToken);
            if (user == null)
                throw new Exception("User " + userId + " not found");

            members.Add(new SpaceMembership
            {
                Member = user,
                Space = space
            });
        }

        await dbContext.Spaces.AddAsync(space, cancellationToken);
        await dbContext.SpaceMemberships.AddRangeAsync(members, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return space;
    }
}

