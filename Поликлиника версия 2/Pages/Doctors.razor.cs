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
    public partial class Doctors
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

        protected IEnumerable<Polyclinic.Models.data.Doctor> doctors;

        protected RadzenDataGrid<Polyclinic.Models.data.Doctor> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            doctors = await dataService.GetDoctors(new Query { Filter = $@"i => i.Surname.Contains(@0) || i.Name.Contains(@0) || i.Forename.Contains(@0) || i.Phone.Contains(@0) || i.AspNetUser.Email.Contains(@0) || i.Gender.Title.Contains(@0) || i.Specialization.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Gender,Specialization,AspNetUser" });
        }
        protected override async Task OnInitializedAsync()
        {
            doctors = await dataService.GetDoctors(new Query { Filter = $@"i => i.Surname.Contains(@0) || i.Name.Contains(@0) || i.Forename.Contains(@0) || i.Phone.Contains(@0) || i.AspNetUser.Email.Contains(@0) || i.Gender.Title.Contains(@0) || i.Specialization.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Gender,Specialization,AspNetUser" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddDoctor>("Добавить доктора", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.Doctor> args)
        {
            await DialogService.OpenAsync<EditDoctor>("Редактировать доктора", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.Doctor doctor)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteDoctor(doctor.Id);

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
                    Detail = $"Не удается удалить доктора"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportDoctorsToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Gender,Specialization,AspNetUser",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Doctors");
                            }

                            if (args == null || args.Value == "xlsx")
                            {
                                await dataService.ExportDoctorsToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Gender,Specialization,AspNetUser",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Doctors");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.Doctor doctor)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран доктор:";
            message.Detail = $"{doctor.Surname} {doctor.Name} {doctor.Forename}";
            NotificationService.Notify(message);
        }
    }
}