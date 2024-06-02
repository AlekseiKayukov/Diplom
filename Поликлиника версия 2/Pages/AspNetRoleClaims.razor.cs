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
    public partial class AspNetRoleClaims
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

        protected IEnumerable<Polyclinic.Models.data.AspNetRoleClaim> aspNetRoleClaims;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetRoleClaim> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetRoleClaims = await dataService.GetAspNetRoleClaims(new Query { Filter = $@"i => i.ClaimType.Contains(@0) || i.ClaimValue.Contains(@0) || i.RoleId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetRole" });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetRoleClaims = await dataService.GetAspNetRoleClaims(new Query { Filter = $@"i => i.ClaimType.Contains(@0) || i.ClaimValue.Contains(@0) || i.RoleId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetRole" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetRoleClaim>("Добавить утверждение к роли", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetRoleClaim> args)
        {
            await DialogService.OpenAsync<EditAspNetRoleClaim>("Редактировать утверждение роли", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetRoleClaim aspNetRoleClaim)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetRoleClaim(aspNetRoleClaim.Id);

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
                    Detail = $"Не удается удалить заявку на роль Asp Net"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportAspNetRoleClaimsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetRole",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetRoleClaims");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetRoleClaimsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetRole",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetRoleClaims");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetRoleClaim aspNetRoleClaim)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран cетевые требования к роли:";
            message.Detail = $"{aspNetRoleClaim.AspNetRole.Name}";
            NotificationService.Notify(message);
        }

    }
}