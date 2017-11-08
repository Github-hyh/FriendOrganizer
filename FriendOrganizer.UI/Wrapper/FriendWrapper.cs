using FriendOrganizer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.Wrapper
{
    public class FriendWrapper : ModelWrapper<Friend>
    {
        public FriendWrapper(Friend friend):base(friend)
        {

        }

        public int Id { get { return Model.Id; } }

        public string FirstName
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue<string>(value);
            }
        }

        public string LastName
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue<string>(value);
            }
        }

        public string Email
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue<string>(value);
            }
        }

        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            switch (propertyName.ToLower())
            {
                case "firstname":
                    if(string.Equals(FirstName,"Robot",StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "Robots are not valid friends";
                    }
                    break;
                case "lastname":
                    if (string.IsNullOrWhiteSpace(LastName))
                    {
                        yield return "Last name not be null";
                    }
                    break;
            }
        }
    }
}
