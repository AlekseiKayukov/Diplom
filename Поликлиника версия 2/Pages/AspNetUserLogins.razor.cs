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
    public partial class AspNetUserLogins
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

        protected IEnumerable<Polyclinic.Models.data.AspNetUserLogin> aspNetUserLogins;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetUserLogin> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetUserLogins = await dataService.GetAspNetUserLogins(new Query { Filter = $@"i => i.LoginProvider.Contains(@0) || i.ProviderKey.Contains(@0) || i.ProviderDisplayName.Contains(@0) || i.UserId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetUserLogins = await dataService.GetAspNetUserLogins(new Query { Filter = $@"i => i.LoginProvider.Contains(@0) || i.ProviderKey.Contains(@0) || i.ProviderDisplayName.Contains(@0) || i.UserId.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetUserLogin>("Добавить логин пользователя", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetUserLogin> args)
        {
            await DialogService.OpenAsync<EditAspNetUserLogin>("Редактировать пользователя", new Dictionary<string, object> { {"LoginProvider", args.Data.LoginProvider}, {"ProviderKey", args.Data.ProviderKey} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetUserLogin aspNetUserLogin)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetUserLogin(aspNetUserLogin.LoginProvider, aspNetUserLogin.ProviderKey);

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
                    Detail = $"Не удается удалить пользователя Asp Net"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportAspNetUserLoginsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserLogins");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetUserLoginsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserLogins");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetUserLogin aspNetUserLogin)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран логин пользователя:";
            message.Detail = $"{aspNetUserLogin.LoginProvider}";
            NotificationService.Notify(message);
        }
    }
}