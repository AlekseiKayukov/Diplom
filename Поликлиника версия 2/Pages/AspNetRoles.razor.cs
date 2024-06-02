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
    public partial class AspNetRoles
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

        protected IEnumerable<Polyclinic.Models.data.AspNetRole> aspNetRoles;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetRole> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetRoles = await dataService.GetAspNetRoles(new Query { Filter = $@"i => i.Id.Contains(@0) || i.ConcurrencyStamp.Contains(@0) || i.Name.Contains(@0) || i.NormalizedName.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetRoles = await dataService.GetAspNetRoles(new Query { Filter = $@"i => i.Id.Contains(@0) || i.ConcurrencyStamp.Contains(@0) || i.Name.Contains(@0) || i.NormalizedName.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetRole>("Добавить роль", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetRole> args)
        {
            await DialogService.OpenAsync<EditAspNetRole>("Редактировать роль", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetRole aspNetRole)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetRole(aspNetRole.Id);

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
                    Detail = $"Не удается удалить AspNetRole"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportAspNetRolesToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetRoles");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetRolesToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetRoles");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetRole aspNetRole)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрана роль в сети:";
            message.Detail = $"{aspNetRole.Name}";
            NotificationService.Notify(message);
        }

    }
}