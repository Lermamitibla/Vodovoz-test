using System;
using Vodovoz_test.Model;
using System.Windows;
using System.Windows.Input;
using Vodovoz_test.Commands;
using Vodovoz_test.SupportingClasses;
using System.Collections.ObjectModel;

namespace Vodovoz_test.ViewModel
{
   partial class MainVM : BaseVM
    {
        private int empPrimaryKey;
        private string _firstName = String.Empty;
        private string _lastName = String.Empty;
        private string _patroymic = String.Empty;
        private Gender _empGender;
        private DateTime _birthDate = new DateTime(1900, 01, 01);
        private int _emploeersDepartmentID;
        private DepWithManagerName _selectedDep;
        private EmployeerOfTheDep selectedEmp;
        private SaveButtonStatus _employyerSaveButtonStatus = SaveButtonStatus.Сохранить;

        private ObservableCollection<EmployeerOfTheDep> _allEmployeesWithDepNam;

        public string FirstName { get { return _firstName; } set { if (!_firstName.Equals(value)) _firstName = value; OnPropertyChanged(); } }
        public string LastName { get { return _lastName; } set { if (!_lastName.Equals(value)) _lastName = value; OnPropertyChanged(); } }
        public string Patronymic { get { return _patroymic; } set { if (!_patroymic.Equals(value)) _patroymic = value; OnPropertyChanged(); } }
        public Gender EmpGender { get { return _empGender; } set { if (!_empGender.Equals(value)) _empGender = value; OnPropertyChanged(); } }
        public DateTime BirthDate { get { return _birthDate; } set { if (!_birthDate.Equals(value)) _birthDate = value; OnPropertyChanged();} }
        public int EmployeersDepID { get { return _emploeersDepartmentID; } set { if (!_emploeersDepartmentID.Equals(value)) _emploeersDepartmentID = value; OnPropertyChanged();} }
        public DepWithManagerName SelectedDept
        {
            get { return _selectedDep; }
            set 
            { 
                _selectedDep = value;
                OnPropertyChanged();
                if (value != null) _emploeersDepartmentID = _selectedDep.depID;
                else _emploeersDepartmentID = 0;
            }
        }

        public ObservableCollection<EmployeerOfTheDep> AllEmployeesWithDepName
        {
            get { return _allEmployeesWithDepNam; }
            set
            {
                if (!_allEmployeesWithDepNam.Equals(value)) _allEmployeesWithDepNam = value;
                OnPropertyChanged();
            }
        }

        public SaveButtonStatus EmployyerSaveButtonStatus { get { return _employyerSaveButtonStatus; } set { if (!_firstName.Equals(value)) _employyerSaveButtonStatus = value; OnPropertyChanged(); } }


        #region Команды и их методы
        public ICommand EmpSaveButtonCommand { get; private set; }
        public ICommand EmpSelectedInDataGrid { get; private set; }
        public ICommand CancelButtonCommand { get; private set; }
        public ICommand DeleteButtonCommand { get; private set; }

        private void OnEmpSaveButtonCommandExecuted (object p)
        {
            if (EmployeersDepID == 0) MessageBox.Show("Необходимо указать отдел");
            else
            {
                if (empPrimaryKey == 0) SaveNewEmp();
                else UpdateEmp();
            }
        }
        private bool CanEmpSaveButtonCommandExecute(object p) => true;

        private void OnEmpSelectedInDataGridExecuted (object p)
        {
            if (p != null)
            {
                selectedEmp = (EmployeerOfTheDep)p;
                empPrimaryKey = selectedEmp.empid;
                FirstName = selectedEmp.firstname;
                LastName = selectedEmp.lastname;
                Patronymic = selectedEmp.patronymic;
                EmpGender = selectedEmp.gender;
                BirthDate = selectedEmp.dateOfBirth;
                EmployyerSaveButtonStatus = SaveButtonStatus.Изменить;
                
                foreach (var dep in _allDepWithManagerName)
                {
                    if (dep.depID == selectedEmp.depID) SelectedDept = dep;
                }
            } 
        }
        private bool CanEmpSelectedInDataGridExecute(object p) => true;

