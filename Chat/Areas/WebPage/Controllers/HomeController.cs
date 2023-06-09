﻿using System.Diagnostics;
using System.Security.Claims;
using Chat.Areas.WebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Chat.Areas.Api.Services;
using Chat.Common.DTOs;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthenticationService _authenticationService;

    public HomeController(ILogger<HomeController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel();


        model.RedirectMessage = (RedirectMessage?) TempData["RedirectMessage"];
      
        // If redirected from login
        if (TempData["loggedinUser"] != null) {
            model.LoggedInUser = (User) TempData["loggedinUser"]!;
        }

        try
        {
            var token = Request.Cookies["chat_access_token"];
            if (token != null)
            {
                var result = await _authenticationService.ValidateToken(token);
                if (result.IsValid)
                {
                    if (result.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var username))
                    {
                        Console.WriteLine("Valid user");
                        model.LoggedInUser = new User { Username =  (string)username};
                    }
                }
            }
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError("Index with unexpected error: {}", e.Message);
            return View(model);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}