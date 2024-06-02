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
    public partial class AddAspNetUserLogin
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

        protected override async Task OnInitializedAsync()
        {
            aspNetUserLogin = new Polyclinic.Models.data.AspNetUserLogin();

            aspNetUsersForUserId = await dataService.GetAspNetUsers();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.AspNetUserLogin aspNetUserLogin;

        protected IEnumerable<Polyclinic.Models.data.AspNetUser> aspNetUsersForUserId;

        protected async Task FormSubmit()
        {
            try
            {
                await dataService.CreateAspNetUserLogin(aspNetUserLogin);
                DialogService.Close(aspNetUserLogin);
            }
            catch (Exception ex)
            {
                hasChanges = ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
                canEdit = !(ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException);
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }


        protected bool hasChanges = false;
        protected bool canEdit = true;

        [Inject]
        protected SecurityService Security { get; set; }
    }
}