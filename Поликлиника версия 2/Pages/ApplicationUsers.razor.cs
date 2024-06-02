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
    public partial class ApplicationUsers
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

        protected IEnumerable<Polyclinic.Models.ApplicationUser> users;
        protected RadzenDataGrid<Polyclinic.Models.ApplicationUser> grid0;
        protected string error;
        protected bool errorVisible;

        [Inject]
        protected SecurityService Security { get; set; }

        protected override async Task OnInitializedAsync()
        {
            users = await Security.GetUsers();
        }

        protected async Task AddClick()
        {
            await DialogService.OpenAsync<AddApplicationUser>("Добавить пользователя приложения");

            users = await Security.GetUsers();
        }

        protected async Task RowSelect(Polyclinic.Models.ApplicationUser user)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбран пользователь:";
            message.Detail = $"{user.Email}";
            NotificationService.Notify(message);

            await DialogService.OpenAsync<EditApplicationUser>("Редактировать пользователя приложения", new Dictionary<string, object>{ {"Id", user.Id} });

            users = await Security.GetUsers();
        }

        protected async Task DeleteClick(Polyclinic.Models.ApplicationUser user)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить этого пользователя?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    await Security.DeleteUser($"{user.Id}");

                    users = await Security.GetUsers();
                }
            }
            catch (Exception ex)
            {
                errorVisible = true;
                error = ex.Message;
            }
        }
    }
}