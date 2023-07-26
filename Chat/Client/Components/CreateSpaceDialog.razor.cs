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

public partial class CreateSpaceDialog : ComponentBase
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }

    [Inject]
    GraphQLClient GraphQLClient { get; set; } = null!;

    [Inject]
    StateContainer StateContainer { get; set; } = null!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = null!;

    MudForm _form;
    bool _isValid = false;
    bool _isRequesting = false;
    string _name = string.Empty;
    string _description = string.Empty;
    string? _userId;
    string? error;

    IEnumerable<string> _members = new HashSet<string>();
    List<UserDto> _friends = new();

    void Cancel()
    {
        MudDialog.Cancel();
    }

    protected override async Task OnInitializedAsync()
    {
        if (authenticationState != null)
        {
            var authState = await authenticationState;
            var user = authState?.User;

            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst("sub")!.Value;
                _userId = userId;
                try
                {
                    var result = await GraphQLClient.GetFriends.ExecuteAsync(userId);
                    result.EnsureNoErrors();
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
                    error = ex.Message;
                }
            }
        }
    }

    async Task CreateSpace()
    {
        if (_isValid && _userId != null)
        {
            try
            {
                error = null;
                _isRequesting = true;
                var members = _members.Append(_userId);
                var result = await GraphQLClient.AddSpace.ExecuteAsync(_name, _description, members.ToList());
                result.EnsureNoErrors();
                var space = result.Data!.AddSpace.Space!;
                var contact = new Contact
                {
                    Id = space.Id,
                    Name = _name,
                    Type = ChatType.Space,
                    Time = space.Updated.UtcDateTime
                };
                StateContainer.AddContact(contact);
                NavigationManager.NavigateTo($"/chat/space/{contact.Id}");
                MudDialog.Close(DialogResult.Ok(true));
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                _isRequesting = false;
            }
        }
    }

}