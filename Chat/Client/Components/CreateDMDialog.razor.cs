using System.Security.Claims;
using Chat.Client.Models;
using Chat.Client.State;
using Chat.Client.GraphQL;
using Chat.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using StrawberryShake;

namespace Chat.Client.Components;

public partial class CreateDMDialog : ComponentBase
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [CascadingParameter]
    Task<AuthenticationState>? authenticationState { get; set; }

    [Inject]
    IGraphQLClient GraphQLClient { get; set; } = null!;
    [Inject] StateContainer StateContainer { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;

    string? _userId;
    List<UserDto> _friends = new();
    string? _error;

    void Cancel() => MudDialog.Cancel();

    protected override async Task OnInitializedAsync()
    {
        if (authenticationState != null)
        {
            var authState = await authenticationState;
            var user = authState?.User;

            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst("sub")?.Value;
                _userId = userId;
                try
                {
                    var result = await GraphQLClient.GetFriends.ExecuteAsync(userId);
                    _friends = result.Data!.Users.Select(x => new UserDto
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        UserName = x.UserName!,
                        Avatar = x.Avatar
                    }).ToList();
                }
                catch (Exception ex)
                {
                    _error = ex.Message;
                }
            }
        }
    }

    async Task CreateDirectChat(string friendId)
    {
        if (_userId != null)
        {
            try
            {
                var result = await GraphQLClient.AddDirectChat.ExecuteAsync(_userId, friendId);
                result.EnsureNoErrors();
                var directChat = result.Data!.AddDirectChat.DirectChat!;
                var user1 = directChat.User1;
                var user2 = directChat.User2;
                var contact = new Contact
                {
                    Id = directChat.Id,
                    Name = user1.Id == _userId ? user2.FirstName + " " + user2.LastName : user1.FirstName + " " + user1.LastName,
                    Image = user1.Id == _userId ? user2.Avatar : user1.Avatar,
                    Type = ChatType.Direct,
                    Time = directChat.Updated.UtcDateTime
                };
                StateContainer.AddContact(contact);
                MudDialog.Close(DialogResult.Ok(true));
                NavigationManager.NavigateTo($"/chat/dm/{contact.Id}");
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }
    }
}