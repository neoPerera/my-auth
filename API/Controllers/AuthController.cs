using APPLICATION.DTOs;
using CORE.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        return await SignIn(loginRequest);
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequest loginRequest)
    {
        var result = await _authService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);
        
        if (result.Success)
        {
            var response = new AuthResponse
            {
                Success = true,
                Token = result.Token,
                Message = result.Message,
                User = result.User != null ? new UserInfo
                {
                    Id = result.User.Id,
                    Username = result.User.Username
                } : null
            };
            return Ok(response);
        }
        else
        {
            return Unauthorized(new { message = result.Message });
        }
    }
    //
    // [HttpPost("validate")]
    // public async Task<IActionResult> Validate([FromBody] ValidateRequest validateRequest)
    // {
    //     var result = await _authService.ValidateTokenAsync(validateRequest.Token);
    //     
    //     if (result.IsValid)
    //     {
    //         // Convert claims to dictionary
    //         var claimsDict = result.Claims?
    //             .GroupBy(c => c.Type)
    //             .ToDictionary(
    //                 g => g.Key,
    //                 g => g.Count() == 1 ? g.First().Value : string.Join(", ", g.Select(c => c.Value))
    //             );
    //
    //         var response = new ValidateResponse
    //         {
    //             IsValid = true,
    //             Message = result.Message,
    //             Claims = claimsDict
    //         };
    //         return Ok(response);
    //     }
    //     else
    //     {
    //         return Unauthorized(new { message = result.Message });
    //     }
    // }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
    {
        var result = await _authService.SignUpAsync(signUpRequest.Username, signUpRequest.Password);

        if (result.Success)
        {
            var response = new SignUpResponse
            {
                Success = true,
                Message = result.Message,
                Token = result.Token,
                User = result.User != null ? new UserInfo
                {
                    Id = result.User.Id,
                    Username = result.User.Username
                } : null
            };
            return Ok(response);
        }
        else
        {
            return BadRequest(new SignUpResponse
            {
                Success = false,
                Message = result.Message
            });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
    {
        // Get username from JWT token claims
        var usernameClaim = User.FindFirst("username")?.Value;
        
        if (string.IsNullOrEmpty(usernameClaim))
        {
            return Unauthorized(new ChangePasswordResponse
            {
                Success = false,
                Message = "Invalid token: username claim not found"
            });
        }

        var result = await _authService.ChangePasswordAsync(
            usernameClaim,
            changePasswordRequest.CurrentPassword,
            changePasswordRequest.NewPassword);

        if (result.Success)
        {
            var response = new ChangePasswordResponse
            {
                Success = true,
                Message = result.Message
            };
            return Ok(response);
        }
        else
        {
            return BadRequest(new ChangePasswordResponse
            {
                Success = false,
                Message = result.Message
            });
        }
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize([FromBody] AuthorizationRequest authorizationRequest)
    {
        var result = await _authService.AuthorizeRequestAsync(
            authorizationRequest.Token,
            authorizationRequest.Service,
            authorizationRequest.Controller,
            authorizationRequest.Action);

        // Convert claims to dictionary for JSON serialization
        var claimsDict = result.Claims?
            .GroupBy(c => c.Type)
            .ToDictionary(
                g => g.Key,
                g => g.Count() == 1 ? g.First().Value : string.Join(", ", g.Select(c => c.Value))
            );

        // Extract roles from claims
        var roles = result.Claims?
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var response = new AuthorizationResponse
        {
            IsAuthorized = result.IsAuthorized,
            Message = result.Message,
            Roles = roles ?? result.Roles,
            Claims = claimsDict
        };

        if (result.IsAuthorized)
        {
            return Ok(response);
        }
        else
        {
            return Unauthorized(response);
        }
    }
}

