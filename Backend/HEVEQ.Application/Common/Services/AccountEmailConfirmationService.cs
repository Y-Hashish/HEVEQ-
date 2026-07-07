using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Options;
using HEVEQ.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace HEVEQ.Application.Common.Services;

public class AccountEmailConfirmationService : IAccountEmailConfirmationService
{
    private readonly AppUrlSettings _appUrls;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountEmailConfirmationService(UserManager<ApplicationUser> userManager, IEmailService emailService, IOptions<AppUrlSettings> appUrlOptions)
    {
        _userManager = userManager;
        _emailService = emailService;
        _appUrls = appUrlOptions.Value;
    }

    public async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new InvalidOperationException("User email is required.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);

        var confirmationUrl = BuildFrontendConfirmationUrl(user.Id, encodedToken);
        var logoUrl = BuildLogoUrl();

        var firstName = string.IsNullOrWhiteSpace(user.FirstName) ? "User" : WebUtility.HtmlEncode(user.FirstName);

        var htmlBody = $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Confirm your HEVEQ account</title>
</head>
<body style="margin:0;padding:0;background-color:#f5f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f5f7fb;margin:0;padding:30px 0;">
        <tr>
            <td align="center">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:640px;background-color:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 18px rgba(0,0,0,0.08);">
                    
                    <tr>
                        <td align="center" style="padding:30px 30px 10px 30px;background-color:#ffffff;">
                            <img src="{logoUrl}" alt="HEVEQ Logo" style="max-width:240px;width:100%;height:auto;display:block;border:0;" />
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:10px 40px 0 40px;text-align:center;">
                            <h1 style="margin:0;font-size:28px;line-height:1.4;color:#0f172a;">Confirm Your Email</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:16px 40px 0 40px;text-align:center;">
                            <p style="margin:0;font-size:16px;line-height:1.8;color:#475569;">
                                Hello <strong>{firstName}</strong>,
                            </p>
                            <p style="margin:16px 0 0 0;font-size:16px;line-height:1.8;color:#475569;">
                                Welcome to <strong>HEVEQ</strong>. Please confirm your email address to activate your account
                                and continue using the platform.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td align="center" style="padding:30px 40px 10px 40px;">
                            <a href="{confirmationUrl}"
                               style="display:inline-block;background-color:#0b1f44;color:#ffffff;text-decoration:none;font-size:16px;font-weight:bold;padding:14px 28px;border-radius:10px;">
                                Confirm Email
                            </a>
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:10px 40px 0 40px;text-align:center;">
                            <p style="margin:0;font-size:14px;line-height:1.8;color:#64748b;">
                                If the button above does not work, copy and paste this link into your browser:
                            </p>
                            <p style="margin:12px 0 0 0;font-size:14px;line-height:1.8;word-break:break-word;">
                                <a href="{confirmationUrl}" style="color:#ea580c;text-decoration:none;">{confirmationUrl}</a>
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:30px 40px 35px 40px;text-align:center;">
                            <p style="margin:0;font-size:13px;line-height:1.8;color:#94a3b8;">
                                If you did not create an account, you can safely ignore this email.
                            </p>
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:18px 30px;background-color:#f8fafc;border-top:1px solid #e2e8f0;text-align:center;">
                            <p style="margin:0;font-size:12px;color:#64748b;">
                                © HEVEQ. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>
""";

        await _emailService.SendEmailAsync(user.Email, "Confirm your account", htmlBody, cancellationToken);
    }

    private string BuildFrontendConfirmationUrl(Guid userId, string encodedToken)
    {
        var frontendBaseUrl = _appUrls.FrontendBaseUrl?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            throw new InvalidOperationException("AppUrls:FrontendBaseUrl is required for email confirmation.");

        return $"{frontendBaseUrl}/auth/confirm-email?userId={userId}&token={encodedToken}";
    }

    private string BuildLogoUrl()
    {
        var apiBaseUrl = _appUrls.ApiBaseUrl?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
            throw new InvalidOperationException("AppUrls:ApiBaseUrl is required to build the logo URL.");

        return $"{apiBaseUrl}/images/HEVEQ.png";
    }
}