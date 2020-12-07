using Vodovoz_test.Model;

namespace Vodovoz_test.ViewModel
{
    internal enum SaveButtonStatus {Сохранить, Изменить}

    partial class MainVM : BaseVM
    {
        private VodovozTESTEntities dbContext;
        public MainVM()
        {
            dbContext = new VodovozTESTEntities();
            CreateAllCommands();
            DownloadAllDataFromDatabase();
        } 
        private void CreateAllCommands()
        {
            CreateEmployeerTabCommands();
            CreateDepartmentsTabComments();
        }
        private void DownloadAllDataFromDatabase()
        {
            DownloadDataForEmploeersDataGrid();
            DownloadDataForDepartmentsDataGrid();
        }
        private void UpdateDataAndCleanProperties()
        {
            DownloadDataForDepartmentsDataGrid();
            DownloadDataForEmploeersDataGrid();
            if (selectedDep != null) UpdateEmpListForSelectedDep();
            CleanEmployeeProperties();
            CleanDepartmentProperties();
        }
    }
}

