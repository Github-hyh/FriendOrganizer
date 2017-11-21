using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.View.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FriendOrganizer.UI.Wrapper;
using System.ComponentModel;
using Prism.Commands;
using System.Windows.Input;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.ViewModel
{
    public class ProgrammingLanguageDetailViewModel : DetailViewModelBase
    {
        private IProgrammingLanguageRepository _programmingLanguageRepository;

        public ObservableCollection<ProgrammingLanguageWrapper> ProgrammingLanguages { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public ICommand AddCommand { get; private set; }

        private ProgrammingLanguageWrapper _selectedProgrammingLanguage;

        public ProgrammingLanguageWrapper SelectedProgrammingLanguage
        {
            get { return _selectedProgrammingLanguage; }
            set
            {
                _selectedProgrammingLanguage = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveCommand).RaiseCanExecuteChanged();
            }
        }


        public ProgrammingLanguageDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IProgrammingLanguageRepository programmingLanguageRepository) :
            base(eventAggregator, messageDialogService)
        {
            _programmingLanguageRepository = programmingLanguageRepository;
            Title = "Programming Languages";

            ProgrammingLanguages = new ObservableCollection<ProgrammingLanguageWrapper>();

            AddCommand = new DelegateCommand(OnAddExecute);
            RemoveCommand = new DelegateCommand(OnRemoveExecute, OnRemoveCanExecute);
        }

        public async override Task LoadAsync(int id)
        {
            Id = id;

            foreach (var wrapper in ProgrammingLanguages)
            {
                wrapper.PropertyChanged -= wrapper_PropertyChanged;
            }
            ProgrammingLanguages.Clear();

            var languages = await _programmingLanguageRepository.GetAllAsync();
            foreach (var model in languages)
            {
                var wrapper = new ProgrammingLanguageWrapper(model);
                wrapper.PropertyChanged += wrapper_PropertyChanged;
                ProgrammingLanguages.Add(wrapper);
            }
        }

        void wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges)
            {
                HasChanges = _programmingLanguageRepository.HasChanges();
            }
            if (e.PropertyName == "HasErrors")
            {
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        protected override void OnDeleteExecute()
        {
            //Not need
        }

        protected override bool OnSaveCanExecute()
        {
            return HasChanges
                && ProgrammingLanguages.All(p => !p.HasErrors);
        }

        protected async override void OnSaveExecute()
        {
            bool success = true;
            try
            {
                await _programmingLanguageRepository.SaveAsync();
                HasChanges = _programmingLanguageRepository.HasChanges();
                RaiseCollectionSavedEvent();
            }
            catch(Exception ex)
            {
                success = false;
                while(ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                MessageDialogService.ShowInfoDialog(string.Format("Error while saving the entities, the data will be reloaded. Details: {0}", ex.Message));
            }

            if (!success)
            {
                await LoadAsync(Id);
            }
        }

        private bool OnRemoveCanExecute()
        {
            return SelectedProgrammingLanguage != null;
        }

        private async void OnRemoveExecute()
        {
            var isReferenced = await _programmingLanguageRepository.IsReferencedByFriendAsync(SelectedProgrammingLanguage.Id);
            if (isReferenced)
            {
                MessageDialogService.ShowInfoDialog(string.Format("The language {0} can't be removed, as it is referenced by at least one friend", SelectedProgrammingLanguage.Name));
                return;
            }

            SelectedProgrammingLanguage.PropertyChanged -= wrapper_PropertyChanged;
            _programmingLanguageRepository.Remove(SelectedProgrammingLanguage.Model);
            ProgrammingLanguages.Remove(SelectedProgrammingLanguage);
            SelectedProgrammingLanguage = null;
            HasChanges = _programmingLanguageRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private void OnAddExecute()
        {
            var newLanguage = new ProgrammingLanguageWrapper(new ProgrammingLanguage());
            newLanguage.PropertyChanged += wrapper_PropertyChanged;
            _programmingLanguageRepository.Add(newLanguage.Model);
            ProgrammingLanguages.Add(newLanguage);

            //Trigger the validation
            newLanguage.Name = "";
        }

    }
}