        private void OnCancelButtonExecuted (object p)
        {
            CancelProperties();
        }
        private bool CanCancelButtonExecute(object p) => true;

        private void OnDeleteButtonExecuted (object p)
        {
            dbContext.Employees.Remove(dbContext.Employees.Find(empPrimaryKey));
            dbContext.SaveChanges();

            DownloadDataForDepartmentsDataGrid();
            UpdateLinkedResources();
            CancelProperties();
        }
        private bool CanDeleteButtonExecute(object p)
        {
            if (selectedEmp == null) return false;
            else return true;
        }
        #endregion

        #region проверка и корректировка свойств перед созданием/изменением данных о сотруднике
        private bool CheckAllEmpDataFilled()
        {
            return true;
        }
        private bool CheckFirstName()
        {
            FirstName.Trim();
            if (FirstName == String.Empty) return false;
            else return true;
        }
        private bool CheckLastName()
        {
            LastName.Trim();
            if (LastName == String.Empty) return false;
            else return true;
        }
        private bool CheckPatronymic()
        {
            Patronymic.Trim();
            return true;
            // допилить при необходимости
        }

        #endregion


        private void CancelProperties()
        {
            FirstName = LastName = Patronymic = String.Empty;
            empPrimaryKey = 0;
            SelectedDept = null;
            selectedEmp = null;
            EmployyerSaveButtonStatus = SaveButtonStatus.Сохранить;
        }
        private void SaveNewEmp()
        {
            var newEmployee = new Employees(FirstName, LastName, Patronymic, EmpGender, BirthDate, EmployeersDepID );

            if (EmpGender == 0) MessageBox.Show("Нужно указать пол нового сотрудника");
            else
            {
                dbContext.Employees.Add(newEmployee);
                dbContext.SaveChanges();

                UpdateLinkedResources();
                CancelProperties();
            } 
        }
        private void UpdateEmp()
        {
                var entity = dbContext.Employees.Find(empPrimaryKey);
                entity.firstname = FirstName;
                entity.lastname = LastName;
                entity.patronymic = Patronymic;
                entity.gender = EmpGender;
                entity.dateOfBirth = BirthDate;
                entity.depID = EmployeersDepID;
                dbContext.SaveChanges();

            DownloadDataForDepartmentsDataGrid();
            UpdateLinkedResources();
            CancelProperties();
        }
        private void UpdateLinkedResources()
        {
            DownloadDataForEmploeersDataGrid();
            UpdateEmpListForSelectedDep();
        }

        private void CreateEmployeerTabCommands()
        {
            EmpSaveButtonCommand = new LambdaCommand(OnEmpSaveButtonCommandExecuted, CanEmpSaveButtonCommandExecute);
            EmpSelectedInDataGrid = new LambdaCommand(OnEmpSelectedInDataGridExecuted, CanEmpSelectedInDataGridExecute);
            CancelButtonCommand = new LambdaCommand(OnCancelButtonExecuted, CanCancelButtonExecute);
            DeleteButtonCommand = new LambdaCommand(OnDeleteButtonExecuted, CanDeleteButtonExecute);
        }

        private void DownloadDataForEmploeersDataGrid()
        {
            var freshEmpList = new ObservableCollection<EmployeerOfTheDep>();
            dynamic rawdata = LoadEmployeersOfTheDep();
            foreach (var source in rawdata)
            {
                freshEmpList.Add(new EmployeerOfTheDep(source.empid, source.firstname, source.lastname, source.patronymic, source.gender, source.dateOfBirth, source.depID, source.depName, source.managerID));
            }
            AllEmployeesWithDepName = freshEmpList;
        }

    }
}
