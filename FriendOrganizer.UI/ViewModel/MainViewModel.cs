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
        private IDetailViewModel _selectedDetailViewModel;
        private IMessageDialogService _messageDialogService;
        private IIndex<string, IDetailViewModel> _detailViewModelCreator;

        public ICommand CreateNewDetailCommand { get; private set; }
        public ICommand OpenSingleDetailViewCommand { get; private set; }

        public INavigationViewModel NavigationViewModel { get; private set; }

        public ObservableCollection<IDetailViewModel> DetailViewModels { get; private set; }

        public IDetailViewModel SelectedDetailViewModel
        {
            get { return _selectedDetailViewModel; }
            set 
            {
                _selectedDetailViewModel = value;
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
            _eventAggregator.GetEvent<AfterDetailClosedEvent>().Subscribe(AfterDetailClosed);
            _messageDialogService = messageDialogService;

            DetailViewModels = new ObservableCollection<IDetailViewModel>();

            CreateNewDetailCommand = new DelegateCommand<Type>(OnCreateNewDetailExecute);
            OpenSingleDetailViewCommand = new DelegateCommand<Type>(OnOpenSingleDetailViewExecute);

            NavigationViewModel = navigationVM;
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenDetailView(OpenDetailViewEventArgs args)
        {
            var detailViewModel = DetailViewModels.SingleOrDefault(vm =>
                vm.Id == args.Id && vm.GetType().Name == args.ViewModelName);

            if (detailViewModel == null)
            {
                detailViewModel = _detailViewModelCreator[args.ViewModelName];

                bool success = true;
                try
                {
                    await detailViewModel.LoadAsync(args.Id);
                }
                catch
                {
                    success = false;
                    _messageDialogService.ShowInfoDialog("Could not load the entity, " +
                        "maybe it was deleted in the meantime by another user. " +
                        "The navigation is refreshed for you.");
                }
                if (!success)
                {
                    await NavigationViewModel.LoadAsync();
                    return;
                }

                DetailViewModels.Add(detailViewModel);
            }

            SelectedDetailViewModel = detailViewModel;
        }

        private int nextNewItemId = 0;
        private void OnCreateNewDetailExecute(Type ViewModel)
        {
            OnOpenDetailView(new OpenDetailViewEventArgs
            {
                Id = nextNewItemId--,
                ViewModelName = ViewModel.Name
            });
        }

        private void OnOpenSingleDetailViewExecute(Type ViewModel)
        {
            OnOpenDetailView(new OpenDetailViewEventArgs
                {
                    Id = -1,
                    ViewModelName = ViewModel.Name
                });
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        private void AfterDetailClosed(AfterDetailClosedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        private void RemoveDetailViewModel(int id, string viewModelName)
        {
            var detailViewModel = DetailViewModels
                   .SingleOrDefault(vm => vm.Id == id
                   && vm.GetType().Name == viewModelName);
            if (detailViewModel != null)
            {
                DetailViewModels.Remove(detailViewModel);
            }
        }
    }
}
