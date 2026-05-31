using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MedLink.Web.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // We only send the email if the user exists AND their email is confirmed
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // We return this even if user doesn't exist to prevent email enumeration attacks
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            // YOUR CUSTOM HTML EMAIL TEMPLATE
            var htmlMessage = $@"
                <div style='max-width: 500px; margin: auto; font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px; border-radius: 8px;'>
                    <h2 style='color: #004d40;'>MedLink Security</h2>
                    <p>Hello,</p>
                    <p>We received a request to reset your password. Click the button below to proceed:</p>
                    <p style='text-align: center;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? "")}' 
                           style='background: #004d40; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                           Reset My Password
                        </a>
                    </p>
                    <p>If you did not make this request, please ignore this email.</p>
                </div>";

            await _emailSender.SendEmailAsync(Input.Email, "Reset your password", htmlMessage);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}