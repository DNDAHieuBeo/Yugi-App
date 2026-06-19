using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using YugiDeck.Core.Interfaces;

namespace YugiDeck.Infrastructure.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
    {
        var host     = config["EmailSettings:SmtpHost"]!;
        var port     = int.Parse(config["EmailSettings:SmtpPort"]!);
        var sender   = config["EmailSettings:SenderEmail"]!;
        var name     = config["EmailSettings:SenderName"]!;
        var password = config["EmailSettings:AppPassword"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(name, sender));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Reset your CardVault password";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px 24px">
                  <h2 style="color:#22c55e;margin-bottom:8px">CardVault</h2>
                  <h3 style="color:#111827;margin-bottom:16px">Reset your password</h3>
                  <p style="color:#6b7280;margin-bottom:24px">
                    Click the button below to set a new password. This link expires in <strong>1 hour</strong>.
                  </p>
                  <a href="{resetLink}"
                     style="display:inline-block;background:#22c55e;color:#fff;padding:12px 28px;
                            border-radius:8px;text-decoration:none;font-weight:600">
                    Reset Password
                  </a>
                  <p style="color:#9ca3af;font-size:12px;margin-top:32px">
                    If you didn't request this, you can safely ignore this email.
                  </p>
                </div>
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(sender, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
