using System.Security.Claims;
using Chat.Server.Data;
using Chat.Server.Data.Entity;

namespace Chat.Server.GraphQL;

public class Query
{
    [UseProjection]
    [UseFiltering]
    public IQueryable<User> GetUsers([Service] ApplicationDbContext dbContext)
    {
        return dbContext.Users;
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<User> GetUser([Service] ApplicationDbContext dbContext, ClaimsPrincipal principal, string id)
    {
        // Users can only query search their own resources
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (principalId != id)
            throw new ArgumentException("User " + id + " is not the current user");
        return dbContext.Users.Where(e => e.Id == id);
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<DirectChat> GetDirectChat([Service] ApplicationDbContext dbContext, ClaimsPrincipal principal, int id)
    {
        // Users can only query the chat if they are a member of it
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var chat = dbContext.DirectChats.FirstOrDefault(e => e.Id == id && (e.User1Id == principalId || e.User2Id == principalId));
        if (chat == null)
            throw new ArgumentException("User " + principalId + " is not a member of chat " + id);
        return dbContext.DirectChats.Where(e => e.Id == id);
    }

    [UseFirstOrDefault]
    [UseProjection]
    [UseFiltering]
    public IQueryable<Space> GetSpace(ApplicationDbContext dbContext, ClaimsPrincipal principal, int id)
    {
        // Users can only query the chat if they are a member of it
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var space = dbContext.SpaceMemberships.FirstOrDefault(e => e.SpaceId == id && e.MemberId == principalId);
        if (space == null)
            throw new ArgumentException("User " + principalId + " is not a member of space " + id);
        return dbContext.Spaces.Where(e => e.Id == id);
    }

    [UsePaging(IncludeTotalCount = true, MaxPageSize = 30)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SpaceMessage> GetSpaceMessages([Service] ApplicationDbContext dbContext, ClaimsPrincipal principal, int spaceId)
    {
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var space = dbContext.SpaceMemberships.FirstOrDefault(e => e.SpaceId == spaceId && e.MemberId == principalId);
        if (space == null)
            throw new ArgumentException("User " + principalId + " is not a member of space " + spaceId);

        return dbContext.SpaceMessages.Where(e => e.SpaceId == spaceId);
    }

    [UsePaging(IncludeTotalCount = true, MaxPageSize = 30)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<DirectMessage> GetDirectMessages([Service] ApplicationDbContext dbContext, ClaimsPrincipal principal, int chatId)
    {
        var principalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var chat = dbContext.DirectChats.FirstOrDefault(e => e.Id == chatId && (e.User1Id == principalId || e.User2Id == principalId));
        if (chat == null)
            throw new ArgumentException("User " + principalId + " is not a member of chat " + chatId);
        return dbContext.DirectMessages.Where(e => e.DirectChatId == chatId);
    }
}