﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module7DataAccess.Model;
using Module7DataAccess.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace Module7DataAccess.ViewModel
{
    public class PersonalViewModel : INotifyPropertyChanged
    {
        private readonly PersonalService _personalService;
        public ObservableCollection<Personal> PersonalList { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }

        }

        private Personal _selectedPersonal;
        public Personal SelectedPersonal
        {
            get => _selectedPersonal;
            set
            {
                _selectedPersonal = value;
                if (_selectedPersonal != null)
                {
                    NewPersonalName = _selectedPersonal.Name;
                    NewPersonalGender = _selectedPersonal.Gender;
                    NewPersonalContactNo = _selectedPersonal.ContactNo;

                    IsPersonSelected = true;
                }
                else
                {
                    IsPersonSelected = false;
                }
                OnPropertyChanged();
            }
        }

        private bool _isPersonSelected;
        public bool IsPersonSelected
        {
            get => _isPersonSelected;
            set
            {
                _isPersonSelected = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // New Personal entry for name, gender, contact no
        private string _newPersonalName;
        public string NewPersonalName
        {
            get => _newPersonalName;
            set
            {
                _newPersonalName = value;
                OnPropertyChanged();
            }
        }

        private string _newPersonalGender;
        public string NewPersonalGender
        {
            get => _newPersonalGender;
            set
            {
                _newPersonalGender = value;
                OnPropertyChanged();
            }
        }

        private string _newPersonalContactNo;
        public string NewPersonalContactNo
        {
            get => _newPersonalContactNo;
            set
            {
                _newPersonalContactNo = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddPersonalCommand { get; }
        public ICommand SelectedPersonCommand { get; }
        public ICommand DeletePersonCommand { get; }

        //PersonalViewModel Contructor

        public PersonalViewModel()
        {
            _personalService = new PersonalService();
            PersonalList = new ObservableCollection<Personal>();
            LoadDataCommand = new Command(async () => await LoadData());
            AddPersonalCommand = new Command(async () => await AddPerson());
            SelectedPersonCommand = new Command<Personal>(person => SelectedPersonal = person);
            DeletePersonCommand = new Command(async() => 
                                  await DeletePersonal(), () =>
                                  SelectedPersonal != null);

            LoadData();
        }

        public async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Loading personal data...";
            try
            {
                var personals = await _personalService.GetAllPersonalsAsync();
                PersonalList.Clear();
                foreach (var personal in personals)
                {
                    PersonalList.Add(personal);
                }
                StatusMessage = "Data load successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddPerson()
        {
            if(IsBusy || string.IsNullOrWhiteSpace(NewPersonalName) || string.IsNullOrWhiteSpace(NewPersonalGender) || string.IsNullOrWhiteSpace(NewPersonalContactNo))
                {
                    StatusMessage = "Please fill in all fields before adding";
                    return;
                }
            IsBusy = true;
            StatusMessage = "Adding new person...";

            try
            {
                var newPerson = new Personal
                {
                    Name = NewPersonalName,
                    Gender = NewPersonalGender,
                    ContactNo = NewPersonalContactNo
                };
                var isSuccess = await _personalService.AddPersonalAsync(newPerson);
                if (isSuccess)
                {
                    NewPersonalName = string.Empty;
                    NewPersonalGender = string.Empty;
                    NewPersonalContactNo = string.Empty;
                    StatusMessage = "New person added successfully";
                }
                else
                {
                    StatusMessage = "Failed to add the new person";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed adding person: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task DeletePersonal()
        {
            if (SelectedPersonal == null) return;
            var answer = await Application.Current.MainPage.DisplayAlert
                ("Confirm Delete", $"Are you sure you want to delete {SelectedPersonal.Name}?", 
                "Yes", "No");

            if (!answer) return;

            IsBusy = true;
            StatusMessage = "Deleting person..";

            try
            {
                var success = await _personalService.DeletePersonalAsync(SelectedPersonal.ID);
                StatusMessage = success ? "Person deleted successfully!" : "Failed to delete person";

                if (success)
                {
                    PersonalList.Remove(SelectedPersonal);
                    SelectedPersonal = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting person: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberNameAttribute] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
