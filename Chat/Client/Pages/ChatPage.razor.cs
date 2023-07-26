using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using StrawberryShake;
using System.Net.Http.Json;
using Chat.Client.State;
using Chat.Client.GraphQL;
using Chat.Client.Layout;
using Chat.Client.Models;
using Chat.Shared;
using Chat.Shared.Dto;
using Chat.Shared.Request;
using MudBlazor;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Chat.Client.Pages;

[Layout(typeof(MainLayout))]
[Authorize]
public partial class ChatPage
{
    [CascadingParameter]
    private Task<AuthenticationState>? authenticationState { get; set; }

    [Parameter]
    public string? Id { get; set; }
    [Parameter]
    public string? Type { get; set; }

    [Inject]
    ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    StateContainer StateContainer { get; set; } = null!;

    [Inject]
    IGraphQLClient GraphQLClient { get; set; } = null!;

    [Inject]
    IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    HttpClient Http { get; set; } = null!;

    [Inject]
    IDialogService DialogService { get; set; } = null!;

    [Inject]
    IConfiguration Configuration { get; set; } = null!;

    [Inject]
    IAccessTokenProvider AccessTokenProvider { get; set; } = null!;

    int _id;
    ChatType _type;
    string? _userId;
    string? _error;
    string _textInput { get; set; } = string.Empty;

    static long maxFileSize = 1024 * 1024 * 10;

    public string TextInput
    {
        get => _textInput;
        set
        {
            if (_fileUploadResult == null)
            {
                _textInput = value;
            }
        }
    }

    List<MessageDto>? _messages;
    bool _loading;

    UploadResult? _fileUploadResult;

    HubConnection? hubConnection;

    // TODO: add fetching more message when scrolling on top
    async Task<List<MessageDto>> FetchMessages(int id, ChatType type)
    {
        if (type == ChatType.Direct)
        {
            var cache = StateContainer.GetMessages(type, id);
            if (cache != null)
            {
                return cache;
            }

            var result = await GraphQLClient.GetDirectMessages.ExecuteAsync(id);
            result.EnsureNoErrors();

            var messages = result.Data!.DirectMessages!.Edges!.Select(x => x.Node).Select(x => new MessageDto
            {
                Id = x.Id,
                Content = x.Content,
                TimeStamp = x.TimeStamp.UtcDateTime.ToString(Constants.DateFormat),
                Type = ChatType.Direct,
                AttachmentUri = x.Attachment?.Uri,
                Author = new UserDto
                {
                    Id = x.Author.Id,
                    UserName = x.Author.UserName!,
                    FirstName = x.Author.FirstName,
                    LastName = x.Author.LastName,
                    Avatar = x.Author.Avatar,
                }
            }).ToList();
            StateContainer.UpdateMessages(type, id, messages);
            return messages;
        }
        else
        {
            var cache = StateContainer.GetMessages(type, id);
            if (cache != null)
            {
                return cache;
            }
            var result = await GraphQLClient.GetSpaceMessages.ExecuteAsync(_id);
            result.EnsureNoErrors();
            var messages = result.Data!.SpaceMessages!.Edges!.Select(x => x.Node).Select(x => new MessageDto
            {
                Id = x.Id,
                Content = x.Content,
                TimeStamp = x.TimeStamp.UtcDateTime.ToString(Constants.DateFormat),
                Type = ChatType.Space,
                AttachmentUri = x.Attachment?.Uri,
                Author = new UserDto
                {
                    Id = x.Author.Id,
                    FirstName = x.Author.FirstName,
                    LastName = x.Author.LastName,
                    UserName = x.Author.UserName!,
                    Avatar = x.Author.Avatar
                }
            }).ToList();

            StateContainer.UpdateMessages(type, id, messages);
            return messages;
        }
    }

    async Task<LinkedList<Contact>> FetchContacts(string userId)
    {
        var mock = Configuration.GetValue<bool>("Mock");
        var random = new Random();
        var result = await GraphQLClient.GetChats.ExecuteAsync(userId);
        result.EnsureNoErrors();
        var user = result.Data!.User!;
        var chats1 = user.DirectChats1.Select(e => new Contact
        {
            Id = e.Id,
            Name = e.User1.Id == userId ? $"{e.User2.FirstName} {e.User2.LastName}" : $"{e.User1.FirstName} {e.User1.LastName}",
            Image = e.User1.Id == userId ? e.User2.Avatar : e.User1.Avatar,
            Type = ChatType.Direct,
            Time = e.Updated.UtcDateTime,
            UnreadMessageCount = mock ? random.Next(0, 3) : 0
        });

        var chats2 = user.DirectChats2.Select(e => new Contact
        {
            Id = e.Id,
            Name = e.User1.Id == userId ? $"{e.User2.FirstName} {e.User2.LastName}" : $"{e.User1.FirstName} {e.User1.LastName}",
            Image = e.User1.Id == userId ? e.User2.Avatar : e.User1.Avatar,
            Type = ChatType.Direct,
            Time = e.Updated.UtcDateTime,
            UnreadMessageCount = mock ? random.Next(0, 3) : 0
        });
        var spaceChats = user.SpaceMemberships.Select(e => new Contact
        {
            Id = e.Space.Id,
            Name = e.Space.Name,
            Image = "",
            Type = ChatType.Space,
            Time = e.Space.Updated.UtcDateTime,
            UnreadMessageCount = mock ? random.Next(0, 3) : 0
        });
        return new LinkedList<Contact>(chats1.Concat(chats2).Concat(spaceChats).OrderByDescending(e =>
         e.Time));
    }

