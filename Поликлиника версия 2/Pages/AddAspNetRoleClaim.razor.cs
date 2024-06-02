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
    public partial class AddAspNetRoleClaim
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
            aspNetRoleClaim = new Polyclinic.Models.data.AspNetRoleClaim();

            aspNetRolesForRoleId = await dataService.GetAspNetRoles();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.AspNetRoleClaim aspNetRoleClaim;

        protected IEnumerable<Polyclinic.Models.data.AspNetRole> aspNetRolesForRoleId;

        protected async Task FormSubmit()
        {
            try
            {
                await dataService.CreateAspNetRoleClaim(aspNetRoleClaim);
                DialogService.Close(aspNetRoleClaim);
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