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
    public partial class AspNetUserClaims
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

        protected IEnumerable<Polyclinic.Models.data.AspNetUserClaim> aspNetUserClaims;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetUserClaim> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetUserClaims = await dataService.GetAspNetUserClaims(new Query { Filter = $@"i => i.ClaimType.Contains(@0) || i.ClaimValue.Contains(@0) || i.UserId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetUserClaims = await dataService.GetAspNetUserClaims(new Query { Filter = $@"i => i.ClaimType.Contains(@0) || i.ClaimValue.Contains(@0) || i.UserId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetUserClaim>("Добавить утверждение к пользователю", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetUserClaim> args)
        {
            await DialogService.OpenAsync<EditAspNetUserClaim>("Редактировать утверждение пользователя", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetUserClaim aspNetUserClaim)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetUserClaim(aspNetUserClaim.Id);

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
                    Detail = $"Не удается удалить заявления пользователей asp net"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportAspNetUserClaimsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserClaims");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetUserClaimsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserClaims");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetUserClaim aspNetUserClaim)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрана пользователь:";
            message.Detail = $"{aspNetUserClaim.AspNetUser.Email}";
            NotificationService.Notify(message);
        }

    }
}