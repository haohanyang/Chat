using Chat.Client.GraphQL;
using Chat.Shared.Dto;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using StrawberryShake;

namespace Chat.Client.Components;

public partial class ContactStatus : ComponentBase
{
    [Parameter]
    public int ChatId { get; set; }
    int _chatId = -1;

    [Parameter]
    public ChatType Type { get; set; }
    ChatType _type;

    [Parameter]
    public string UserId { get; set; } = null!;

    [Inject]
    IDialogService DialogService { get; set; } = null!;

    [Inject]
    IGraphQLClient GraphQLClient { get; set; } = null!;

    UserDto? user;
    SpaceDto? space;
    string? error;

    private void OpenContactDetails()
    {
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            NoHeader = true,
        };
        var parameters = new DialogParameters<ContactDetailsDialog>();
        parameters.Add(x => x.User, user);
        parameters.Add(x => x.Space, space);
        DialogService.Show<ContactDetailsDialog>("Details", parameters, options);
    }

    async Task FetchContact()
    {
        if (Type == ChatType.Direct)
        {
            var result = await GraphQLClient.GetDirectChat.ExecuteAsync(ChatId);
            result.EnsureNoErrors();
            var chat = result.Data!.DirectChat!;
            user = chat.User1.Id == UserId ? new UserDto
            {
                Id = chat.User2.Id,
                FirstName = chat.User2.FirstName,
                UserName = chat.User2.UserName!,
                Avatar = chat.User2.Avatar,
                Bio = chat.User2.Bio,
            } : new UserDto
            {
                Id = chat.User1.Id,
                FirstName = chat.User1.FirstName,
                UserName = chat.User1.UserName!,
                Avatar = chat.User1.Avatar,
                Bio = chat.User1.Bio,
            };
        }
        else
        {
            var result = await GraphQLClient.GetSpace.ExecuteAsync(ChatId);
            result.EnsureNoErrors();
            var data = result.Data!.Space!;
            space = new SpaceDto
            {
                Id = data.Id,
                Name = data.Name,
                Description = data.Description,
                Members = data.Memberships.Select(x => x.Member).Select(x => new UserDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    UserName = x.UserName!,
                    Avatar = x.Avatar,
                })
            };
        }
    }

    protected async override Task OnInitializedAsync()
    {
        _chatId = ChatId;
        _type = Type;
        try
        {
            await FetchContact();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_chatId == ChatId && _type == Type)
        {
            return;
        }

        user = null;
        space = null;
        _type = Type;
        _chatId = ChatId;
        try
        {
            await FetchContact();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }
}