using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace Polyclinic.Pages
{
    public partial class Schedules
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

        protected IEnumerable<Polyclinic.Models.data.Schedule> schedules;

        protected RadzenDataGrid<Polyclinic.Models.data.Schedule> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            schedules = await dataService.GetSchedules(new Query { Filter = $@"i => i.Cabinet.Contains(@0) || i.Duration.ToString().Contains(@0) || i.Doctor.Surname.Contains(@0) || i.Doctor.Name.Contains(@0) || i.Doctor.Forename.Contains(@0) || i.Doctor.Specialization.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Doctor" });
        }


        protected override async Task OnInitializedAsync()
        {
            schedules = await dataService.GetSchedules(new Query { Filter = $@"i => i.Cabinet.Contains(@0) || i.Duration.ToString().Contains(@0) || i.Doctor.Surname.Contains(@0) || i.Doctor.Name.Contains(@0) || i.Doctor.Forename.Contains(@0) || i.Doctor.Specialization.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Doctor" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddSchedule>("Добавить расписание", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.Schedule> args)
        {
            await DialogService.OpenAsync<EditSchedule>("Редактировать расписание", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.Schedule schedule)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteSchedule(schedule.Id);

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
                    Detail = $"Не удается удалить расписание"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportSchedulesToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Doctor",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Schedules");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportSchedulesToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Doctor",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Schedules");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.Schedule schedule)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрано расписание доктора:";
            message.Detail = $"{schedule.Doctor.Surname} {schedule.Doctor.Name} {schedule.Doctor.Forename}";
            NotificationService.Notify(message);
        }
    }
}