    // In the initialize method, fetch the user's chats and add them to the state container
    // so that the sidebar can display them.
    // Also, initialize the SignalR connection
    protected override async Task OnInitializedAsync()
    {
        StateContainer.MainOnChange += StateHasChanged;
        if (authenticationState != null)
        {
            var authState = await authenticationState;
            var user = authState?.User;
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst("sub")?.Value;
                if (userId != null)
                {
                    _userId = userId;
                    try
                    {
                        // Fetch the contacts
                        var contacts = await FetchContacts(userId);
                        StateContainer.Contacts = contacts;
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Failed to fetch contacts: " + e.Message);
                        StateContainer.ContactError = e.Message;
                        return;
                    }

                    try
                    {
                        // Initialize the SignalR connection
                        hubConnection = new HubConnectionBuilder()
                            .WithUrl(Http.BaseAddress + "signalr", options =>
                            {
                                options.AccessTokenProvider = async () =>
                                {
                                    var tokenResult = await AccessTokenProvider.RequestAccessToken();
                                    if (tokenResult.TryGetToken(out var token))
                                    {
                                        return token.Value;
                                    }
                                    else
                                    {
                                        throw new Exception("Failed to get access token");
                                    }
                                };
                            })
                            .Build();

                        hubConnection.On<MessageDto>("ReceiveMessage", message =>
                        {
                            StateContainer.UpdateMessage(message);
                        });
                        if (!Configuration.GetValue<bool>("Mock"))
                        {
                            await hubConnection.StartAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Snackbar.Add(e.Message, Severity.Error);
                    }
                }
            }
            else
            {
                Console.Error.WriteLine("User is not authenticated");
                _error = "Not authenticated";
            }
        }
        else
        {
            Console.Error.WriteLine("Authentication state is  null");
            _error = "Not authenticated";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_messages != null && _messages.Count > 0)
        {
            await JSRuntime.InvokeVoidAsync("scrollToMessageListBottom");
        }
    }

    // When the parameters are set, fetch the messages for the chat
    protected override async Task OnParametersSetAsync()
    {
        _error = null;
        if (Id == null && Type == null)
        {
            _messages = null;
            return;
        }

        if (!string.IsNullOrEmpty(Id) && int.TryParse(Id, out _id))
        {
            try
            {
                if (Type == "dm")
                {
                    _loading = true;
                    StateContainer.SelectedChatType = ChatType.Direct;
                    StateContainer.SelectedChatId = _id;
                    _type = ChatType.Direct;
                    _messages = await FetchMessages(_id, _type);
                }
                else if (Type == "space")
                {
                    _loading = true;
                    StateContainer.SelectedChatType = ChatType.Space;
                    StateContainer.SelectedChatId = _id;
                    _type = ChatType.Space;
                    _messages = await FetchMessages(_id, _type);
                }
                else
                {
                    _error = "Invalid chat type";
                }
            }
            catch (Exception e)
            {
                _error = e.Message;
            }
            finally
            {
                _loading = false;
            }
        }
        else
        {
            _error = "Invalid chat id";
        }
    }

    public void Dispose()
    {
        StateContainer.MainOnChange -= StateHasChanged;
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection != null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    async Task SendMessage()
    {
        if (_userId != null && _messages != null && !string.IsNullOrWhiteSpace(_textInput))
        {
            var request = new MessageRequest
            {
                AuthorId = _userId,
                Content = _textInput,
                ChatId = _id,
                Type = _type,
                AttachmentUploadResult = _fileUploadResult
            };

            try
            {
                var response = await Http.PostAsJsonAsync<MessageRequest>("/api/v1/message", request);
                response.EnsureSuccessStatusCode();
                _fileUploadResult = null;
                _textInput = "";
                var message = await response.Content.ReadFromJsonAsync<MessageDto>();
                if (message != null)
                {
                    _messages.Add(message);
                }
                else
                {
                    Console.Error.WriteLine("Message is null");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Snackbar.Add("Failed to send the message", Severity.Error);
            }
        }
    }

    async Task UploadFiles(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        try
        {
            HttpResponseMessage response;
            var readStream = file.OpenReadStream(maxFileSize);

            if (Configuration.GetValue<bool>("Mock"))
            {
                var memoryStream = new MemoryStream(new byte[readStream.Length]);
                await readStream.CopyToAsync(memoryStream);
                var base64 = Convert.ToBase64String(memoryStream.ToArray());
                response = await Http.PostAsync("/api/v1/file", new StringContent($"data:{file.ContentType};base64,{base64}"));
            }
            else
            {
                var fileContent = new StreamContent(readStream);
                fileContent.Headers.ContentType =
                            new MediaTypeHeaderValue(file.ContentType);
                content.Add(
                            content: fileContent,
                            name: "\"files\"",
                            fileName: file.Name);
                response = await Http.PostAsync("/api/v1/file", content);
            }
            response.EnsureSuccessStatusCode();
            Snackbar.Add("File uploaded", Severity.Success);
            _fileUploadResult = await response.Content.ReadFromJsonAsync<UploadResult>();
            _textInput = file.Name;
        }
        catch (Exception e)
        {
            Snackbar.Add("Failed to upload the file", Severity.Error);
            Console.Error.WriteLine(e.Message);
        }
    }

    void CancelFile()
    {
        _fileUploadResult = null;
        _textInput = "";
    }
}