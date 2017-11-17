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
using Autofac.Features.Indexed;

namespace FriendOrganizer.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;
        private IDetailViewModel _detailViewModel;
        private IMessageDialogService _messageDialogService;
        private IIndex<string, IDetailViewModel> _detailViewModelCreator;

        public ICommand CreateNewDetailCommand { get; private set; }

        public INavigationViewModel NavigationViewModel { get; private set; }

        public IDetailViewModel DetailViewModel
        {
            get { return _detailViewModel; }
            set 
            { 
                _detailViewModel = value;
                OnPropertyChanged();
            }
        }        

        public MainViewModel(INavigationViewModel navigationVM, 
            IIndex<string, IDetailViewModel> detailViewModelCreator,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService)
        {
            _detailViewModelCreator = detailViewModelCreator;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OpenDetailViewEvent>().Subscribe(OnOpenDetailView);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);
            _messageDialogService = messageDialogService;

            CreateNewDetailCommand = new DelegateCommand<Type>(OnCreateNewDetailExecute);

            NavigationViewModel = navigationVM;
        }
        
        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenDetailView(OpenDetailViewEventArgs args)
        {
            if (DetailViewModel != null && DetailViewModel.HasChanges)
            {
                var result = _messageDialogService
                    .ShowOkCancelDialog("You've made changes. Navigate away?","Question");
                if(result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }

            DetailViewModel = _detailViewModelCreator[args.ViewModelName];
            await DetailViewModel.LoadAsync(args.Id);
        }

        private void OnCreateNewDetailExecute(Type ViewModel)
        {
            OnOpenDetailView(new OpenDetailViewEventArgs
            {
                ViewModelName = ViewModel.Name
            });
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            DetailViewModel = null;
        }

    }
}
