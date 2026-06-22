using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YugiDeck.Core.DTOs.Auth;
using YugiDeck.Core.Entities;
using YugiDeck.Core.Interfaces;
using YugiDeck.Infrastructure.Data;

namespace YugiDeck.Infrastructure.Services;

public class AuthService(
    UserManager<IdentityUser> userManager,
    AppDbContext db,
    IConfiguration config,
    IEmailService emailService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var user = new IdentityUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        token.IsRevoked = true;
        await db.SaveChangesAsync();

        var user = await userManager.FindByIdAsync(token.UserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        return await BuildResponseAsync(user);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is not null)
        {
            token.IsRevoked = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return; // silent — don't reveal whether email exists

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = config["FrontendUrl"] ?? "http://localhost:4200";
        var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await emailService.SendPasswordResetAsync(email, user.UserName ?? email, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Invalid request.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<AuthResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Check email taken by another user
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != userId)
                throw new InvalidOperationException("Email is already in use.");

            user.Email = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
        }

        // Check username taken by another user
        if (!string.Equals(user.UserName, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await userManager.FindByNameAsync(request.Username);
            if (existing is not null && existing.Id != userId)
                throw new InvalidOperationException("Username is already taken.");

            user.UserName = request.Username;
            user.NormalizedUserName = request.Username.ToUpperInvariant();
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return await BuildResponseAsync(user);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private async Task<AuthResponse> BuildResponseAsync(IdentityUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? ""
        };
    }

    private string GenerateAccessToken(IdentityUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = int.Parse(config["JwtSettings:ExpiryMinutes"]!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> CreateRefreshTokenAsync(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiryDays = int.Parse(config["JwtSettings:RefreshTokenExpiryDays"]!);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return token;
    }
}
