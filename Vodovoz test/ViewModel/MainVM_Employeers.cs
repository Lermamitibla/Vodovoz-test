using System;
using Vodovoz_test.Model;
using System.Windows;
using System.Windows.Input;
using Vodovoz_test.Commands;
using Vodovoz_test.SupportingClasses;
using System.Collections.ObjectModel;
using System.Linq;

namespace Vodovoz_test.ViewModel
{
   partial class MainVM : BaseVM
    {
        private int selectedEmployeerID;
        private string _firstName = String.Empty;
        private string _lastName = String.Empty;
        private string _patroymic = String.Empty;
        private Gender _empGender;
        private DateTime _birthDate = new DateTime(1900, 01, 01);
        private int employeersDepartmentID;
        private DepWithManagerName _selectedDep;
        private EmployeerOfTheDep selectedEmp;
        private SaveButtonStatus _employyerSaveButtonStatus = SaveButtonStatus.Сохранить;
        private ObservableCollection<EmployeerOfTheDep> _allEmployeesWithDepNam;
        private bool isExistingEmployee;

        #region свойства
        public string FirstName { get { return _firstName; } set { if (!_firstName.Equals(value)) _firstName = value; OnPropertyChanged(); } }
        public string LastName { get { return _lastName; } set { if (!_lastName.Equals(value)) _lastName = value; OnPropertyChanged(); } }
        public string Patronymic { get { return _patroymic; } set { if (!_patroymic.Equals(value)) _patroymic = value; OnPropertyChanged(); } }
        public Gender EmployeerGender { get { return _empGender; } set { if (!_empGender.Equals(value)) _empGender = value; OnPropertyChanged(); } }
        public DateTime BirthDate { get { return _birthDate; } set { if (!_birthDate.Equals(value)) _birthDate = value; OnPropertyChanged(); } }
        public DepWithManagerName DepartmentSelectedForEmployee
        {
            get { return _selectedDep; }
            set 
            { 
                _selectedDep = value;
                OnPropertyChanged();
                if (value != null)
                {
                    employeersDepartmentID = _selectedDep.depID;
                }
                else employeersDepartmentID = 0;
            }
        }
        public ObservableCollection<EmployeerOfTheDep> AllEmployeesWithDepName { get { return _allEmployeesWithDepNam; } set { _allEmployeesWithDepNam = value; OnPropertyChanged(); } }
        public SaveButtonStatus EmployyerSaveButtonStatus { get { return _employyerSaveButtonStatus; } set { if (!_firstName.Equals(value)) _employyerSaveButtonStatus = value; OnPropertyChanged(); } }
        #endregion

        #region Команды и их методы
        public ICommand EmployeerSaveButtonCommand { get; private set; }
        public ICommand EmployeerSelectedInDataGrid { get; private set; }
        public ICommand EmployeerCancelButtonCommand { get; private set; }
        public ICommand DeleteEmployeerButtonCommand { get; private set; }

        private void OnEmpSaveButtonCommandExecuted (object p)
        {
                if (isExistingEmployee) UpdateEmp();
                else SaveNewEmp();
        }
        private bool CanEmpSaveButtonCommandExecute(object p) => true;

        private void OnEmpSelectedInDataGridExecuted (object p)
        {
            if (p != null)
            {
                selectedEmp = (EmployeerOfTheDep)p;
                SetEmployeePropertiesFromChoosenEmployee();

                isExistingEmployee = true;
                EmployyerSaveButtonStatus = SaveButtonStatus.Изменить;
            } 
        }
        private bool CanEmpSelectedInDataGridExecute(object p) => true;

        private void OnEmployeerCancelButtonExecuted (object p)
        {
            CleanEmployeeProperties();
        }
        private bool CanEmployeerCancelButtonExecute(object p) => true;

        private void OnDeleteEmployeerButtonExecuted (object p)
        {
            DeleteEmp();
        }
        private bool CanDeleteEmployeerButtonExecute(object p)
        {
            if (selectedEmp == null) return false;
            else return true;
        }
        #endregion

