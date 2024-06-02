using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace Polyclinic.Pages
{
    public partial class Index
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
        protected SecurityService Security { get; set; }

        int zoom = 3;
        RadzenGoogleMap map;

        async Task OnMarkerClick(RadzenGoogleMapMarker marker)
        {
            var message = $"Custom information about <b>{marker.Title}</b>";

            var code = $@"
            var map = Radzen['{map.UniqueID}'].instance;
            var marker = map.markers.find(m => m.title == '{marker.Title}');
            if(window.infoWindow) {{window.infoWindow.close();}}
            window.infoWindow = new google.maps.InfoWindow({{content: '{message}'}});
            setTimeout(() => window.infoWindow.open(map, marker), 200);
            ";

            await JSRuntime.InvokeVoidAsync("eval", code);
        }
    }
}