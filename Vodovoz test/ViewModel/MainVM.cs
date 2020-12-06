using System.Linq;
using Vodovoz_test.Model;
using Vodovoz_test.SupportingClasses;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vodovoz_test.ViewModel
{
    class MainVM : BaseVM
    {
        
        protected static int selectedDepId;

        protected string _saveButtonContent = "Сохранить";
        protected static ObservableCollection<EmployeerOfTheDep> _allEmployeesWithDepNam;
        protected static ObservableCollection<DepWithManagerName> _allDepWithManagerName;
        private static VodovozTESTEntities _dbContext = new VodovozTESTEntities();
        protected static ObservableCollection<EmployeerOfTheDep> _allEmpOfSelectedDep;
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        static MainVM()
        {
            _allEmployeesWithDepNam = GetEmployeersOfTheDep();
            _allDepWithManagerName = GetAllDepsWithManagerName();
        }

        protected VodovozTESTEntities DBcontext { get { return _dbContext; } }
        public static ObservableCollection<EmployeerOfTheDep> AllEmployeesWithDepName
        {
            get { return _allEmployeesWithDepNam; }
            set { if (!_allEmployeesWithDepNam.Equals(value)) _allEmployeesWithDepNam = value; 
                OnStaticPropertyChanged(); }
        }
        public static ObservableCollection<DepWithManagerName> AllDepWithManagerName
        {
            get { return _allDepWithManagerName; }
            set {  _allDepWithManagerName = value; OnStaticPropertyChanged(); }
        }
        public string SaveButtonContent { get { return _saveButtonContent; } protected set { if (!_saveButtonContent.Equals(value)) _saveButtonContent = value; OnPropertyChanged(); } }

        internal static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        protected static void UpdateEmpListForSelectedDep()
        {
            if (selectedDepId != 0) UpdateEmpListForSelectedDep(selectedDepId);
        }
        protected static void UpdateEmpListForSelectedDep(int depId)
        {
            selectedDepId = depId;
            AllEmpOfSelectedDep = GetEmpListFromSelectedDep(selectedDepId);
        }

        private static ObservableCollection<EmployeerOfTheDep> GetEmpListFromSelectedDep(int depID)
        {
            var result = new ObservableCollection<EmployeerOfTheDep>();
            foreach (var emp in AllEmployeesWithDepName)
            {
                if (emp.depID == depID) result.Add(emp);
            }
            return result;
        }
        public static ObservableCollection<EmployeerOfTheDep> AllEmpOfSelectedDep
        {
            get { return _allEmpOfSelectedDep; } set { _allEmpOfSelectedDep = value; OnStaticPropertyChanged(); }
        }
        protected static ObservableCollection<DepWithManagerName> GetAllDepsWithManagerName()
        {
            var freshDepWithManagerNameList = new ObservableCollection<DepWithManagerName>();
            //поскольку есть возможность создавать отделы без руководителя, то для получения всех отделов с фио руководителя (или без) нужно применить внешнее соединение left join, которе не поддерживается в LINQ. Поэтому идет прямой запрос в БД. 
            string sqlcommand = "select D.depID, depName, managerID, case when managerID is not null then CONCAT(lastname, ' ', firstname, ' ', patronymic) else 'руководитель не назначен' end as managerfullname from Departments as D left join Employees as E on D.managerID = E.empid; ";

                var result = _dbContext.Database.SqlQuery(typeof(DepWithManagerName), sqlcommand);

                foreach (var a in result)
                {
                    freshDepWithManagerNameList.Add((DepWithManagerName)a);
                }

            return freshDepWithManagerNameList;
        }
        private static dynamic LoadEmployeersOfTheDep()
        {
            dynamic result = (from Employees in _dbContext.Employees.ToList()
                              join Departments in _dbContext.Departments.ToList()
                              on Employees.depID equals Departments.depID
                              select new
                              { Employees.empid, Employees.firstname, Employees.lastname, Employees.patronymic, Employees.gender, Employees.dateOfBirth, Employees.depID, Departments.depName, Departments.managerID }).ToList();
            return result;
        }
        protected static ObservableCollection<EmployeerOfTheDep> GetEmployeersOfTheDep()
        {
            var freshEmpList = new ObservableCollection<EmployeerOfTheDep>();
            dynamic rawdata = LoadEmployeersOfTheDep();
            foreach (var source in rawdata)
            {
                freshEmpList.Add(new EmployeerOfTheDep(source.empid, source.firstname, source.lastname, source.patronymic, source.gender, source.dateOfBirth, source.depID, source.depName, source.managerID));
            }
            return freshEmpList;
        }

    }
}

