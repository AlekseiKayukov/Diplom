using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace Polyclinic.Pages
{
    public partial class Patients
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

        protected IEnumerable<Polyclinic.Models.data.Patient> patients;

        protected RadzenDataGrid<Polyclinic.Models.data.Patient> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            patients = await dataService.GetPatients(new Query { Filter = $@"i => i.AspNetUser.Email.Contains(@0) || i.Surname.Contains(@0) || i.Name.Contains(@0) || i.Forename.Contains(@0) || i.PlaceResidence.Contains(@0) || i.Address.Contains(@0) || i.Phone.Contains(@0) || i.Email.Contains(@0) || i.MHIpolicy.Contains(@0) || i.VMIpolicy.Contains(@0) || i.PassportSeries.Contains(@0) || i.PassportNumber.Contains(@0) || i.DateOfBirth.ToString().Contains(@0) || i.DateOfIssue.ToString().Contains(@0) || i.Gender.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser,Gender" });
        }
        protected override async Task OnInitializedAsync()
        {
            patients = await dataService.GetPatients(new Query { Filter = $@"i => i.AspNetUser.Email.Contains(@0) || i.Surname.Contains(@0) || i.Name.Contains(@0) || i.Forename.Contains(@0) || i.PlaceResidence.Contains(@0) || i.Address.Contains(@0) || i.Phone.Contains(@0) || i.Email.Contains(@0) || i.MHIpolicy.Contains(@0) || i.VMIpolicy.Contains(@0) || i.PassportSeries.Contains(@0) || i.PassportNumber.Contains(@0) || i.DateOfBirth.ToString().Contains(@0) || i.DateOfIssue.ToString().Contains(@0) || i.Gender.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser,Gender" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddPatient>("Добавить пациента", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.Patient> args)
        {
            await DialogService.OpenAsync<EditPatient>("Редактировать пациента", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.Patient patient)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeletePatient(patient.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Ошибка",
                    Detail = $"Не удается удалить пациента"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportPatientsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser,Gender",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Patients");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportPatientsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser,Gender",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Patients");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.Patient patient)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран пациент:";
            message.Detail = $"{patient.Surname} {patient.Name} {patient.Forename}";
            NotificationService.Notify(message);
        }
    }
}