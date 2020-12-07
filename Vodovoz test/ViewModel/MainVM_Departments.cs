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
        private string _depName = String.Empty;
        private int _depManagerID;
        private EmployeerOfTheDep _managerOfSelectedDep;
        private bool isSelectedManagerAreValid;
        private DepWithManagerName selectedDep;
        private int selectedDepId;
        private SaveButtonStatus _departmentSaveButtonStatus = SaveButtonStatus.Сохранить;

        protected ObservableCollection<DepWithManagerName> _allDepWithManagerName;



        public string DepName { get { return _depName; } set { if (!_depName.Equals(value)) _depName = value; OnPropertyChanged(); } }
        public EmployeerOfTheDep ManagerOfSelectedDep 
        { 
            get { return _managerOfSelectedDep; } 
            set 
            {
                /*if(!_managerOfSelectedDep.Equals(value)) */
                if (value != null)
                {
                    if (value.empid == _depManagerID) isSelectedManagerAreValid = true;
                    else isSelectedManagerAreValid = false;
                }
                else isSelectedManagerAreValid = false;
                _managerOfSelectedDep = value;
                OnPropertyChanged();
            } 
        } //была ошибка со ссылкой на нул при переключении на отдел без менеджера, помогло удаление проверки Equals

        public ObservableCollection<DepWithManagerName> AllDepWithManagerName
        {
            get { return _allDepWithManagerName; }
            set { _allDepWithManagerName = value; OnPropertyChanged(); }
        }
        public SaveButtonStatus DepartmentSaveButtonStatus { get { return _employyerSaveButtonStatus; } set { if (!_firstName.Equals(value)) _employyerSaveButtonStatus = value; OnPropertyChanged(); } }

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
                if (selectedDepId == 0) SaveNewDep();
                else UpdateDep();
            }
        }
        private bool CanDeptSaveButtonCommandExecute(object p) => true;

        private void OnDepSelectedInDataGridExecuted(object p)
        {
            if (p != null)
            {
                selectedDep = (DepWithManagerName)p;
                selectedDepId = selectedDep.depID;
                UpdateEmpListForSelectedDep(selectedDepId);
                
                if (selectedDep.managerID != null)
                {
                    _depManagerID = (int)selectedDep.managerID;
                    ManagerOfSelectedDep = GetManagerOfTheSelectedDep(_depManagerID);
                }
                else _depManagerID = 0;

                DepartmentSaveButtonStatus = SaveButtonStatus.Сохранить;
                DepName = selectedDep.depName;
            }
        }
        private bool CanDepSelectedInDataGridExecute(object p) => true;

        private void OnDepCancelButtonExecuted(object p)
        {
            selectedDepId = 0;
            AllEmpOfSelectedDep = null;
            ManagerOfSelectedDep = null;
            DepName = String.Empty;
        }
        private bool CanDepCancelButtonExecute(object p) => true;

        private void OnDepRemoveManagerCommandExecuted(object p)
        {
            if (ManagerOfSelectedDep == null || !isSelectedManagerAreValid) MessageBox.Show("У отдела нет руководителя");
            else RemoveManager();
        }
        private bool CanDepRemoveManagerCommandExecute(object p)
        {
            if (isSelectedManagerAreValid) return true;
            else return false;
        }

        private void OnDepRemoveCommandExecuted(object p)
        {
            bool IsEverythingAllright = true;

            if (_depManagerID != 0) 
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
            if (AllDepWithManagerName.Count == 0 || selectedDep == null || selectedDepId == 0) return false;
            else return true;
        }
        #endregion

        private void SaveNewDep()
        {
            dbContext.Departments.Add(new Departments(_depName));
            dbContext.SaveChanges();

            DownloadDataForDepartmentsDataGrid();
            CancellProperties();
            MessageBox.Show("Депортамент создан");
        }
        private void UpdateDep()
        {
            var entity = dbContext.Departments.Find(selectedDepId);
            entity.depName = DepName;
            if (_depManagerID == 0 && ManagerOfSelectedDep != null) entity.managerID = _managerOfSelectedDep.empid;
            dbContext.SaveChanges();

            DownloadDataForDepartmentsDataGrid();
            DownloadDataForEmploeersDataGrid();
            CancellProperties();
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
        private void RemoveManager()
        {
            var dep = dbContext.Departments.Find(selectedDepId);
            dep.managerID = null;
            dbContext.SaveChanges();

            ManagerOfSelectedDep = null;
            CancellProperties();
            DownloadDataForDepartmentsDataGrid();
            // MessageBox.Show("Руководитель смещен");
        }
        private void RemoveDepartment()
        {
            dbContext.Departments.Remove(dbContext.Departments.Find(selectedDepId));
            dbContext.SaveChanges();

            DownloadDataForDepartmentsDataGrid();
            DownloadDataForEmploeersDataGrid();
            CancellProperties();

            //MessageBox.Show("Отдел удолен");
        }
        private void CancellProperties()
        {
            DepName = String.Empty;
            ManagerOfSelectedDep = null;
            AllEmpOfSelectedDep = null;
            selectedDepId = 0;
            DepartmentSaveButtonStatus = SaveButtonStatus.Изменить;
        }

        private bool CheckPropertiesBeforeCreateOrUpdate()
        {
            if (DepName == String.Empty)
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

            AllDepWithManagerName = freshDepWithManagerNameList;
        }

    }
}
