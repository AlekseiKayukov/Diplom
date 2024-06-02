using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using Polyclinic.Data;

namespace Polyclinic.Controllers
{
    public partial class ExportdataController : ExportController
    {
        private readonly dataContext context;
        private readonly dataService service;

        public ExportdataController(dataContext context, dataService service)
        {
            this.service = service;
            this.context = context;
        }

        [HttpGet("/export/data/diagnoses/csv")]
        [HttpGet("/export/data/diagnoses/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDiagnosesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDiagnoses(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/diagnoses/excel")]
        [HttpGet("/export/data/diagnoses/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDiagnosesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDiagnoses(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/doctors/csv")]
        [HttpGet("/export/data/doctors/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDoctorsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetDoctors(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/doctors/excel")]
        [HttpGet("/export/data/doctors/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportDoctorsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetDoctors(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/genders/csv")]
        [HttpGet("/export/data/genders/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportGendersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetGenders(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/genders/excel")]
        [HttpGet("/export/data/genders/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportGendersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetGenders(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/patients/csv")]
        [HttpGet("/export/data/patients/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportPatientsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetPatients(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/patients/excel")]
        [HttpGet("/export/data/patients/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportPatientsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetPatients(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/records/csv")]
        [HttpGet("/export/data/records/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRecordsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetRecords(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/records/excel")]
        [HttpGet("/export/data/records/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportRecordsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetRecords(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/schedules/csv")]
        [HttpGet("/export/data/schedules/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSchedulesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetSchedules(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/schedules/excel")]
        [HttpGet("/export/data/schedules/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSchedulesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetSchedules(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/specializations/csv")]
        [HttpGet("/export/data/specializations/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSpecializationsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetSpecializations(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/specializations/excel")]
        [HttpGet("/export/data/specializations/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportSpecializationsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetSpecializations(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/visits/csv")]
        [HttpGet("/export/data/visits/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportVisitsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetVisits(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/visits/excel")]
        [HttpGet("/export/data/visits/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportVisitsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetVisits(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetroleclaims/csv")]
        [HttpGet("/export/data/aspnetroleclaims/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetRoleClaimsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetRoleClaims(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetroleclaims/excel")]
        [HttpGet("/export/data/aspnetroleclaims/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetRoleClaimsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetRoleClaims(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetroles/csv")]
        [HttpGet("/export/data/aspnetroles/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetRolesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetRoles(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetroles/excel")]
        [HttpGet("/export/data/aspnetroles/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetRolesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetRoles(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserclaims/csv")]
        [HttpGet("/export/data/aspnetuserclaims/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserClaimsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetUserClaims(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserclaims/excel")]
        [HttpGet("/export/data/aspnetuserclaims/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserClaimsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetUserClaims(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserlogins/csv")]
        [HttpGet("/export/data/aspnetuserlogins/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserLoginsToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetUserLogins(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserlogins/excel")]
        [HttpGet("/export/data/aspnetuserlogins/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserLoginsToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetUserLogins(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserroles/csv")]
        [HttpGet("/export/data/aspnetuserroles/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserRolesToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetUserRoles(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetuserroles/excel")]
        [HttpGet("/export/data/aspnetuserroles/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserRolesToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetUserRoles(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetusers/csv")]
        [HttpGet("/export/data/aspnetusers/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUsersToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetUsers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetusers/excel")]
        [HttpGet("/export/data/aspnetusers/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUsersToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetUsers(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetusertokens/csv")]
        [HttpGet("/export/data/aspnetusertokens/csv(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserTokensToCSV(string fileName = null)
        {
            return ToCSV(ApplyQuery(await service.GetAspNetUserTokens(), Request.Query, false), fileName);
        }

        [HttpGet("/export/data/aspnetusertokens/excel")]
        [HttpGet("/export/data/aspnetusertokens/excel(fileName='{fileName}')")]
        public async Task<FileStreamResult> ExportAspNetUserTokensToExcel(string fileName = null)
        {
            return ToExcel(ApplyQuery(await service.GetAspNetUserTokens(), Request.Query, false), fileName);
        }
    }
}
