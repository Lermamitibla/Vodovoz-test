using System;
using Vodovoz_test.Model;
using Vodovoz_test.SupportingClasses;
using Vodovoz_test.Commands;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace Vodovoz_test.ViewModel
{
    partial class MainVM : BaseVM
    {
        private string _departmentName = String.Empty;
        private int departmentManagerID;
        private EmployeerOfTheDep _managerOfSelectedDepartment;
        private bool isSelectedEmployeeAreManager;
        private DepWithManagerName selectedDep;
        private int selectedDepId;
        private SaveButtonStatus _departmentSaveButtonStatus;
        protected ObservableCollection<DepWithManagerName> _allDepartmentsWithManagerName;
        private bool IsExistingDepartment;
        private ObservableCollection<EmployeerOfTheDep> _allEmpOfSelectedDep;

        public string DepartmentName { 
            get { return _departmentName; } 
            set { if (!_departmentName.Equals(value)) _departmentName = value; OnPropertyChanged(); } 
        }
        public EmployeerOfTheDep ManagerOfSelectedDepartment 
        { 
            get { return _managerOfSelectedDepartment; } 
            set 
            {
                if (value != null)  CheckIfSelectedEmployeeAreManager(value.empid);
                _managerOfSelectedDepartment = value;
                OnPropertyChanged();
            } 
        }
        public ObservableCollection<DepWithManagerName> AllDepartmentsWithManagerName
        {
            get { return _allDepartmentsWithManagerName; }
            set { _allDepartmentsWithManagerName = value; OnPropertyChanged(); }
        }
        public SaveButtonStatus DepartmentSaveButtonStatus 
        { 
            get { return _departmentSaveButtonStatus; } 
            set { if (!_firstName.Equals(value)) _departmentSaveButtonStatus = value; OnPropertyChanged(); } 
        }
        public ObservableCollection<EmployeerOfTheDep> AllEmpOfSelectedDep
        {
            get { return _allEmpOfSelectedDep; }
            set { _allEmpOfSelectedDep = value; OnPropertyChanged(); }
        }

        #region Команды и их методы
        public ICommand DeptSaveButtonCommand { get; private set; }
        public ICommand DepSelectedInDataGrid { get; private set; }
        public ICommand DepCancelButtonCommand { get; private set; }
        public ICommand DepRemoveManagerCommand { get; private set; }
        public ICommand DepRemoveCommand { get; private set; }

        private void OnDeptSaveButtonCommandExecuted(object p)
        {
            if (CheckPropertiesBeforeCreateOrUpdate()) 
            {
                if (IsExistingDepartment) UpdateDep();
                else SaveNewDep();
            }
        }
        private bool CanDeptSaveButtonCommandExecute(object p) => true;

        private void OnDepSelectedInDataGridExecuted(object p)
        {
            if (p != null)
            {
                selectedDep = (DepWithManagerName)p;
                SetChoosenDepartmentProperties();

                DepartmentSaveButtonStatus = SaveButtonStatus.Изменить;
            }
        }
        private bool CanDepSelectedInDataGridExecute(object p) => true;

        private void OnDepCancelButtonExecuted(object p)
        {
            CleanDepartmentProperties();
        }
        private bool CanDepCancelButtonExecute(object p) => true;

        private void OnDepRemoveManagerCommandExecuted(object p)  //проверить
        { 
            RemoveManager();
        }
        private bool CanDepRemoveManagerCommandExecute(object p)
        {
            if (isSelectedEmployeeAreManager) return true;
            else return false;
        }

        private void OnDepRemoveCommandExecuted(object p)
        {
            bool IsEverythingAllright = true;

            if (departmentManagerID != 0) 
            {
                MessageBox.Show("Перед удалением отдела нужно сместить руководителя");
                IsEverythingAllright = false;
            } 

            if (AllEmpOfSelectedDep.Count > 0)
            {
                MessageBox.Show("Перед удалением отдела в нем не должно быть сотрудников");
                IsEverythingAllright = false;
            }

            if (IsEverythingAllright) RemoveDepartment();
        }
        private bool CanDepRemoveCommandExecute(object p)
        {
            if (AllDepartmentsWithManagerName.Count == 0 || selectedDep == null || selectedDepId == 0) return false;
            else return true;
        }
        #endregion

        private void SaveNewDep()
        {
            dbContext.Departments.Add(new Departments(_departmentName));
            dbContext.SaveChanges();

            UpdateDataAndCleanProperties();
        }
        private void UpdateDep()
        {
            var entity = dbContext.Departments.Find(selectedDepId);
            entity.depName = DepartmentName;
            if (departmentManagerID == 0 && ManagerOfSelectedDepartment != null) entity.managerID = _managerOfSelectedDepartment.empid;
            dbContext.SaveChanges();

            UpdateDataAndCleanProperties();
        }        

        private void RemoveManager()
        {
            var dep = dbContext.Departments.Find(selectedDepId);
            dep.managerID = null;
            dbContext.SaveChanges();
            UpdateDataAndCleanProperties();
        }
        private void RemoveDepartment()
        {
            dbContext.Departments.Remove(dbContext.Departments.Find(selectedDepId));
            dbContext.SaveChanges();
            UpdateDataAndCleanProperties();
        }

        private void CleanDepartmentProperties()
        {
            DepartmentName = String.Empty;
            ManagerOfSelectedDepartment = null;
            isSelectedEmployeeAreManager = false;
            IsExistingDepartment = false;
            AllEmpOfSelectedDep = null;
            selectedDepId = 0;
            DepartmentSaveButtonStatus = SaveButtonStatus.Сохранить;
        }
        private bool CheckPropertiesBeforeCreateOrUpdate()
        {
            if (DepartmentName == String.Empty)
            {
                MessageBox.Show("Необходимо указать название отдела");
                return false;
            }
            return true;
        }
        private void CreateDepartmentsTabComments()
        {
            DeptSaveButtonCommand = new LambdaCommand(OnDeptSaveButtonCommandExecuted, CanDeptSaveButtonCommandExecute);
            DepSelectedInDataGrid = new LambdaCommand(OnDepSelectedInDataGridExecuted, CanDepSelectedInDataGridExecute);
            DepCancelButtonCommand = new LambdaCommand(OnDepCancelButtonExecuted, CanDepCancelButtonExecute);
            DepRemoveManagerCommand = new LambdaCommand(OnDepRemoveManagerCommandExecuted, CanDepRemoveManagerCommandExecute);
            DepRemoveCommand = new LambdaCommand(OnDepRemoveCommandExecuted, CanDepRemoveCommandExecute);
        }
        private void DownloadDataForDepartmentsDataGrid()
        {
            var freshDepWithManagerNameList = new ObservableCollection<DepWithManagerName>();
            //поскольку есть возможность создавать отделы без руководителя, то для получения всех отделов с фио руководителя (или без) нужно применить внешнее соединение left join, которе не поддерживается в LINQ. Поэтому идет прямой запрос в БД. 
            string sqlcommand = "select D.depID, depName, managerID, case when managerID is not null then CONCAT(lastname, ' ', firstname, ' ', patronymic) else 'руководитель не назначен' end as managerfullname from Departments as D left join Employees as E on D.managerID = E.empid; ";

            var result = dbContext.Database.SqlQuery(typeof(DepWithManagerName), sqlcommand);

            foreach (var a in result)
            {
                freshDepWithManagerNameList.Add((DepWithManagerName)a);
            }

            AllDepartmentsWithManagerName = freshDepWithManagerNameList;
        }
        private void CheckIfSelectedEmployeeAreManager(int empid)
        {
            if (empid == departmentManagerID) isSelectedEmployeeAreManager = true;
            else isSelectedEmployeeAreManager = false;
        }
        private void SetChoosenDepartmentProperties()
        {
            DepartmentName = selectedDep.depName;
            IsExistingDepartment = true;
            selectedDepId = selectedDep.depID;

            UpdateEmpListForSelectedDep(selectedDepId);

            if (selectedDep.managerID != null)
            {
                departmentManagerID = (int)selectedDep.managerID;
                ManagerOfSelectedDepartment = GetManagerOfTheSelectedDep(departmentManagerID);
            }
            else departmentManagerID = 0;

            
        }
        private EmployeerOfTheDep GetManagerOfTheSelectedDep(int managerID)
        {
            EmployeerOfTheDep result = null;
            foreach (var emp in AllEmpOfSelectedDep)
            {
                if (emp.empid == managerID) result = emp;
            }
            return result;
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
        private ObservableCollection<EmployeerOfTheDep> GetEmpListFromSelectedDep(int depID)
        {
            var result = new ObservableCollection<EmployeerOfTheDep>();
            foreach (var emp in AllEmployeesWithDepName)
            {
                if (emp.depID == depID) result.Add(emp);
            }
            return result;
        }
    }
}