        #region проверка и корректировка свойств перед созданием/изменением данных о сотруднике
        private bool CheckAllEmpDataProperties()
        {
            if (CheckFirstName() && CheckLastName() && CheckGender() && CheckDepartment()) return true;
            else return false;
        }
        private bool CheckFirstName()
        {
            FirstName.Trim();
            if (FirstName == String.Empty) 
            {
                MessageBox.Show("Укажите имя сотрудника");
                return false;
            } 
            else return true;
        }
        private bool CheckLastName()
        {
            LastName.Trim();
            if (LastName == String.Empty)
            {
                MessageBox.Show("Укажите фамилию сотрудника");
                return false;
            }
            else return true;
        }
        private bool CheckGender()
        {
            if (EmployeerGender == 0)
            {
                MessageBox.Show("Нужно указать пол нового сотрудника");
                return false;
            }
            else return true;
        }
        private bool CheckDepartment()
        {
            if (employeersDepartmentID == 0)
            {
                MessageBox.Show("Необходимо указать отдел");
                return false;
            }
            else return true;
        }
        #endregion

        private void DeleteEmp()
        {
            dbContext.Employees.Remove(dbContext.Employees.Find(selectedEmployeerID));
            dbContext.SaveChanges();

            UpdateDataAndCleanProperties();
        }
        private void SaveNewEmp()
        {
            if(CheckAllEmpDataProperties()) 
            {
                var newEmployee = new Employees(FirstName, LastName, Patronymic, EmployeerGender, BirthDate, employeersDepartmentID);

                dbContext.Employees.Add(newEmployee);
                dbContext.SaveChanges();

                UpdateDataAndCleanProperties();
            }
        }
        private void UpdateEmp()
        {
                var entity = dbContext.Employees.Find(selectedEmployeerID);
                entity.firstname = FirstName;
                entity.lastname = LastName;
                entity.patronymic = Patronymic;
                entity.gender = EmployeerGender;
                entity.dateOfBirth = BirthDate;
                entity.depID = employeersDepartmentID;
                dbContext.SaveChanges();

            UpdateDataAndCleanProperties();  
        }
        private void CleanEmployeeProperties()
        {
            isExistingEmployee = false;
            FirstName = LastName = Patronymic = String.Empty;
            selectedEmployeerID = 0;
            DepartmentSelectedForEmployee = null;
            selectedEmp = null;
            EmployyerSaveButtonStatus = SaveButtonStatus.Сохранить;
        }
        private void CreateEmployeerTabCommands()
        {
            EmployeerSaveButtonCommand = new LambdaCommand(OnEmpSaveButtonCommandExecuted, CanEmpSaveButtonCommandExecute);
            EmployeerSelectedInDataGrid = new LambdaCommand(OnEmpSelectedInDataGridExecuted, CanEmpSelectedInDataGridExecute);
            EmployeerCancelButtonCommand = new LambdaCommand(OnEmployeerCancelButtonExecuted, CanEmployeerCancelButtonExecute);
            DeleteEmployeerButtonCommand = new LambdaCommand(OnDeleteEmployeerButtonExecuted, CanDeleteEmployeerButtonExecute);
        }
        private void DownloadDataForEmploeersDataGrid()
        {
            var freshEmpList = new ObservableCollection<EmployeerOfTheDep>();
            dynamic rawdata = LoadRawEmloyeersDataFromDatabase();
            foreach (var source in rawdata)
            {
                freshEmpList.Add(new EmployeerOfTheDep(source.empid, source.firstname, source.lastname, source.patronymic, source.gender, source.dateOfBirth, source.depID, source.depName, source.managerID));
            }
            AllEmployeesWithDepName = freshEmpList;
        }
        private dynamic LoadRawEmloyeersDataFromDatabase()
        {
            dynamic result = (from Employees in dbContext.Employees.ToList()
                              join Departments in dbContext.Departments.ToList()
                              on Employees.depID equals Departments.depID
                              select new
                              { Employees.empid, Employees.firstname, Employees.lastname, Employees.patronymic, Employees.gender, Employees.dateOfBirth, Employees.depID, Departments.depName, Departments.managerID }).ToList();
            return result;
        }
        private void SetEmployeePropertiesFromChoosenEmployee()
        {
            selectedEmployeerID = selectedEmp.empid;
            FirstName = selectedEmp.firstname;
            LastName = selectedEmp.lastname;
            Patronymic = selectedEmp.patronymic;
            EmployeerGender = selectedEmp.gender;
            BirthDate = selectedEmp.dateOfBirth;

            SetDepartmentOfChoosenEmployee();
        }
        private void SetDepartmentOfChoosenEmployee()
        {
            foreach (var dep in _allDepartmentsWithManagerName)
            {
                if (dep.depID == selectedEmp.depID) DepartmentSelectedForEmployee = dep;
            }
        }
    }
}
