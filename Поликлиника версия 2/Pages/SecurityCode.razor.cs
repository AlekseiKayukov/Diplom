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
    public partial class SecurityCode
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

        async Task VerifySecurityCode(string code)
        {
            if (code.Count() == 6)
            {
                await JSRuntime.InvokeVoidAsync("eval", "document.forms[0].submit()");
            }
        }

        string message;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var uri = new Uri(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString());
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            message = $"Мы отправили проверочный код по адресу {query.Get("email")}. Введите код из приведенного ниже электронного письма.";
        }

        RadzenSecurityCode sc;

        [Inject]
        protected SecurityService Security { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await sc.FocusAsync();
            }
        }
    }
}
