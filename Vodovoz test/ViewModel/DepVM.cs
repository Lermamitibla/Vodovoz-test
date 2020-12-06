using System;
using Vodovoz_test.Model;
using Vodovoz_test.SupportingClasses;
using Vodovoz_test.Commands;
using System.Windows.Input;
using System.Windows;

namespace Vodovoz_test.ViewModel
{
    class DepVM : MainVM
    {
        private string _depName = String.Empty;
        private int _depManagerID;
        private EmployeerOfTheDep _managerOfSelectedDep;
        private bool isSelectedManagerAreValid;
        private DepWithManagerName selectedDep;

        public DepVM()
        {
            DeptSaveButtonCommand = new LambdaCommand(OnDeptSaveButtonCommandExecuted, CanDeptSaveButtonCommandExecute);
            DepSelectedInDataGrid = new LambdaCommand(OnDepSelectedInDataGridExecuted, CanDepSelectedInDataGridExecute);
            DepCancelButtonCommand = new LambdaCommand(OnDepCancelButtonExecuted, CanDepCancelButtonExecute);
            DepRemoveManagerCommand = new LambdaCommand(OnDepRemoveManagerCommandExecuted, CanDepRemoveManagerCommandExecute);
            DepRemoveCommand = new LambdaCommand(OnDepRemoveCommandExecuted, CanDepRemoveCommandExecute);
        }

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

        #region Команды и их методы
        public ICommand DeptSaveButtonCommand { get; }
        public ICommand DepSelectedInDataGrid { get; }
        public ICommand DepCancelButtonCommand { get; }
        public ICommand DepRemoveManagerCommand { get; }
        public ICommand DepRemoveCommand { get; }


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

                SaveButtonContent = "Изменить";
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
            DBcontext.Departments.Add(new Departments(_depName));
            DBcontext.SaveChanges();

            AllDepWithManagerName = GetAllDepsWithManagerName();
            CancellProperties();
            MessageBox.Show("Депортамент создан");
        }
        private void UpdateDep()
        {
            var entity = DBcontext.Departments.Find(selectedDepId);
            entity.depName = DepName;
            if (_depManagerID == 0 && ManagerOfSelectedDep != null) entity.managerID = _managerOfSelectedDep.empid;
            DBcontext.SaveChanges();

            AllDepWithManagerName = GetAllDepsWithManagerName();
            AllEmployeesWithDepName = GetEmployeersOfTheDep();
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
            var dep = DBcontext.Departments.Find(selectedDepId);
            dep.managerID = null;
            DBcontext.SaveChanges();

            ManagerOfSelectedDep = null;
            CancellProperties();
            AllDepWithManagerName = GetAllDepsWithManagerName();
           // MessageBox.Show("Руководитель смещен");
        }
        private void RemoveDepartment()
        {
            DBcontext.Departments.Remove(DBcontext.Departments.Find(selectedDepId));
            DBcontext.SaveChanges();

            AllDepWithManagerName = GetAllDepsWithManagerName();
            AllEmployeesWithDepName = GetEmployeersOfTheDep();
            CancellProperties();

            //MessageBox.Show("Отдел удолен");
        }
        private void CancellProperties()
        {
            DepName = String.Empty;
            ManagerOfSelectedDep = null;
            AllEmpOfSelectedDep = null;
            selectedDepId = 0;
            SaveButtonContent = "Сохранить";
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

    }
}
