using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Radzen.Blazor;

namespace Polyclinic.Pages
{
    public partial class EditRecord
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }
        [Inject]
        public dataService dataService { get; set; }

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            record = await dataService.GetRecordById(Id);

            schedulesForIdSchedule = await dataService.GetFreeSchedulesEdit((int)record.IdSchedule);

            patientsForIdPatient = await dataService.GetPatients();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.Record record;

        protected IEnumerable<Polyclinic.Models.data.Schedule> schedulesForIdSchedule;

        protected IEnumerable<Polyclinic.Models.data.Patient> patientsForIdPatient;

        protected async Task FormSubmit()
        {
            try
            {
                await dataService.UpdateRecord(Id, record);
                record = await dataService.GetRecordById(Id);
                SendByEmail();
                DialogService.Close(record);
            }
            catch (Exception ex)
            {
                hasChanges = ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
                canEdit = !(ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException);
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }


        protected bool hasChanges = false;
        protected bool canEdit = true;

        [Inject]
        protected SecurityService Security { get; set; }


        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
            dataService.Reset();
            hasChanges = false;
            canEdit = true;

            record = await dataService.GetRecordById(Id);
        }

        public void SendByEmail()
        {
            // Открываем и читаем данные из JSON файла
            string jsonFilePath = @$"{Environment.CurrentDirectory}\Models\Email\Setting.json";
            string jsonText = File.ReadAllText(jsonFilePath);

            // Десериализуем данные из JSON файла
            var emailData = JsonSerializer.Deserialize<Polyclinic.Models.Email.EmailSetting>(jsonText);

            var patient =  dataService.GetPatientByEmailAsync(Convert.ToInt32(record.IdPatient));

            if (patient != null && !string.IsNullOrEmpty(patient.Result.Email))
            {
                // Отправляем письмо с данными из JSON файла
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailData.AddressEmail);
                    mail.To.Add(patient.Result.Email);
                    mail.Subject = emailData.Subject;
                    mail.Body = $"{emailData.Body} {record.CabinetWorkTime}";

                    using (var smtp = new SmtpClient(emailData.AddressSMPT, emailData.Port))
                    {
                        smtp.Credentials = new NetworkCredential(emailData.AddressEmail, emailData.Password);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            else
            {
                var message = new NotificationMessage();
                message.Summary = "У пациента не указана почта";
                message.Detail = $"{patient.Result.PatientFullName}";
                message.Severity = NotificationSeverity.Error;
                message.Duration = 4000;
                NotificationService.Notify(message);
            }
        }
    }
}