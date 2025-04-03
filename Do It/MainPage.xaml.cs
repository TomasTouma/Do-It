using System.Collections.ObjectModel;
using System.Text.Json;


namespace Do_It
{
    public partial class MainPage : ContentPage
    {
        //Two collection, one for all tasks and one for completed tasks
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<TaskItem> MarkedTasks { get; set; } = new ObservableCollection<TaskItem>();
      
        public MainPage()
        {           
            InitializeComponent();
            InitialiseSettings();          
            TaskListView.ItemsSource = MarkedTasks;
            CompletedSwitch.Toggled += CompletedSwitch_Toggled;
            FilterTasks();
            UpdateFooterVisibility();
        }
       
        //method to capitalize the first letter in the task entry
        private void TaskEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TaskEntry.Text))
            {
                TaskEntry.Text = char.ToUpper(TaskEntry.Text[0]) + TaskEntry.Text.Substring(1);
            }
        }
        //method to make the list footer visible when there is no tasks in the list
        private void UpdateFooterVisibility()
        {
            if (TaskListView.Footer is Label footerLabel)
            {
                footerLabel.IsVisible = Tasks.Count == 0;
            }
        }
        //method to add entered tasks from TaskEntry to collection and display it in the list
        private void AddButton_Clicked(object sender, EventArgs e)
        {
            
            if (!string.IsNullOrWhiteSpace(TaskEntry.Text))
            {
                Tasks.Add(new TaskItem { TaskText = TaskEntry.Text, IsCompleted = false });
                TaskEntry.Text = string.Empty;                
            }
            WriteResultsJSON(Tasks, "tasks.json");
            TaskEntry.Focus();
            FilterTasks();
            UpdateFooterVisibility();
        }
        //method to delete a task from the collection and remove it from the list
        private async void TaskDeleteButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Delete Task?", "Would you like to delete the task", "Yes", "Cancel");
            
            if (answer==true)
            {
                if (sender is ImageButton button && button.BindingContext is TaskItem task)
                {
                    Tasks.Remove(task); 
                    FilterTasks();
                    WriteResultsJSON(Tasks, "tasks.json");
                    UpdateFooterVisibility();
                }
            }
            else { }
                       
        }
        //method to delete clear the task collection, eather the completed tasks only or all tasks based on the switch status
        private async void MultiDeleteButton_Clicked(object sender, EventArgs e)
        {
            if (CompletedSwitch.IsToggled && MarkedTasks.Count>0)
            {               
                bool answer = await DisplayAlert("Delete completed tasks?", "Would you like to delete all completed tasks", "Yes", "Cancel");

                if (answer == true)
                {
                    for (int i = Tasks.Count - 1; i >= 0; i--)
                    {
                        if (Tasks[i].IsCompleted)
                        {
                            Tasks.RemoveAt(i);
                        }
                    }

                    FilterTasks();
                    WriteResultsJSON(Tasks, "tasks.json");
                    UpdateFooterVisibility();

                }
                else { }
            }
            else if(!CompletedSwitch.IsToggled && Tasks.Count > 0)
            {
                bool answer = await DisplayAlert("Delete all tasks?", "Would you like to delete all the tasks", "Yes", "Cancel");

                if (answer == true)
                {
                    Tasks.Clear();
                    FilterTasks();
                    WriteResultsJSON(Tasks, "tasks.json");
                    UpdateFooterVisibility();
                }
                else { }
            }
        }
        //calls the FilterTasks method so it shows only the completed tasks
        private void CompletedSwitch_Toggled(object sender, EventArgs e)
        {
            FilterTasks();             
        }
        //this method clear the completed tasks list first every time its called, and add every completed task to the list again so its up to date always
        //and its called almost every where so it always update the list when the comleted switch is toggled it shows only the competed tasks
        private void FilterTasks()
        {
            MarkedTasks.Clear(); 
            if (CompletedSwitch.IsToggled)
            {
                foreach (var task in Tasks)
                {
                    if (task.IsCompleted)
                    {
                        MarkedTasks.Add(task);
                    }
                }
            }
            else
            {                
                foreach (var task in Tasks)
                {
                    MarkedTasks.Add(task);
                }
            }
        }
        //save the list to a json file
        private static void WriteResultsJSON(ObservableCollection<TaskItem> tasks, string fileName)
        {
            string jsonarray = JsonSerializer.Serialize(tasks);
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using FileStream outputStream = File.Create(targetFile);
            using StreamWriter streamWriter = new StreamWriter(outputStream);
            streamWriter.Write(jsonarray);
        }
        //read the tasks from json file
        private static ObservableCollection<TaskItem> ReadResultsJSON(string fileName)
        {
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            ObservableCollection<TaskItem> tasks;
            if (File.Exists(targetFile))
            {
                using FileStream outputStream = File.OpenRead(targetFile);
                using StreamReader streamReader = new StreamReader(outputStream);
                string jsonstring = streamReader.ReadToEnd();
                tasks = JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(jsonstring);
            }
            else
            {
                tasks = new ObservableCollection<TaskItem>();
            }
            return tasks;
        }

        private void InitialiseSettings()
        {
            if (File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, "tasks.json")))
            {
                Tasks = ReadResultsJSON("tasks.json");
            }
            else
            {
                Tasks = new ObservableCollection<TaskItem>();
            }          
            FilterTasks();
        }
        //this update the tasks text that are already in the list when clicked on one of them
        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is TaskItem task)
            {
                task.TaskText = e.NewTextValue; 
                WriteResultsJSON(Tasks, "tasks.json");
            }
        }
        //this update the task status if its checked or not 
        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TaskItem task)
            {
                task.IsCompleted = e.Value;               
                WriteResultsJSON(Tasks, "tasks.json");
            }
        }       
    }
}
