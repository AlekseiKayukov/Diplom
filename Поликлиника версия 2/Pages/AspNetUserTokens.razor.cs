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
    public partial class AspNetUserTokens
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

        protected IEnumerable<Polyclinic.Models.data.AspNetUserToken> aspNetUserTokens;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetUserToken> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetUserTokens = await dataService.GetAspNetUserTokens(new Query { Filter = $@"i => i.UserId.Contains(@0) || i.LoginProvider.Contains(@0) || i.Name.Contains(@0) || i.Value.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetUserTokens = await dataService.GetAspNetUserTokens(new Query { Filter = $@"i => i.UserId.Contains(@0) || i.LoginProvider.Contains(@0) || i.Name.Contains(@0) || i.Value.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetUserToken>("Добавить токен пользователя", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetUserToken> args)
        {
            await DialogService.OpenAsync<EditAspNetUserToken>("Редактировать токен пользователя", new Dictionary<string, object> { {"UserId", args.Data.UserId}, {"LoginProvider", args.Data.LoginProvider}, {"Name", args.Data.Name} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetUserToken aspNetUserToken)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetUserToken(aspNetUserToken.UserId, aspNetUserToken.LoginProvider, aspNetUserToken.Name);

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
                    Detail = $"Не удается удалить токен пользователя Asp Net"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportAspNetUserTokensToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserTokens");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetUserTokensToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserTokens");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetUserToken aspNetUserToken)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран токен пользователя:";
            message.Detail = $"{aspNetUserToken.Name}";
            NotificationService.Notify(message);
        }
    }
}