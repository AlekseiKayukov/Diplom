using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Word = Microsoft.Office.Interop.Word;

namespace Polyclinic.Pages
{
    public partial class Visits
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

        protected IEnumerable<Polyclinic.Models.data.Visit> visits;

        protected RadzenDataGrid<Polyclinic.Models.data.Visit> grid0;

        protected string search = "";

        [Inject]
        protected SecurityService Security { get; set; }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            visits = await dataService.GetVisits(new Query { Filter = $@"i => i.Complaint.Contains(@0) || i.Appointments.Contains(@0) || i.Status.Contains(@0) 
            || i.Diagnosis.Title.Contains(@0) 
            || i.Record.Patient.Surname.Contains(@0)
            || i.Record.Patient.Name.Contains(@0)
            || i.Record.Patient.Forename.Contains(@0)
            || i.Record.Schedule.Doctor.Surname.Contains(@0)
            || i.Record.Schedule.Doctor.Name.Contains(@0)
            || i.Record.Schedule.Doctor.Forename.Contains(@0)
            || i.Record.Schedule.Cabinet.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Diagnosis,Record" });
        }
        protected override async Task OnInitializedAsync()
        {
            visits = await dataService.GetVisits(new Query { Filter = $@"i => i.Complaint.Contains(@0) || i.Appointments.Contains(@0) || i.Status.Contains(@0) 
            || i.Diagnosis.Title.Contains(@0) 
            || i.Record.Patient.Surname.Contains(@0)
            || i.Record.Patient.Name.Contains(@0)
            || i.Record.Patient.Forename.Contains(@0)
            || i.Record.Schedule.Doctor.Surname.Contains(@0)
            || i.Record.Schedule.Doctor.Name.Contains(@0)
            || i.Record.Schedule.Doctor.Forename.Contains(@0)
            || i.Record.Schedule.Cabinet.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Diagnosis,Record" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddVisit>("Добавить посещение", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Polyclinic.Models.data.Visit> args)
        {
            await DialogService.OpenAsync<EditVisit>("Редактировать посещение", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Polyclinic.Models.data.Visit visit)
        {
            try
            {
                if (await DialogService.Confirm("Вы уверены, что хотите удалить эту запись?","Подтверждение", new ConfirmOptions{OkButtonText = "Да", CancelButtonText = "Нет"}) == true)
                {
                    var deleteResult = await dataService.DeleteVisit(visit.Id);

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
                    Detail = $"Не удается удалить посещение"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await dataService.ExportVisitsToCSV(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Diagnosis,Record",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Visits");
            }

            if (args == null || args.Value == "xlsx")
            {
                await dataService.ExportVisitsToExcel(new Query
{
    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
    OrderBy = $"{grid0.Query.OrderBy}",
    Expand = "Diagnosis,Record",
    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
}, "Visits");
            }
        }
        protected Polyclinic.Models.data.Visit visitsExam;
        protected async Task ClickDataGridRow(Polyclinic.Models.data.Visit visit)
        {
            var message = new NotificationMessage();
            message.Summary = "Выбрано посещение пациента:";
            message.Detail = $"{visit.Record.Patient.Surname} {visit.Record.Patient.Name} {visit.Record.Patient.Forename}";
            NotificationService.Notify(message);
            visitsExam = visit;
        }
        
        protected async Task CreateListExam(MouseEventArgs args)
        {
            //try
            //{
            if (await DialogService.Confirm("Вы уверены, что хотите сформировать осмотр?", 
                    "Подтверждение", new ConfirmOptions { OkButtonText = "Да", CancelButtonText = "Нет" }) == true)
                {
                    if (visitsExam == null)
                    {
                        var message = new NotificationMessage();
                        message.Summary = "Не выбрана строка для формирования осмотра!";
                        message.Severity= NotificationSeverity.Warning;
                        NotificationService.Notify(message);
                        return;
                    }
                    else
                    {
                        // открытие шаблона документа
                        var items = new Dictionary<string, string>
                        { // осуществление замены данных в документе на соответствующие значения
                            {"<DATE>", visitsExam.DateVisit.ToShortDateString()+" "+ visitsExam.TimeVisit.ToShortTimeString()},
                            {"<FULLNAMEPATIENT>", visitsExam.Record.Patient.PatientFullName},
                            {"<NUMBERCART>", visitsExam.Record.Patient.NumberCard.ToString()},
                            {"<DATEOFBIRTH>", visitsExam.Record.Patient.DateOfBirth.ToShortDateString()},
                            {"<DIAGNOSIS>", visitsExam.Diagnosis.Title},
                            {"<RECEPTION>", visitsExam.DateVisit.ToShortDateString()+" "+ visitsExam.TimeVisit.ToShortTimeString()},
                            {"<COMPLAIN>", visitsExam.Complaint},
                            {"<EXAM>", visitsExam.Appointments},
                            {"<FULLNAMEDOCTOR>", visitsExam.Record.Schedule.Doctor.DoctorFullName}
                        };
                            Process(items, visitsExam.Record.Patient.PatientFullName.ToString());
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    NotificationService.Notify(new NotificationMessage
            //    {
            //        Severity = NotificationSeverity.Error,
            //        Summary = $"Ошибка",
            //        Detail = $"Не удается сформировать осмотр"
            //    });
            //}
        }

        private System.IO.FileInfo _fileInfo;
        internal bool Process(Dictionary<string, string> items, string SurnameP)
        {
            _fileInfo = new System.IO.FileInfo($@"{Environment.CurrentDirectory}\\bin\\Debug\\net7.0\\rezultat_osmotra.docx");
            Word.Application app = null; // Создаем файл приложения WORD
            try
            {   
                app = new Word.Application(); // задаем имя
                Object file = _fileInfo.FullName;
                Object missing = Type.Missing;
                app.Documents.Open(file); // Открываем файл
                foreach (var item in items)
                {
                    Word.Find find = app.Selection.Find; // осуществляем поиск в файле
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;
                    Object wrap = Word.WdFindWrap.wdFindContinue;
                    Object replace = Word.WdReplace.wdReplaceAll; // задаем определенные фильтры
                    find.Execute(FindText: Type.Missing,
                        MatchCase: false, MatchWholeWord: false,
                        MatchWildcards: false, MatchSoundsLike: missing,
                        MatchAllWordForms: false,
                        Forward: true, Wrap: wrap,
                        Format: false, ReplaceWith: missing, Replace: replace);
                }
                Object newFileName = System.IO.Path.Combine(@$"{Environment.CurrentDirectory}\Осмотры пациентов\",SurnameP +" "+ DateTime.Now.TimeOfDay.TotalDays.ToString() + " " + _fileInfo.Name);
                app.ActiveDocument.SaveAs2(newFileName);
                app.ActiveDocument.Close(); // закрытие файла
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally
            {
                if (app != null)
               {
                    app.Quit(); // Завершение процесса записи
                    var message = new NotificationMessage();
                    message.Summary = "Осмотр успешно сформирован!";
                    message.Severity = NotificationSeverity.Success;
                    NotificationService.Notify(message);
                }
            }
            return false;
        }
    }
}