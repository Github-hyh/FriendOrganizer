using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.ViewModel
{
    public interface IFriendDetailViewModel
    {
        Task LoadAsync(int friendId);
        bool HasChanges { get; }
    }
}
