using Microsoft.Extensions.Configuration;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace MedLink.Presenter.Presenters;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent);
    Task SendAppointmentConfirmationAsync(string email, string patientName, string doctorName, DateTime date, string time, string appointmentNumber);
    Task SendAppointmentReminderAsync(string email, string patientName, string doctorName, DateTime date, string time);
    Task SendCancellationEmailAsync(string email, string patientName, string doctorName, DateTime date);
}

public class BrevoEmailSender : IEmailSender
{
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public BrevoEmailSender(IConfiguration config)
    {
        _apiKey = config["Brevo:ApiKey"] ?? string.Empty;
        _senderEmail = config["Brevo:SenderEmail"] ?? "noreply@medlink.edu";
        _senderName = config["Brevo:SenderName"] ?? "MedLink Health Portal";
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        if (string.IsNullOrEmpty(_apiKey)) return;

        Configuration.Default.ApiKey["api-key"] = _apiKey;
        var apiInstance = new TransactionalEmailsApi();
        var sendEmail = new SendSmtpEmail
        {
            To = new List<SendSmtpEmailTo> { new() { Email = toEmail, Name = toName } },
            Sender = new SendSmtpEmailSender { Email = _senderEmail, Name = _senderName },
            Subject = subject,
            HtmlContent = htmlContent
        };
        await apiInstance.SendTransacEmailAsync(sendEmail);
    }

    public async Task SendAppointmentConfirmationAsync(string email, string patientName, string doctorName, DateTime date, string time, string appointmentNumber)
    {
        var html = $@"
        <div style='font-family:sans-serif;max-width:600px;margin:auto'>
            <div style='background:#0f4c81;padding:20px;text-align:center'>
                <h1 style='color:#fff;margin:0'>MedLink</h1>
                <p style='color:#a0c4e4;margin:4px 0'>University Health Portal</p>
            </div>
            <div style='padding:32px;background:#fff'>
                <h2 style='color:#0f4c81'>Appointment Confirmed ✓</h2>
                <p>Dear {patientName},</p>
                <p>Your appointment has been successfully booked.</p>
                <div style='background:#f0f7ff;border-left:4px solid #0f4c81;padding:16px;margin:20px 0'>
                    <p style='margin:4px 0'><strong>Appointment No:</strong> {appointmentNumber}</p>
                    <p style='margin:4px 0'><strong>Doctor:</strong> {doctorName}</p>
                    <p style='margin:4px 0'><strong>Date:</strong> {date:dddd, MMMM d, yyyy}</p>
                    <p style='margin:4px 0'><strong>Time:</strong> {time}</p>
                </div>
                <p>Please arrive 10 minutes early. Bring your student ID card.</p>
            </div>
            <div style='background:#f8f9fa;padding:16px;text-align:center;color:#666;font-size:12px'>
                <p>MedLink — University Health Center</p>
            </div>
        </div>";
        await SendEmailAsync(email, patientName, $"Appointment Confirmed — {appointmentNumber}", html);
    }

    public async Task SendAppointmentReminderAsync(string email, string patientName, string doctorName, DateTime date, string time)
    {
        var html = $@"
        <div style='font-family:sans-serif;max-width:600px;margin:auto'>
            <div style='background:#0f4c81;padding:20px;text-align:center'>
                <h1 style='color:#fff;margin:0'>MedLink</h1>
            </div>
            <div style='padding:32px;background:#fff'>
                <h2 style='color:#0f4c81'>Appointment Reminder 🔔</h2>
                <p>Dear {patientName}, your appointment is tomorrow.</p>
                <div style='background:#fff8e1;border-left:4px solid #ffc107;padding:16px;margin:20px 0'>
                    <p style='margin:4px 0'><strong>Doctor:</strong> {doctorName}</p>
                    <p style='margin:4px 0'><strong>Date:</strong> {date:dddd, MMMM d, yyyy}</p>
                    <p style='margin:4px 0'><strong>Time:</strong> {time}</p>
                </div>
            </div>
        </div>";
        await SendEmailAsync(email, patientName, "Appointment Reminder — MedLink", html);
    }

    public async Task SendCancellationEmailAsync(string email, string patientName, string doctorName, DateTime date)
    {
        var html = $@"
        <div style='font-family:sans-serif;max-width:600px;margin:auto'>
            <div style='background:#0f4c81;padding:20px;text-align:center'>
                <h1 style='color:#fff;margin:0'>MedLink</h1>
            </div>
            <div style='padding:32px;background:#fff'>
                <h2 style='color:#dc3545'>Appointment Cancelled</h2>
                <p>Dear {patientName},</p>
                <p>Your appointment with {doctorName} on {date:dddd, MMMM d, yyyy} has been cancelled.</p>
                <p>You can book a new appointment at any time through the portal.</p>
            </div>
        </div>";
        await SendEmailAsync(email, patientName, "Appointment Cancelled — MedLink", html);
    }
}