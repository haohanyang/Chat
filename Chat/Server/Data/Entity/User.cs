using Chat.Shared.Dto;
using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Data.Entity;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    public ICollection<DirectChat> DirectChats1
    { get; set; } = new HashSet<DirectChat>();
    public ICollection<DirectChat> DirectChats2 { get; set; } = new HashSet<DirectChat>();
    public ICollection<SpaceMembership> SpaceMemberships { get; set; } = new HashSet<SpaceMembership>();
    public ICollection<SpaceMessage> SpaceMessages { get; set; } = new HashSet<SpaceMessage>();
    public ICollection<DirectMessage> DirectMessages { get; set; } = new HashSet<DirectMessage>();
    public ICollection<DirectMessageAttachment> DirectMessageAttachments { get; set; } = new HashSet<DirectMessageAttachment>();
    public ICollection<SpaceMessageAttachment> SpaceMessageAttachments { get; set; } = new HashSet<SpaceMessageAttachment>();

    public UserDto ToDto() => new()
    {
        Id = Id,
        FirstName = FirstName,
        LastName = LastName,
        Avatar = Avatar,
        UserName = UserName!,
        Bio = Bio
    };
}