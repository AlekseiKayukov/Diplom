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
    public partial class ApplicationRoles
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

        protected IEnumerable<Polyclinic.Models.ApplicationRole> roles;
        protected RadzenDataGrid<Polyclinic.Models.ApplicationRole> grid0;
        protected string error;
        protected bool errorVisible;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            roles = await Security.GetRoles();
        }

        protected async Task AddClick()
        {
            await DialogService.OpenAsync<AddApplicationRole>("Добавить роль приложения");

            roles = await Security.GetRoles();
        }

        protected async Task DeleteClick(Polyclinic.Models.ApplicationRole role)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту роль?") == true)
                {
                    await Security.DeleteRole($"{role.Id}");

                    roles = await Security.GetRoles();
                }
            }
            catch (Exception ex)
            {
                errorVisible = true;
                error = ex.Message;
            }
        }

        protected async Task ClickDataGridRow(Polyclinic.Models.ApplicationRole role)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрана роль:";
            message.Detail = $"{role.Name}";
            NotificationService.Notify(message);
        }

    }
}