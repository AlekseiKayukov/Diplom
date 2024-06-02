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
    public partial class AspNetUserRoles
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

        protected IEnumerable<Polyclinic.Models.data.AspNetUserRole> aspNetUserRoles;

        protected RadzenDataGrid<Polyclinic.Models.data.AspNetUserRole> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            aspNetUserRoles = await dataService.GetAspNetUserRoles(new Query { Filter = $@"i => i.AspNetUser.Email.Contains(@0) || i.AspNetRole.Name.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser,AspNetRole" });
        }
        protected override async Task OnInitializedAsync()
        {
            aspNetUserRoles = await dataService.GetAspNetUserRoles(new Query { Filter = $@"i => i.AspNetUser.Email.Contains(@0) || i.AspNetRole.Name.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AspNetUser,AspNetRole" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAspNetUserRole>("Добавить пользователю роль", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.AspNetUserRole> args)
        {
            await DialogService.OpenAsync<EditAspNetUserRole>("Редактировать пользователю роль", new Dictionary<string, object> { {"UserId", args.Data.UserId}, {"RoleId", args.Data.RoleId} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.AspNetUserRole aspNetUserRole)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteAspNetUserRole(aspNetUserRole.UserId, aspNetUserRole.RoleId);

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
                await dataService.ExportAspNetUserRolesToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser,AspNetRole",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserRoles");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportAspNetUserRolesToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "AspNetUser,AspNetRole",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "AspNetUserRoles");
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.data.AspNetUserRole aspNetUserRole)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран пользователь:";
            message.Detail = $"{aspNetUserRole.AspNetUser.Email}";
            NotificationService.Notify(message);
        }
    }
}