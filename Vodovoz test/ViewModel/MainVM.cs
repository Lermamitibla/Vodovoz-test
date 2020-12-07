using System.Linq;
using Vodovoz_test.Model;
using Vodovoz_test.SupportingClasses;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace Vodovoz_test.ViewModel
{
    internal enum SaveButtonStatus {Сохранить, Изменить}

    partial class MainVM : BaseVM
    {

        private VodovozTESTEntities dbContext;
        protected static ObservableCollection<EmployeerOfTheDep> _allEmpOfSelectedDep;
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        

        public MainVM()
        {
            CreateAllCommands();
            DownloadAllDataFromDatabase();
        }







        private void UpdateEmpListForSelectedDep()
        {
            if (selectedDepId != 0) UpdateEmpListForSelectedDep(selectedDepId);
        }
        private void UpdateEmpListForSelectedDep(int depId)
        {
            selectedDepId = depId;
            AllEmpOfSelectedDep = GetEmpListFromSelectedDep(selectedDepId);
        }

        private  ObservableCollection<EmployeerOfTheDep> GetEmpListFromSelectedDep(int depID)
        {
            var result = new ObservableCollection<EmployeerOfTheDep>();
            foreach (var emp in AllEmployeesWithDepName)
            {
                if (emp.depID == depID) result.Add(emp);
            }
            return result;
        }
        public ObservableCollection<EmployeerOfTheDep> AllEmpOfSelectedDep
        {
            get { return _allEmpOfSelectedDep; } set { _allEmpOfSelectedDep = value; OnPropertyChanged(); }
        }
        
        private dynamic LoadEmployeersOfTheDep()
        {
            dynamic result = (from Employees in dbContext.Employees.ToList()
                              join Departments in dbContext.Departments.ToList()
                              on Employees.depID equals Departments.depID
                              select new
                              { Employees.empid, Employees.firstname, Employees.lastname, Employees.patronymic, Employees.gender, Employees.dateOfBirth, Employees.depID, Departments.depName, Departments.managerID }).ToList();
            return result;
        }


        private void CreateAllCommands()
        {
            CreateEmployeerTabCommands();
            CreateDepartmentsTabComments();
        }

        private void DownloadAllDataFromDatabase()
        {
            dbContext = new VodovozTESTEntities();
            DownloadDataForEmploeersDataGrid();
            DownloadDataForDepartmentsDataGrid();
        }




    }
}

