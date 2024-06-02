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
    public partial class AddDoctor
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
            doctor = new Polyclinic.Models.data.Doctor();

            gendersForIdGender = await dataService.GetGenders();

            specializationsForIdSpecialization = await dataService.GetSpecializations();

            aspNetUsersForIdUser = await dataService.GetAspNetUsers();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.Doctor doctor;

        protected IEnumerable<Polyclinic.Models.data.Gender> gendersForIdGender;

        protected IEnumerable<Polyclinic.Models.data.Specialization> specializationsForIdSpecialization;

        protected IEnumerable<Polyclinic.Models.data.AspNetUser> aspNetUsersForIdUser;

        protected async Task FormSubmit()
        {
            try
            {
                await dataService.CreateDoctor(doctor);
                DialogService.Close(doctor);
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