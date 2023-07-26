using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Chat.Client.Layout;
using Chat.Shared.Request;
using Microsoft.AspNetCore.Components.Forms;

namespace Chat.Client.Pages;

[Layout(typeof(HomeLayout))]
public partial class Signin
{
    [Inject]
    HttpClient Http { get; set; } = null!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = null!;

    [Inject] IConfiguration Configuration { get; set; } = null!;
    bool isRequesting;
    string? error;
    bool success;
    bool showMockInfo = true;
    LoginRequest formModel = new();

    void CloseInfo() => showMockInfo = false;
    async Task Login(EditContext context)
    {
        error = null;
        success = false;
        isRequesting = true;
        try
        {
            var response = await Http.PostAsJsonAsync<LoginRequest>("/api/v1/auth/signin", formModel);
            if (response.IsSuccessStatusCode)
            {
                success = true;
                StateHasChanged();
                await Task.Delay(2000);
                NavigationManager.NavigateTo("/chat", true);
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                error = result ?? "Unknown error occured";
            }
        }
        catch (Exception ex)
        {
            error = "Unknown error occured";
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            isRequesting = false;
        }
    }
}