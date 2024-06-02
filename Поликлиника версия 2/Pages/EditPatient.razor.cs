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
    public partial class EditPatient
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

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            patient = await dataService.GetPatientById(Id);

            aspNetUsersForIdUser = await dataService.GetAspNetUsers();

            gendersForIdGender = await dataService.GetGenders();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.Patient patient;

        protected IEnumerable<Polyclinic.Models.data.AspNetUser> aspNetUsersForIdUser;

        protected IEnumerable<Polyclinic.Models.data.Gender> gendersForIdGender;

        protected async Task FormSubmit()
        {
            try
            {
                await dataService.UpdatePatient(Id, patient);
                DialogService.Close(patient);
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
        protected DateTime CurrentDate = DateTime.Now;

        protected async Task ReloadButtonClick(MouseEventArgs args)
        {
           dataService.Reset();
            hasChanges = false;
            canEdit = true;

            patient = await dataService.GetPatientById(Id);
        }
    }
}