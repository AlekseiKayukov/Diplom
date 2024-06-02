using System;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;

using Polyclinic.Data;

namespace Polyclinic
{
    public partial class dataService
    {
        dataContext Context
        {
           get
           {
             return this.context;
           }
        }

        private readonly dataContext context;
        private readonly NavigationManager navigationManager;

        public dataService(dataContext context, NavigationManager navigationManager)
        {
            this.context = context;
            this.navigationManager = navigationManager;
        }

        public async Task<Polyclinic.Models.data.Patient> GetPatientByEmailAsync(long patientId)
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.Id == Convert.ToInt32(patientId));
            return patient;
        }

        public void Reset() => Context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public void ApplyQuery<T>(ref IQueryable<T> items, Query query = null)
        {
            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    if (query.FilterParameters != null)
                    {
                        items = items.Where(query.Filter, query.FilterParameters);
                    }
                    else
                    {
                        items = items.Where(query.Filter);
                    }
                }

                if (!string.IsNullOrEmpty(query.OrderBy))
                {
                    items = items.OrderBy(query.OrderBy);
                }

                if (query.Skip.HasValue)
                {
                    items = items.Skip(query.Skip.Value);
                }

                if (query.Top.HasValue)
                {
                    items = items.Take(query.Top.Value);
                }
            }
        }


        public async Task ExportDiagnosesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/diagnoses/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/diagnoses/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDiagnosesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/diagnoses/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/diagnoses/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDiagnosesRead(ref IQueryable<Polyclinic.Models.data.Diagnosis> items);

        public async Task<IQueryable<Polyclinic.Models.data.Diagnosis>> GetDiagnoses(Query query = null)
        {
            var items = Context.Diagnoses.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnDiagnosesRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnDiagnosisGet(Polyclinic.Models.data.Diagnosis item);
        partial void OnGetDiagnosisById(ref IQueryable<Polyclinic.Models.data.Diagnosis> items);


        public async Task<Polyclinic.Models.data.Diagnosis> GetDiagnosisById(long id)
        {
            var items = Context.Diagnoses
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetDiagnosisById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDiagnosisGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDiagnosisCreated(Polyclinic.Models.data.Diagnosis item);
        partial void OnAfterDiagnosisCreated(Polyclinic.Models.data.Diagnosis item);

        public async Task<Polyclinic.Models.data.Diagnosis> CreateDiagnosis(Polyclinic.Models.data.Diagnosis diagnosis)
        {
            OnDiagnosisCreated(diagnosis);

            var existingItem = Context.Diagnoses
                              .Where(i => i.Id == diagnosis.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Diagnoses.Add(diagnosis);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(diagnosis).State = EntityState.Detached;
                throw;
            }

            OnAfterDiagnosisCreated(diagnosis);

            return diagnosis;
        }

        public async Task<Polyclinic.Models.data.Diagnosis> CancelDiagnosisChanges(Polyclinic.Models.data.Diagnosis item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnDiagnosisUpdated(Polyclinic.Models.data.Diagnosis item);
        partial void OnAfterDiagnosisUpdated(Polyclinic.Models.data.Diagnosis item);

        public async Task<Polyclinic.Models.data.Diagnosis> UpdateDiagnosis(long id, Polyclinic.Models.data.Diagnosis diagnosis)
        {
            OnDiagnosisUpdated(diagnosis);

            var itemToUpdate = Context.Diagnoses
                              .Where(i => i.Id == diagnosis.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(diagnosis);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDiagnosisUpdated(diagnosis);

            return diagnosis;
        }

        partial void OnDiagnosisDeleted(Polyclinic.Models.data.Diagnosis item);
        partial void OnAfterDiagnosisDeleted(Polyclinic.Models.data.Diagnosis item);

        public async Task<Polyclinic.Models.data.Diagnosis> DeleteDiagnosis(long id)
        {
            var itemToDelete = Context.Diagnoses
                              .Where(i => i.Id == id)
                              .Include(i => i.Visits)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnDiagnosisDeleted(itemToDelete);


            Context.Diagnoses.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDiagnosisDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportDoctorsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/doctors/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/doctors/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDoctorsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/doctors/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/doctors/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDoctorsRead(ref IQueryable<Polyclinic.Models.data.Doctor> items);

        public async Task<IQueryable<Polyclinic.Models.data.Doctor>> GetDoctors(Query query = null)
        {
            var items = Context.Doctors.AsQueryable();

            items = items.Include(i => i.Gender);
            items = items.Include(i => i.Specialization);
            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnDoctorsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnDoctorGet(Polyclinic.Models.data.Doctor item);
        partial void OnGetDoctorById(ref IQueryable<Polyclinic.Models.data.Doctor> items);


        public async Task<Polyclinic.Models.data.Doctor> GetDoctorById(long id)
        {
            var items = Context.Doctors
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Gender);
            items = items.Include(i => i.Specialization);
            items = items.Include(i => i.AspNetUser);
 
            OnGetDoctorById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDoctorGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDoctorCreated(Polyclinic.Models.data.Doctor item);
        partial void OnAfterDoctorCreated(Polyclinic.Models.data.Doctor item);

        public async Task<Polyclinic.Models.data.Doctor> CreateDoctor(Polyclinic.Models.data.Doctor doctor)
        {
            OnDoctorCreated(doctor);

            var existingItem = Context.Doctors
                              .Where(i => i.Id == doctor.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Doctors.Add(doctor);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(doctor).State = EntityState.Detached;
                throw;
            }

            OnAfterDoctorCreated(doctor);

            return doctor;
        }

        public async Task<Polyclinic.Models.data.Doctor> CancelDoctorChanges(Polyclinic.Models.data.Doctor item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnDoctorUpdated(Polyclinic.Models.data.Doctor item);
        partial void OnAfterDoctorUpdated(Polyclinic.Models.data.Doctor item);

        public async Task<Polyclinic.Models.data.Doctor> UpdateDoctor(long id, Polyclinic.Models.data.Doctor doctor)
        {
            OnDoctorUpdated(doctor);

            var itemToUpdate = Context.Doctors
                              .Where(i => i.Id == doctor.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(doctor);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDoctorUpdated(doctor);

            return doctor;
        }

        partial void OnDoctorDeleted(Polyclinic.Models.data.Doctor item);
        partial void OnAfterDoctorDeleted(Polyclinic.Models.data.Doctor item);

        public async Task<Polyclinic.Models.data.Doctor> DeleteDoctor(long id)
        {
            var itemToDelete = Context.Doctors
                              .Where(i => i.Id == id)
                              .Include(i => i.Schedules)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnDoctorDeleted(itemToDelete);


            Context.Doctors.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDoctorDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportGendersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/genders/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/genders/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportGendersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/genders/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/genders/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnGendersRead(ref IQueryable<Polyclinic.Models.data.Gender> items);

        public async Task<IQueryable<Polyclinic.Models.data.Gender>> GetGenders(Query query = null)
        {
            var items = Context.Genders.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnGendersRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnGenderGet(Polyclinic.Models.data.Gender item);
        partial void OnGetGenderById(ref IQueryable<Polyclinic.Models.data.Gender> items);


        public async Task<Polyclinic.Models.data.Gender> GetGenderById(long id)
        {
            var items = Context.Genders
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetGenderById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnGenderGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnGenderCreated(Polyclinic.Models.data.Gender item);
        partial void OnAfterGenderCreated(Polyclinic.Models.data.Gender item);

        public async Task<Polyclinic.Models.data.Gender> CreateGender(Polyclinic.Models.data.Gender gender)
        {
            OnGenderCreated(gender);

            var existingItem = Context.Genders
                              .Where(i => i.Id == gender.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Genders.Add(gender);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(gender).State = EntityState.Detached;
                throw;
            }

            OnAfterGenderCreated(gender);

            return gender;
        }

        public async Task<Polyclinic.Models.data.Gender> CancelGenderChanges(Polyclinic.Models.data.Gender item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnGenderUpdated(Polyclinic.Models.data.Gender item);
        partial void OnAfterGenderUpdated(Polyclinic.Models.data.Gender item);

        public async Task<Polyclinic.Models.data.Gender> UpdateGender(long id, Polyclinic.Models.data.Gender gender)
        {
            OnGenderUpdated(gender);

            var itemToUpdate = Context.Genders
                              .Where(i => i.Id == gender.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(gender);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterGenderUpdated(gender);

            return gender;
        }

        partial void OnGenderDeleted(Polyclinic.Models.data.Gender item);
        partial void OnAfterGenderDeleted(Polyclinic.Models.data.Gender item);

        public async Task<Polyclinic.Models.data.Gender> DeleteGender(long id)
        {
            var itemToDelete = Context.Genders
                              .Where(i => i.Id == id)
                              .Include(i => i.Doctors)
                              .Include(i => i.Patients)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnGenderDeleted(itemToDelete);


            Context.Genders.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterGenderDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportPatientsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/patients/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/patients/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportPatientsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/patients/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/patients/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnPatientsRead(ref IQueryable<Polyclinic.Models.data.Patient> items);

        public async Task<IQueryable<Polyclinic.Models.data.Patient>> GetPatients(Query query = null)
        {
            var items = Context.Patients.AsQueryable();

            items = items.Include(i => i.Gender);
            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnPatientsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnPatientGet(Polyclinic.Models.data.Patient item);
        partial void OnGetPatientById(ref IQueryable<Polyclinic.Models.data.Patient> items);


        public async Task<Polyclinic.Models.data.Patient> GetPatientById(long id)
        {
            var items = Context.Patients
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Gender);
            items = items.Include(i => i.AspNetUser);
 
            OnGetPatientById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnPatientGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnPatientCreated(Polyclinic.Models.data.Patient item);
        partial void OnAfterPatientCreated(Polyclinic.Models.data.Patient item);

        public async Task<Polyclinic.Models.data.Patient> CreatePatient(Polyclinic.Models.data.Patient patient)
        {
            OnPatientCreated(patient);

            var existingItem = Context.Patients
                              .Where(i => i.Id == patient.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Patients.Add(patient);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(patient).State = EntityState.Detached;
                throw;
            }

            OnAfterPatientCreated(patient);

            return patient;
        }

        public async Task<Polyclinic.Models.data.Patient> CancelPatientChanges(Polyclinic.Models.data.Patient item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnPatientUpdated(Polyclinic.Models.data.Patient item);
        partial void OnAfterPatientUpdated(Polyclinic.Models.data.Patient item);

        public async Task<Polyclinic.Models.data.Patient> UpdatePatient(long id, Polyclinic.Models.data.Patient patient)
        {
            OnPatientUpdated(patient);

            var itemToUpdate = Context.Patients
                              .Where(i => i.Id == patient.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(patient);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterPatientUpdated(patient);

            return patient;
        }

        partial void OnPatientDeleted(Polyclinic.Models.data.Patient item);
        partial void OnAfterPatientDeleted(Polyclinic.Models.data.Patient item);

        public async Task<Polyclinic.Models.data.Patient> DeletePatient(long id)
        {
            var itemToDelete = Context.Patients
                              .Where(i => i.Id == id)
                              .Include(i => i.Records)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnPatientDeleted(itemToDelete);


            Context.Patients.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterPatientDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportRecordsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/records/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/records/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportRecordsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/records/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/records/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnRecordsRead(ref IQueryable<Polyclinic.Models.data.Record> items);

        public async Task<IQueryable<Polyclinic.Models.data.Record>> GetRecords(Query query = null)
        {
            var items = Context.Records.AsQueryable();

            items = items.Include(i => i.Patient);
            items = items.Include(i => i.Schedule);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnRecordsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnRecordGet(Polyclinic.Models.data.Record item);
        partial void OnGetRecordById(ref IQueryable<Polyclinic.Models.data.Record> items);


        public async Task<Polyclinic.Models.data.Record> GetRecordById(long id)
        {
            var items = Context.Records
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Patient);
            items = items.Include(i => i.Schedule);
 
            OnGetRecordById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnRecordGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnRecordCreated(Polyclinic.Models.data.Record item);
        partial void OnAfterRecordCreated(Polyclinic.Models.data.Record item);

        public async Task<Polyclinic.Models.data.Record> CreateRecord(Polyclinic.Models.data.Record record)
        {
            OnRecordCreated(record);

            var existingItem = Context.Records
                            .Where(i => i.Id == record.Id)
                            .FirstOrDefault();

            if (existingItem != null)
            {
                throw new Exception("Item already available");
            }            

            try
            {
                Context.Records.Add(record);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(record).State = EntityState.Detached;
                throw;
            }

            OnAfterRecordCreated(record);

            return record;
        }
        public async Task<Polyclinic.Models.data.Record> CancelRecordChanges(Polyclinic.Models.data.Record item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnRecordUpdated(Polyclinic.Models.data.Record item);
        partial void OnAfterRecordUpdated(Polyclinic.Models.data.Record item);

        public async Task<Polyclinic.Models.data.Record> UpdateRecord(long id, Polyclinic.Models.data.Record record)
        {
            OnRecordUpdated(record);

            var itemToUpdate = Context.Records
                              .Where(i => i.Id == record.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(record);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterRecordUpdated(record);

            return record;
        }

        partial void OnRecordDeleted(Polyclinic.Models.data.Record item);
        partial void OnAfterRecordDeleted(Polyclinic.Models.data.Record item);

        public async Task<Polyclinic.Models.data.Record> DeleteRecord(long id)
        {
            var itemToDelete = Context.Records
                              .Where(i => i.Id == id)
                              .Include(i => i.Visits)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnRecordDeleted(itemToDelete);


            Context.Records.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterRecordDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportSchedulesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/schedules/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/schedules/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportSchedulesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/schedules/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/schedules/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnSchedulesRead(ref IQueryable<Polyclinic.Models.data.Schedule> items);

        public async Task<IQueryable<Polyclinic.Models.data.Schedule>> GetSchedules(Query query = null)
        {
            var items = Context.Schedules.AsQueryable();

            items = items.Include(i => i.Doctor);
            items = items.Include(i => i.Doctor.Specialization);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnSchedulesRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnScheduleGet(Polyclinic.Models.data.Schedule item);
        partial void OnGetScheduleById(ref IQueryable<Polyclinic.Models.data.Schedule> items);


        public async Task<Polyclinic.Models.data.Schedule> GetScheduleById(long id)
        {
            var items = Context.Schedules
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Doctor);
 
            OnGetScheduleById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnScheduleGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnScheduleCreated(Polyclinic.Models.data.Schedule item);
        partial void OnAfterScheduleCreated(Polyclinic.Models.data.Schedule item);

        public async Task<List<Polyclinic.Models.data.Schedule>> CreateSchedules(List<Polyclinic.Models.data.Schedule> schedules)
        {
            List<Polyclinic.Models.data.Schedule> createdSchedules = new List<Polyclinic.Models.data.Schedule>();

            foreach (var schedule in schedules)
            {
                OnScheduleCreated(schedule);

                var existingItem = Context.Schedules
                                .Where(i => i.Id == schedule.Id)
                                .FirstOrDefault();

                if (existingItem != null)
                {
                    throw new Exception("Item already available");
                }

                try
                {
                    // Add the initial schedule to the list of created schedules
                    


                    DateTime currentTime = schedule.TimeStart;
                    while (currentTime < schedule.TimeEnd)
                    {
                        var newSchedule = new Polyclinic.Models.data.Schedule
                        {
                            DateStart = schedule.DateStart,
                            TimeStart = currentTime,
                            TimeEnd = currentTime.AddMinutes(Convert.ToDouble(schedule.Duration)),
                            Duration = schedule.Duration,
                            Cabinet = schedule.Cabinet,
                            IdDoctor = schedule.IdDoctor
                        };
                        
                        if (newSchedule.TimeEnd > schedule.TimeEnd)
                        {
                            newSchedule.TimeEnd = schedule.TimeEnd;
                        }
                        
                        Context.Schedules.Add(newSchedule);
                        createdSchedules.Add(newSchedule);

                        currentTime = newSchedule.TimeEnd;
                    }
                    Context.SaveChanges();
                    Context.Schedules.Add(schedule);
                    createdSchedules.Add(schedule);
                }
                catch
                {
                    foreach (var createdSchedule in createdSchedules)
                    {
                        Context.Entry(createdSchedule).State = EntityState.Detached;
                    }
                    throw;
                }

                OnAfterScheduleCreated(schedule);
            }

            return createdSchedules;
        }

        public async Task<Polyclinic.Models.data.Schedule> CancelScheduleChanges(Polyclinic.Models.data.Schedule item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnScheduleUpdated(Polyclinic.Models.data.Schedule item);
        partial void OnAfterScheduleUpdated(Polyclinic.Models.data.Schedule item);

        public async Task<Polyclinic.Models.data.Schedule> UpdateSchedule(long id, Polyclinic.Models.data.Schedule schedule)
        {
            OnScheduleUpdated(schedule);

            var itemToUpdate = Context.Schedules
                              .Where(i => i.Id == schedule.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(schedule);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterScheduleUpdated(schedule);

            return schedule;
        }

        partial void OnScheduleDeleted(Polyclinic.Models.data.Schedule item);
        partial void OnAfterScheduleDeleted(Polyclinic.Models.data.Schedule item);

        public async Task<Polyclinic.Models.data.Schedule> DeleteSchedule(long id)
        {
            var itemToDelete = Context.Schedules
                              .Where(i => i.Id == id)
                              .Include(i => i.Records)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnScheduleDeleted(itemToDelete);


            Context.Schedules.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterScheduleDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportSpecializationsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/specializations/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/specializations/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportSpecializationsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/specializations/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/specializations/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnSpecializationsRead(ref IQueryable<Polyclinic.Models.data.Specialization> items);

        public async Task<IQueryable<Polyclinic.Models.data.Specialization>> GetSpecializations(Query query = null)
        {
            var items = Context.Specializations.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnSpecializationsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnSpecializationGet(Polyclinic.Models.data.Specialization item);
        partial void OnGetSpecializationById(ref IQueryable<Polyclinic.Models.data.Specialization> items);


        public async Task<Polyclinic.Models.data.Specialization> GetSpecializationById(long id)
        {
            var items = Context.Specializations
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetSpecializationById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnSpecializationGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnSpecializationCreated(Polyclinic.Models.data.Specialization item);
        partial void OnAfterSpecializationCreated(Polyclinic.Models.data.Specialization item);

        public async Task<Polyclinic.Models.data.Specialization> CreateSpecialization(Polyclinic.Models.data.Specialization specialization)
        {
            OnSpecializationCreated(specialization);

            var existingItem = Context.Specializations
                              .Where(i => i.Id == specialization.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Specializations.Add(specialization);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(specialization).State = EntityState.Detached;
                throw;
            }

            OnAfterSpecializationCreated(specialization);

            return specialization;
        }

        public async Task<Polyclinic.Models.data.Specialization> CancelSpecializationChanges(Polyclinic.Models.data.Specialization item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnSpecializationUpdated(Polyclinic.Models.data.Specialization item);
        partial void OnAfterSpecializationUpdated(Polyclinic.Models.data.Specialization item);

        public async Task<Polyclinic.Models.data.Specialization> UpdateSpecialization(long id, Polyclinic.Models.data.Specialization specialization)
        {
            OnSpecializationUpdated(specialization);

            var itemToUpdate = Context.Specializations
                              .Where(i => i.Id == specialization.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(specialization);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterSpecializationUpdated(specialization);

            return specialization;
        }

        partial void OnSpecializationDeleted(Polyclinic.Models.data.Specialization item);
        partial void OnAfterSpecializationDeleted(Polyclinic.Models.data.Specialization item);

        public async Task<Polyclinic.Models.data.Specialization> DeleteSpecialization(long id)
        {
            var itemToDelete = Context.Specializations
                              .Where(i => i.Id == id)
                              .Include(i => i.Doctors)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnSpecializationDeleted(itemToDelete);


            Context.Specializations.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterSpecializationDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportVisitsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/visits/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/visits/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportVisitsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/visits/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/visits/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnVisitsRead(ref IQueryable<Polyclinic.Models.data.Visit> items);

        public async Task<IQueryable<Polyclinic.Models.data.Visit>> GetVisits(Query query = null)
        {
            var items = Context.Visits.AsQueryable();

            items = items.Include(i => i.Diagnosis);
            items = items.Include(i => i.Record);
            items = items.Include(i => i.Record.Patient);
            items = items.Include(i => i.Record.Schedule);
            items = items.Include(i => i.Record.Schedule.Doctor);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnVisitsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnVisitGet(Polyclinic.Models.data.Visit item);
        partial void OnGetVisitById(ref IQueryable<Polyclinic.Models.data.Visit> items);


        public async Task<Polyclinic.Models.data.Visit> GetVisitById(long id)
        {
            var items = Context.Visits
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.Diagnosis);
            items = items.Include(i => i.Record);
 
            OnGetVisitById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnVisitGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnVisitCreated(Polyclinic.Models.data.Visit item);
        partial void OnAfterVisitCreated(Polyclinic.Models.data.Visit item);

        public async Task<Polyclinic.Models.data.Visit> CreateVisit(Polyclinic.Models.data.Visit visit)
        {
            OnVisitCreated(visit);

            var existingItem = Context.Visits
                              .Where(i => i.Id == visit.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Visits.Add(visit);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(visit).State = EntityState.Detached;
                throw;
            }

            OnAfterVisitCreated(visit);

            return visit;
        }

        public async Task<Polyclinic.Models.data.Visit> CancelVisitChanges(Polyclinic.Models.data.Visit item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnVisitUpdated(Polyclinic.Models.data.Visit item);
        partial void OnAfterVisitUpdated(Polyclinic.Models.data.Visit item);

        public async Task<Polyclinic.Models.data.Visit> UpdateVisit(long id, Polyclinic.Models.data.Visit visit)
        {
            OnVisitUpdated(visit);

            var itemToUpdate = Context.Visits
                              .Where(i => i.Id == visit.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(visit);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterVisitUpdated(visit);

            return visit;
        }

        partial void OnVisitDeleted(Polyclinic.Models.data.Visit item);
        partial void OnAfterVisitDeleted(Polyclinic.Models.data.Visit item);

        public async Task<Polyclinic.Models.data.Visit> DeleteVisit(long id)
        {
            var itemToDelete = Context.Visits
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnVisitDeleted(itemToDelete);


            Context.Visits.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterVisitDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetRoleClaimsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetroleclaims/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetroleclaims/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetRoleClaimsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetroleclaims/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetroleclaims/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetRoleClaimsRead(ref IQueryable<Polyclinic.Models.data.AspNetRoleClaim> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetRoleClaim>> GetAspNetRoleClaims(Query query = null)
        {
            var items = Context.AspNetRoleClaims.AsQueryable();

            items = items.Include(i => i.AspNetRole);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetRoleClaimsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetRoleClaimGet(Polyclinic.Models.data.AspNetRoleClaim item);
        partial void OnGetAspNetRoleClaimById(ref IQueryable<Polyclinic.Models.data.AspNetRoleClaim> items);


        public async Task<Polyclinic.Models.data.AspNetRoleClaim> GetAspNetRoleClaimById(long id)
        {
            var items = Context.AspNetRoleClaims
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.AspNetRole);
 
            OnGetAspNetRoleClaimById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetRoleClaimGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetRoleClaimCreated(Polyclinic.Models.data.AspNetRoleClaim item);
        partial void OnAfterAspNetRoleClaimCreated(Polyclinic.Models.data.AspNetRoleClaim item);

        public async Task<Polyclinic.Models.data.AspNetRoleClaim> CreateAspNetRoleClaim(Polyclinic.Models.data.AspNetRoleClaim aspnetroleclaim)
        {
            OnAspNetRoleClaimCreated(aspnetroleclaim);

            var existingItem = Context.AspNetRoleClaims
                              .Where(i => i.Id == aspnetroleclaim.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetRoleClaims.Add(aspnetroleclaim);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetroleclaim).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetRoleClaimCreated(aspnetroleclaim);

            return aspnetroleclaim;
        }

        public async Task<Polyclinic.Models.data.AspNetRoleClaim> CancelAspNetRoleClaimChanges(Polyclinic.Models.data.AspNetRoleClaim item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetRoleClaimUpdated(Polyclinic.Models.data.AspNetRoleClaim item);
        partial void OnAfterAspNetRoleClaimUpdated(Polyclinic.Models.data.AspNetRoleClaim item);

        public async Task<Polyclinic.Models.data.AspNetRoleClaim> UpdateAspNetRoleClaim(long id, Polyclinic.Models.data.AspNetRoleClaim aspnetroleclaim)
        {
            OnAspNetRoleClaimUpdated(aspnetroleclaim);

            var itemToUpdate = Context.AspNetRoleClaims
                              .Where(i => i.Id == aspnetroleclaim.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetroleclaim);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetRoleClaimUpdated(aspnetroleclaim);

            return aspnetroleclaim;
        }

        partial void OnAspNetRoleClaimDeleted(Polyclinic.Models.data.AspNetRoleClaim item);
        partial void OnAfterAspNetRoleClaimDeleted(Polyclinic.Models.data.AspNetRoleClaim item);

        public async Task<Polyclinic.Models.data.AspNetRoleClaim> DeleteAspNetRoleClaim(long id)
        {
            var itemToDelete = Context.AspNetRoleClaims
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetRoleClaimDeleted(itemToDelete);


            Context.AspNetRoleClaims.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetRoleClaimDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetRolesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetroles/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetroles/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetRolesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetroles/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetroles/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetRolesRead(ref IQueryable<Polyclinic.Models.data.AspNetRole> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetRole>> GetAspNetRoles(Query query = null)
        {
            var items = Context.AspNetRoles.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetRolesRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetRoleGet(Polyclinic.Models.data.AspNetRole item);
        partial void OnGetAspNetRoleById(ref IQueryable<Polyclinic.Models.data.AspNetRole> items);


        public async Task<Polyclinic.Models.data.AspNetRole> GetAspNetRoleById(string id)
        {
            var items = Context.AspNetRoles
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetAspNetRoleById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetRoleGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetRoleCreated(Polyclinic.Models.data.AspNetRole item);
        partial void OnAfterAspNetRoleCreated(Polyclinic.Models.data.AspNetRole item);

        public async Task<Polyclinic.Models.data.AspNetRole> CreateAspNetRole(Polyclinic.Models.data.AspNetRole aspnetrole)
        {
            OnAspNetRoleCreated(aspnetrole);

            var existingItem = Context.AspNetRoles
                              .Where(i => i.Id == aspnetrole.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetRoles.Add(aspnetrole);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetrole).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetRoleCreated(aspnetrole);

            return aspnetrole;
        }

        public async Task<Polyclinic.Models.data.AspNetRole> CancelAspNetRoleChanges(Polyclinic.Models.data.AspNetRole item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetRoleUpdated(Polyclinic.Models.data.AspNetRole item);
        partial void OnAfterAspNetRoleUpdated(Polyclinic.Models.data.AspNetRole item);

        public async Task<Polyclinic.Models.data.AspNetRole> UpdateAspNetRole(string id, Polyclinic.Models.data.AspNetRole aspnetrole)
        {
            OnAspNetRoleUpdated(aspnetrole);

            var itemToUpdate = Context.AspNetRoles
                              .Where(i => i.Id == aspnetrole.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetrole);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetRoleUpdated(aspnetrole);

            return aspnetrole;
        }

        partial void OnAspNetRoleDeleted(Polyclinic.Models.data.AspNetRole item);
        partial void OnAfterAspNetRoleDeleted(Polyclinic.Models.data.AspNetRole item);

        public async Task<Polyclinic.Models.data.AspNetRole> DeleteAspNetRole(string id)
        {
            var itemToDelete = Context.AspNetRoles
                              .Where(i => i.Id == id)
                              .Include(i => i.AspNetRoleClaims)
                              .Include(i => i.AspNetUserRoles)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetRoleDeleted(itemToDelete);


            Context.AspNetRoles.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetRoleDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetUserClaimsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserclaims/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserclaims/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetUserClaimsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserclaims/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserclaims/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetUserClaimsRead(ref IQueryable<Polyclinic.Models.data.AspNetUserClaim> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetUserClaim>> GetAspNetUserClaims(Query query = null)
        {
            var items = Context.AspNetUserClaims.AsQueryable();

            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetUserClaimsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetUserClaimGet(Polyclinic.Models.data.AspNetUserClaim item);
        partial void OnGetAspNetUserClaimById(ref IQueryable<Polyclinic.Models.data.AspNetUserClaim> items);


        public async Task<Polyclinic.Models.data.AspNetUserClaim> GetAspNetUserClaimById(long id)
        {
            var items = Context.AspNetUserClaims
                              .AsNoTracking()
                              .Where(i => i.Id == id);

            items = items.Include(i => i.AspNetUser);
 
            OnGetAspNetUserClaimById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetUserClaimGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetUserClaimCreated(Polyclinic.Models.data.AspNetUserClaim item);
        partial void OnAfterAspNetUserClaimCreated(Polyclinic.Models.data.AspNetUserClaim item);

        public async Task<Polyclinic.Models.data.AspNetUserClaim> CreateAspNetUserClaim(Polyclinic.Models.data.AspNetUserClaim aspnetuserclaim)
        {
            OnAspNetUserClaimCreated(aspnetuserclaim);

            var existingItem = Context.AspNetUserClaims
                              .Where(i => i.Id == aspnetuserclaim.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetUserClaims.Add(aspnetuserclaim);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetuserclaim).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetUserClaimCreated(aspnetuserclaim);

            return aspnetuserclaim;
        }

        public async Task<Polyclinic.Models.data.AspNetUserClaim> CancelAspNetUserClaimChanges(Polyclinic.Models.data.AspNetUserClaim item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetUserClaimUpdated(Polyclinic.Models.data.AspNetUserClaim item);
        partial void OnAfterAspNetUserClaimUpdated(Polyclinic.Models.data.AspNetUserClaim item);

        public async Task<Polyclinic.Models.data.AspNetUserClaim> UpdateAspNetUserClaim(long id, Polyclinic.Models.data.AspNetUserClaim aspnetuserclaim)
        {
            OnAspNetUserClaimUpdated(aspnetuserclaim);

            var itemToUpdate = Context.AspNetUserClaims
                              .Where(i => i.Id == aspnetuserclaim.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetuserclaim);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetUserClaimUpdated(aspnetuserclaim);

            return aspnetuserclaim;
        }

        partial void OnAspNetUserClaimDeleted(Polyclinic.Models.data.AspNetUserClaim item);
        partial void OnAfterAspNetUserClaimDeleted(Polyclinic.Models.data.AspNetUserClaim item);

        public async Task<Polyclinic.Models.data.AspNetUserClaim> DeleteAspNetUserClaim(long id)
        {
            var itemToDelete = Context.AspNetUserClaims
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetUserClaimDeleted(itemToDelete);


            Context.AspNetUserClaims.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetUserClaimDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetUserLoginsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserlogins/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserlogins/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetUserLoginsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserlogins/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserlogins/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetUserLoginsRead(ref IQueryable<Polyclinic.Models.data.AspNetUserLogin> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetUserLogin>> GetAspNetUserLogins(Query query = null)
        {
            var items = Context.AspNetUserLogins.AsQueryable();

            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetUserLoginsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetUserLoginGet(Polyclinic.Models.data.AspNetUserLogin item);
        partial void OnGetAspNetUserLoginByLoginProviderAndProviderKey(ref IQueryable<Polyclinic.Models.data.AspNetUserLogin> items);


        public async Task<Polyclinic.Models.data.AspNetUserLogin> GetAspNetUserLoginByLoginProviderAndProviderKey(string loginprovider, string providerkey)
        {
            var items = Context.AspNetUserLogins
                              .AsNoTracking()
                              .Where(i => i.LoginProvider == loginprovider && i.ProviderKey == providerkey);

            items = items.Include(i => i.AspNetUser);
 
            OnGetAspNetUserLoginByLoginProviderAndProviderKey(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetUserLoginGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetUserLoginCreated(Polyclinic.Models.data.AspNetUserLogin item);
        partial void OnAfterAspNetUserLoginCreated(Polyclinic.Models.data.AspNetUserLogin item);

        public async Task<Polyclinic.Models.data.AspNetUserLogin> CreateAspNetUserLogin(Polyclinic.Models.data.AspNetUserLogin aspnetuserlogin)
        {
            OnAspNetUserLoginCreated(aspnetuserlogin);

            var existingItem = Context.AspNetUserLogins
                              .Where(i => i.LoginProvider == aspnetuserlogin.LoginProvider && i.ProviderKey == aspnetuserlogin.ProviderKey)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetUserLogins.Add(aspnetuserlogin);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetuserlogin).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetUserLoginCreated(aspnetuserlogin);

            return aspnetuserlogin;
        }

        public async Task<Polyclinic.Models.data.AspNetUserLogin> CancelAspNetUserLoginChanges(Polyclinic.Models.data.AspNetUserLogin item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetUserLoginUpdated(Polyclinic.Models.data.AspNetUserLogin item);
        partial void OnAfterAspNetUserLoginUpdated(Polyclinic.Models.data.AspNetUserLogin item);

        public async Task<Polyclinic.Models.data.AspNetUserLogin> UpdateAspNetUserLogin(string loginprovider, string providerkey, Polyclinic.Models.data.AspNetUserLogin aspnetuserlogin)
        {
            OnAspNetUserLoginUpdated(aspnetuserlogin);

            var itemToUpdate = Context.AspNetUserLogins
                              .Where(i => i.LoginProvider == aspnetuserlogin.LoginProvider && i.ProviderKey == aspnetuserlogin.ProviderKey)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetuserlogin);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetUserLoginUpdated(aspnetuserlogin);

            return aspnetuserlogin;
        }

        partial void OnAspNetUserLoginDeleted(Polyclinic.Models.data.AspNetUserLogin item);
        partial void OnAfterAspNetUserLoginDeleted(Polyclinic.Models.data.AspNetUserLogin item);

        public async Task<Polyclinic.Models.data.AspNetUserLogin> DeleteAspNetUserLogin(string loginprovider, string providerkey)
        {
            var itemToDelete = Context.AspNetUserLogins
                              .Where(i => i.LoginProvider == loginprovider && i.ProviderKey == providerkey)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetUserLoginDeleted(itemToDelete);


            Context.AspNetUserLogins.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetUserLoginDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetUserRolesToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserroles/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserroles/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetUserRolesToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetuserroles/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetuserroles/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetUserRolesRead(ref IQueryable<Polyclinic.Models.data.AspNetUserRole> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetUserRole>> GetAspNetUserRoles(Query query = null)
        {
            var items = Context.AspNetUserRoles.AsQueryable();

            items = items.Include(i => i.AspNetRole);
            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetUserRolesRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetUserRoleGet(Polyclinic.Models.data.AspNetUserRole item);
        partial void OnGetAspNetUserRoleByUserIdAndRoleId(ref IQueryable<Polyclinic.Models.data.AspNetUserRole> items);


        public async Task<Polyclinic.Models.data.AspNetUserRole> GetAspNetUserRoleByUserIdAndRoleId(string userid, string roleid)
        {
            var items = Context.AspNetUserRoles
                              .AsNoTracking()
                              .Where(i => i.UserId == userid && i.RoleId == roleid);

            items = items.Include(i => i.AspNetRole);
            items = items.Include(i => i.AspNetUser);
 
            OnGetAspNetUserRoleByUserIdAndRoleId(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetUserRoleGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetUserRoleCreated(Polyclinic.Models.data.AspNetUserRole item);
        partial void OnAfterAspNetUserRoleCreated(Polyclinic.Models.data.AspNetUserRole item);

        public async Task<Polyclinic.Models.data.AspNetUserRole> CreateAspNetUserRole(Polyclinic.Models.data.AspNetUserRole aspnetuserrole)
        {
            OnAspNetUserRoleCreated(aspnetuserrole);

            var existingItem = Context.AspNetUserRoles
                              .Where(i => i.UserId == aspnetuserrole.UserId && i.RoleId == aspnetuserrole.RoleId)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetUserRoles.Add(aspnetuserrole);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetuserrole).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetUserRoleCreated(aspnetuserrole);

            return aspnetuserrole;
        }

        public async Task<Polyclinic.Models.data.AspNetUserRole> CancelAspNetUserRoleChanges(Polyclinic.Models.data.AspNetUserRole item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetUserRoleUpdated(Polyclinic.Models.data.AspNetUserRole item);
        partial void OnAfterAspNetUserRoleUpdated(Polyclinic.Models.data.AspNetUserRole item);

        public async Task<Polyclinic.Models.data.AspNetUserRole> UpdateAspNetUserRole(string userid, string roleid, Polyclinic.Models.data.AspNetUserRole aspnetuserrole)
        {
            OnAspNetUserRoleUpdated(aspnetuserrole);

            var itemToUpdate = Context.AspNetUserRoles
                              .Where(i => i.UserId == aspnetuserrole.UserId && i.RoleId == aspnetuserrole.RoleId)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetuserrole);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetUserRoleUpdated(aspnetuserrole);

            return aspnetuserrole;
        }

        partial void OnAspNetUserRoleDeleted(Polyclinic.Models.data.AspNetUserRole item);
        partial void OnAfterAspNetUserRoleDeleted(Polyclinic.Models.data.AspNetUserRole item);

        public async Task<Polyclinic.Models.data.AspNetUserRole> DeleteAspNetUserRole(string userid, string roleid)
        {
            var itemToDelete = Context.AspNetUserRoles
                              .Where(i => i.UserId == userid && i.RoleId == roleid)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetUserRoleDeleted(itemToDelete);


            Context.AspNetUserRoles.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetUserRoleDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetUsersToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetusers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetusers/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetUsersToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetusers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetusers/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetUsersRead(ref IQueryable<Polyclinic.Models.data.AspNetUser> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetUser>> GetAspNetUsers(Query query = null)
        {
            var items = Context.AspNetUsers.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetUsersRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetUserGet(Polyclinic.Models.data.AspNetUser item);
        partial void OnGetAspNetUserById(ref IQueryable<Polyclinic.Models.data.AspNetUser> items);


        public async Task<Polyclinic.Models.data.AspNetUser> GetAspNetUserById(string id)
        {
            var items = Context.AspNetUsers
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetAspNetUserById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetUserGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetUserCreated(Polyclinic.Models.data.AspNetUser item);
        partial void OnAfterAspNetUserCreated(Polyclinic.Models.data.AspNetUser item);

        public async Task<Polyclinic.Models.data.AspNetUser> CreateAspNetUser(Polyclinic.Models.data.AspNetUser aspnetuser)
        {
            OnAspNetUserCreated(aspnetuser);

            var existingItem = Context.AspNetUsers
                              .Where(i => i.Id == aspnetuser.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetUsers.Add(aspnetuser);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetuser).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetUserCreated(aspnetuser);

            return aspnetuser;
        }

        public async Task<Polyclinic.Models.data.AspNetUser> CancelAspNetUserChanges(Polyclinic.Models.data.AspNetUser item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetUserUpdated(Polyclinic.Models.data.AspNetUser item);
        partial void OnAfterAspNetUserUpdated(Polyclinic.Models.data.AspNetUser item);

        public async Task<Polyclinic.Models.data.AspNetUser> UpdateAspNetUser(string id, Polyclinic.Models.data.AspNetUser aspnetuser)
        {
            OnAspNetUserUpdated(aspnetuser);

            var itemToUpdate = Context.AspNetUsers
                              .Where(i => i.Id == aspnetuser.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetuser);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetUserUpdated(aspnetuser);

            return aspnetuser;
        }

        partial void OnAspNetUserDeleted(Polyclinic.Models.data.AspNetUser item);
        partial void OnAfterAspNetUserDeleted(Polyclinic.Models.data.AspNetUser item);

        public async Task<Polyclinic.Models.data.AspNetUser> DeleteAspNetUser(string id)
        {
            var itemToDelete = Context.AspNetUsers
                              .Where(i => i.Id == id)
                              .Include(i => i.AspNetUserTokens)
                              .Include(i => i.AspNetUserClaims)
                              .Include(i => i.AspNetUserLogins)
                              .Include(i => i.AspNetUserRoles)
                              .Include(i => i.Doctors)
                              .Include(i => i.Patients)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetUserDeleted(itemToDelete);


            Context.AspNetUsers.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetUserDeleted(itemToDelete);

            return itemToDelete;
        }
    
        public async Task ExportAspNetUserTokensToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetusertokens/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetusertokens/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportAspNetUserTokensToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/data/aspnetusertokens/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/data/aspnetusertokens/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnAspNetUserTokensRead(ref IQueryable<Polyclinic.Models.data.AspNetUserToken> items);

        public async Task<IQueryable<Polyclinic.Models.data.AspNetUserToken>> GetAspNetUserTokens(Query query = null)
        {
            var items = Context.AspNetUserTokens.AsQueryable();

            items = items.Include(i => i.AspNetUser);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnAspNetUserTokensRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnAspNetUserTokenGet(Polyclinic.Models.data.AspNetUserToken item);
        partial void OnGetAspNetUserTokenByUserIdAndLoginProviderAndName(ref IQueryable<Polyclinic.Models.data.AspNetUserToken> items);


        public async Task<Polyclinic.Models.data.AspNetUserToken> GetAspNetUserTokenByUserIdAndLoginProviderAndName(string userid, string loginprovider, string name)
        {
            var items = Context.AspNetUserTokens
                              .AsNoTracking()
                              .Where(i => i.UserId == userid && i.LoginProvider == loginprovider && i.Name == name);

            items = items.Include(i => i.AspNetUser);
 
            OnGetAspNetUserTokenByUserIdAndLoginProviderAndName(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnAspNetUserTokenGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnAspNetUserTokenCreated(Polyclinic.Models.data.AspNetUserToken item);
        partial void OnAfterAspNetUserTokenCreated(Polyclinic.Models.data.AspNetUserToken item);

        public async Task<Polyclinic.Models.data.AspNetUserToken> CreateAspNetUserToken(Polyclinic.Models.data.AspNetUserToken aspnetusertoken)
        {
            OnAspNetUserTokenCreated(aspnetusertoken);

            var existingItem = Context.AspNetUserTokens
                              .Where(i => i.UserId == aspnetusertoken.UserId && i.LoginProvider == aspnetusertoken.LoginProvider && i.Name == aspnetusertoken.Name)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.AspNetUserTokens.Add(aspnetusertoken);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(aspnetusertoken).State = EntityState.Detached;
                throw;
            }

            OnAfterAspNetUserTokenCreated(aspnetusertoken);

            return aspnetusertoken;
        }

        public async Task<Polyclinic.Models.data.AspNetUserToken> CancelAspNetUserTokenChanges(Polyclinic.Models.data.AspNetUserToken item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnAspNetUserTokenUpdated(Polyclinic.Models.data.AspNetUserToken item);
        partial void OnAfterAspNetUserTokenUpdated(Polyclinic.Models.data.AspNetUserToken item);

        public async Task<Polyclinic.Models.data.AspNetUserToken> UpdateAspNetUserToken(string userid, string loginprovider, string name, Polyclinic.Models.data.AspNetUserToken aspnetusertoken)
        {
            OnAspNetUserTokenUpdated(aspnetusertoken);

            var itemToUpdate = Context.AspNetUserTokens
                              .Where(i => i.UserId == aspnetusertoken.UserId && i.LoginProvider == aspnetusertoken.LoginProvider && i.Name == aspnetusertoken.Name)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(aspnetusertoken);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterAspNetUserTokenUpdated(aspnetusertoken);

            return aspnetusertoken;
        }

        partial void OnAspNetUserTokenDeleted(Polyclinic.Models.data.AspNetUserToken item);
        partial void OnAfterAspNetUserTokenDeleted(Polyclinic.Models.data.AspNetUserToken item);

        public async Task<Polyclinic.Models.data.AspNetUserToken> DeleteAspNetUserToken(string userid, string loginprovider, string name)
        {
            var itemToDelete = Context.AspNetUserTokens
                              .Where(i => i.UserId == userid && i.LoginProvider == loginprovider && i.Name == name)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnAspNetUserTokenDeleted(itemToDelete);


            Context.AspNetUserTokens.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterAspNetUserTokenDeleted(itemToDelete);

            return itemToDelete;
        }

        public async Task<IQueryable<Polyclinic.Models.data.Schedule>> GetFreeSchedules(Query query = null)
        {
            var items = Context.Schedules.AsQueryable();

            var usedIdSchedules = Context.Records.Select(r => r.IdSchedule).ToList();

            items =  items.Where(s => !usedIdSchedules.Contains(s.Id));

            items = items.Include(i => i.Doctor);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnSchedulesRead(ref items);

            return await Task.FromResult(items);
        }

        public async Task<IQueryable<Polyclinic.Models.data.Schedule>> GetFreeSchedulesEdit(int? id = null, Query query = null)
        {
            var items = Context.Schedules.AsQueryable();

            var usedIdSchedules = Context.Records.Select(r => r.IdSchedule).ToList();

            if (id != null)
            {
                // Исключить запись с переданным идентификатором из списка занятых записей
                usedIdSchedules.Remove(id.Value);
            }

            items = items.Where(s => !usedIdSchedules.Contains(s.Id));

            items = items.Include(i => i.Doctor);

            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach (var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnSchedulesRead(ref items);

            return await Task.FromResult(items);
        }
    }
}