﻿using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using FriendOrganizer.UI.Event;
using System.Windows.Input;
using Prism.Commands;
using FriendOrganizer.UI.Wrapper;
using FriendOrganizer.UI.View.Services;
using FriendOrganizer.UI.Data.Lookups;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FriendOrganizer.UI.ViewModel
{
    public class FriendDetailViewModel : DetailViewModelBase, IFriendDetailViewModel
    {
        private IFriendRepository _friendRespository;
        private FriendWrapper _friend;
        private IProgrammingLanguageLookupDataService _programmingLanguageLookupDataService;

        public ICommand RemovePhoneNumberCommand { get; private set; }
        public ICommand AddPhoneNumberCommand { get; private set; }

        public ObservableCollection<LookupItem> ProgrammingLanguages { get; private set; }
        public ObservableCollection<FriendPhoneNumberWrapper> PhoneNumbers { get; private set; }

        public FriendWrapper Friend
        {
            get
            {
                return _friend;
            }
            private set
            {
                _friend = value;
                OnPropertyChanged();
            }
        }

        private FriendPhoneNumberWrapper _selectedPhoneNumber;

        public FriendPhoneNumberWrapper SelectedPhoneNumber
        {
            get { return _selectedPhoneNumber; }
            set
            {
                _selectedPhoneNumber = value;
                OnPropertyChanged();
                ((DelegateCommand)RemovePhoneNumberCommand).RaiseCanExecuteChanged();
            }
        }

        public FriendDetailViewModel(IFriendRepository friendDataService,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IProgrammingLanguageLookupDataService programmingLanguageLookupDataService) :
            base(eventAggregator, messageDialogService)
        {
            _friendRespository = friendDataService;
            _programmingLanguageLookupDataService = programmingLanguageLookupDataService;

            eventAggregator.GetEvent<AfterCollectionSavedEvent>()
                .Subscribe(AfterCollectionSaved);

            AddPhoneNumberCommand = new DelegateCommand(OnAddPhoneNumberExecute);
            RemovePhoneNumberCommand = new DelegateCommand(OnRemovePhoneNumberExecute, OnRemovePhoneNumberCanExecute);

            ProgrammingLanguages = new ObservableCollection<LookupItem>();
            PhoneNumbers = new ObservableCollection<FriendPhoneNumberWrapper>();
        }

        private async void AfterCollectionSaved(AfterCollectionSavedEventArgs args)
        {
            if (args.ViewModelName == Constants.ProgrammingLanguageDetailViewModel)
            {
                await LoadProgrammingLanguagesLookupAsync();
            }
        }

        private bool OnRemovePhoneNumberCanExecute()
        {
            return SelectedPhoneNumber != null;
        }

        private void OnRemovePhoneNumberExecute()
        {
            SelectedPhoneNumber.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;
            _friendRespository.RemovePhoneNumber(SelectedPhoneNumber.Model);
            PhoneNumbers.Remove(SelectedPhoneNumber);
            SelectedPhoneNumber = null;
            HasChanges = _friendRespository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private void OnAddPhoneNumberExecute()
        {
            var newNumber = new FriendPhoneNumberWrapper(new FriendPhoneNumber());
            newNumber.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;
            PhoneNumbers.Add(newNumber);
            Friend.Model.PhoneNumbers.Add(newNumber.Model);
            newNumber.Number = ""; // Trigger validation :-)
        }

        protected override async void OnDeleteExecute()
        {
            if (await _friendRespository.HasMeetingsAsync(Friend.Id))
            {
                MessageDialogService.ShowInfoDialog(string.Format("{0} {1} can't be deleted, as this friend is part of at least one meeting", Friend.FirstName, Friend.LastName));
                return;
            }
            var result = MessageDialogService.ShowOkCancelDialog(string.Format("Do you really want to delete {0} {1} ?", Friend.FirstName, Friend.LastName), "Question");
            if (result == MessageDialogResult.Ok)
            {
                _friendRespository.Remove(Friend.Model);
                await _friendRespository.SaveAsync();
                RaiseDetailDeletedEvent(Friend.Id);
            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Friend != null
                && !Friend.HasErrors
                && PhoneNumbers.All(pn => !pn.HasErrors)
                && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            await SaveWithOptimisticConcurrencyAsync(_friendRespository.SaveAsync,
                () =>
                {
                    HasChanges = _friendRespository.HasChanges();
                    Id = Friend.Id;
                    RaiseDetailSavedEvent(Friend.Id, Friend.FirstName + " " + Friend.LastName);

                });
        }

        public override async Task LoadAsync(int friendId)
        {
            var friend = friendId > 0
                ? await _friendRespository.GetByIdAsync(friendId)
                : CreateNewFriend();

            Id = friendId;

            InitializeFriend(friend);

            InitializeFriendPhoneNumbers(friend.PhoneNumbers);

            await LoadProgrammingLanguagesLookupAsync();
        }

        private void InitializeFriendPhoneNumbers(ICollection<FriendPhoneNumber> phoneNumbers)
        {
            foreach (var wrapper in PhoneNumbers)
            {
                wrapper.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;
            }
            PhoneNumbers.Clear();

            foreach (var friendPhoneNumber in phoneNumbers)
            {
                var wrapper = new FriendPhoneNumberWrapper(friendPhoneNumber);
                wrapper.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;
                PhoneNumbers.Add(wrapper);
            }
        }

        private void FriendPhoneNumberWrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges)
            {
                HasChanges = _friendRespository.HasChanges();
            }
            if (e.PropertyName == "HasErrors")
            {
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        private void InitializeFriend(Model.Friend friend)
        {
            Friend = new FriendWrapper(friend);
            Friend.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                {
                    HasChanges = _friendRespository.HasChanges();
                }
                if (e.PropertyName == "HasErrors")
                {
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }
                if (e.PropertyName == "FirstName"
                    || e.PropertyName == "LastName")
                {
                    SetTitle();
                }
            };
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            if (Friend.Id == 0)
            {
                //Little trick to trigger the validation
                Friend.FirstName = "";
            }

            SetTitle();
        }

        private void SetTitle()
        {
            this.Title = string.Format("{0} {1}", Friend.FirstName, Friend.LastName);
        }

        private Friend CreateNewFriend()
        {
            var friend = new Friend();
            _friendRespository.Add(friend);
            return friend;
        }

        private async Task LoadProgrammingLanguagesLookupAsync()
        {
            ProgrammingLanguages.Clear();
            ProgrammingLanguages.Add(new NullLookupItem { DisplayMember = " - " });
            var lookup = await _programmingLanguageLookupDataService.GetProgrammingLanguageLookupAsync();
            foreach (var lookupItem in lookup)
            {
                ProgrammingLanguages.Add(lookupItem);
            }
        }
    }
}
