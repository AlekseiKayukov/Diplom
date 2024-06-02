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
    public partial class AddSchedule
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
            schedule = new Polyclinic.Models.data.Schedule();

            doctorsForIdDoctor = await dataService.GetDoctors();
        }
        protected bool errorVisible;
        protected Polyclinic.Models.data.Schedule schedule;

        protected IEnumerable<Polyclinic.Models.data.Doctor> doctorsForIdDoctor;

        protected async Task FormSubmit()
        {
            try
            {
                List<Polyclinic.Models.data.Schedule> createdSchedules = await dataService.CreateSchedules(new List<Polyclinic.Models.data.Schedule> { schedule });
                foreach (var createdSchedule in createdSchedules)
                {
                    DialogService.Close(createdSchedule);
                }
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