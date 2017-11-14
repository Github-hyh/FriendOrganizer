using FriendOrganizer.Model;
using FriendOrganizer.UI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using FriendOrganizer.UI.Event;
using Prism.Events;
using FriendOrganizer.UI.View.Services;
using System.Windows.Input;
using Prism.Commands;

namespace FriendOrganizer.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private Func<IFriendDetailViewModel> _friendDetailVMCreator;
        private IFriendDetailViewModel _friendDetailViewModel;
        private IMessageDialogService _messageDialogService;

        public ICommand CreateNewFriendCommand { get; private set; }

        public INavigationViewModel NavigationViewModel { get; private set; }

        public IFriendDetailViewModel FriendDetailViewModel
        {
            get { return _friendDetailViewModel; }
            set 
            { 
                _friendDetailViewModel = value;
                OnPropertyChanged();
            }
        }        

        public MainViewModel(INavigationViewModel navigationVM, 
            Func<IFriendDetailViewModel> friendDetailVMCreator,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService)
        {
            _friendDetailVMCreator = friendDetailVMCreator;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OpenFriendDetailViewEvent>().Subscribe(OnOpenFriendDetailView);
            _eventAggregator.GetEvent<AfterFriendDeleteEvent>().Subscribe(AfterFriendDeleted);
            _messageDialogService = messageDialogService;

            CreateNewFriendCommand = new DelegateCommand(OnCreateNewFriendExecute);

            NavigationViewModel = navigationVM;
        }
        
        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenFriendDetailView(int? friendId)
        {
            if (FriendDetailViewModel != null && FriendDetailViewModel.HasChanges)
            {
                var result = _messageDialogService
                    .ShowOkCancelDialog("You've made changes. Navigate away?","Question");
                if(result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }
            FriendDetailViewModel = _friendDetailVMCreator();
            await FriendDetailViewModel.LoadAsync(friendId);
        }

        private void OnCreateNewFriendExecute()
        {
            OnOpenFriendDetailView(null);
        }

        private void AfterFriendDeleted(int friendId)
        {
            FriendDetailViewModel = null;
        }

    }
}
