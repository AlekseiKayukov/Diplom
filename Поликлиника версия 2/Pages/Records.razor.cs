using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Polyclinic.Models.data;

namespace Polyclinic.Pages
{
    public partial class Records
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

        protected IEnumerable<Polyclinic.Models.data.Record> records;

        protected RadzenDataGrid<Polyclinic.Models.data.Record> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";
            await grid0.GoToPage(0);

            records = await dataService.GetRecords(new Query { Filter = $@"i => i.Patient.Surname.Contains(@0) || i.Patient.Name.Contains(@0) || i.Patient.Forename.Contains(@0) || i.Schedule.Cabinet.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Schedule,Patient" });
        }
        protected override async Task OnInitializedAsync()
        {
            records = await dataService.GetRecords(new Query { Filter = $@"i => i.Patient.Surname.Contains(@0) || i.Patient.Name.Contains(@0) || i.Patient.Forename.Contains(@0) || i.Schedule.Cabinet.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Schedule,Patient" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddRecord>("Добавить запись", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.Record> args)
        {
            await DialogService.OpenAsync<EditRecord>("Редактировать запись", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.Record record)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteRecord(record.Id);

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
                    Detail = $"Не удается удалить запись"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportRecordsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Schedule,Patient",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Records");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportRecordsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Schedule,Patient",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Records");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.Record record)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрана запись пациента:";
            message.Detail = $"{record.Patient.Surname} {record.Patient.Name} {record.Patient.Forename}";
            NotificationService.Notify(message);
        }
    }
}